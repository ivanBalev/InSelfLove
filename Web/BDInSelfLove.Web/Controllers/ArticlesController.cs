namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;
    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Articles;
    using BDInSelfLove.Services.Data.CloudinaryServices;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.Controllers.Helpers;
    using BDInSelfLove.Web.InputModels.Article;
    using BDInSelfLove.Web.ViewModels.Article;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class ArticlesController : PaginationHelper
    {
        private readonly ICloudinaryService cloudinaryService;
        private readonly IArticleService articleService;

        public ArticlesController(
            ICloudinaryService cloudinaryService,
            IArticleService articleService)
            : base(articleService)
        {
            this.cloudinaryService = cloudinaryService;
            this.articleService = articleService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            return this.View(await this.GetArticlesPreview(page));
        }

        [HttpGet]
        [Route("Articles/{slug}")]
        public async Task<IActionResult> Single(string slug)
        {
            // Get info for client
            var viewModel = AutoMapperConfig.MapperInstance
                .Map<ArticleViewModel>(await this.articleService
                .GetBySlug(slug));

            // Return 404 if article doesn't exist
            if (viewModel == null)
            {
                return this.NotFound();
            }

            // Adjust comments CreatedOn to user's local time
            foreach (var comment in viewModel.Comments)
            {
                comment.CreatedOn = TimezoneHelper.ToLocalTime(comment.CreatedOn, this.UserTimezoneIdFromCookie);
            }

            return this.View(viewModel);
        }

        // Admin acces only below
        [HttpGet]
        [Route("Articles/Create")]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public IActionResult Create()
        {
            return this.View();
        }

        [HttpPost]
        [Route("Articles/Create")]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public async Task<IActionResult> Create(ArticleCreateInputModel inputModel)
        {
            //int syllablesResult = await this.EnterContentSyllables(inputModel);
            await this.SetArticlePhoto(inputModel);

            string slug = await this.articleService
                .Create(AutoMapperConfig.MapperInstance.Map<Article>(inputModel));

            //if (syllablesResult != 1)
            //{
            //    this.TempData["StatusMessage"] = "Syllabification error";
            //}

            return this.RedirectToAction("Single", new { slug });
        }

        [HttpGet]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await this.articleService.GetById(id)
                .To<ArticleEditInputModel>().FirstOrDefaultAsync();

            return this.View(model);
        }

        [HttpPost]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public async Task<IActionResult> Edit(ArticleEditInputModel inputModel)
        {
            await this.SetArticlePhoto(inputModel);

            string slug = await this.articleService
                .Edit(AutoMapperConfig.MapperInstance.Map<Article>(inputModel));

            return this.RedirectToAction("Single", new { slug });
        }

        [HttpPost]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public async Task<IActionResult> Delete(int id)
        {
            await this.articleService.Delete(id);

            return this.Redirect("/");
        }

        // Helper methods
        private async Task<int> EnterContentSyllables(ArticleCreateInputModel inputModel)
        {
            var invisibleHyphenChar = "&shy;";
            var content = inputModel.Content;
            var uri = "http://rechnik.chitanka.info/w/";

            // Remove short and hyphenated words
            string[] longWords = Regex.Replace(content, @"<[^>]+>", string.Empty)
                                        .Split(new char[] { '-', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(x => Regex.Match(x, "[а-яА-Я]{5,}").Value)
                                        .ToArray();

            // Keep track of words that have already been modified.
            // We may have the same word in content more than once(Character capitalization).
            var replacedWordsIndexes = new List<int>();

            for (int i = 0; i < longWords.Length; i++)
            {
                var originalWord = longWords[i];
                string html = string.Empty;

                try
                {
                    html = await this.GetAsync(uri + originalWord);
                }
                catch (Exception e)
                {
                    // 404 Word not found
                    continue;
                }

                var redirectRegex = new Regex(@"<p class=""data"">(.|\n)+<a href=""/w/(?<href>[^<]+)"">(.|\n)+<\/p>");
                var wordFoundRegex = new Regex(@"<div class=""data"">(?<data>(.|\n)+)</div>");
                var termsRegex = new Regex(@"<td><a href=[^>]+>(?<term>.{6,})</a>");

                // Most likely, original words won't be in infinitive, so we'll be redirected
                var redirectHrefs = redirectRegex.Matches(html);
                if (redirectHrefs.Count > 0)
                {
                    html = await this.GetAsync(uri + redirectHrefs.First().Groups["href"].Value);
                }

                // Word is in infinitive
                var matchingTerm = termsRegex.Matches(html).Select(m => m.Groups["term"].Value)
                                                           .FirstOrDefault(t => t
                                                           .Replace("-", string.Empty)
                                                           .Equals(originalWord.ToLower()));

                // Make sure original word and response word match 100%
                if (matchingTerm == null || !originalWord.ToLower().Equals(matchingTerm.Replace("-", string.Empty)))
                {
                    continue;
                }

                // Add hyphenation to original word
                var capitalizedMatchingTerm = this.CapitalizeMatchingTerm(originalWord, matchingTerm);
                capitalizedMatchingTerm = capitalizedMatchingTerm.Replace("-", invisibleHyphenChar);

                // Get word index in original content
                var indexOfOriginalWord = content.IndexOf(originalWord);

                // If word exists more than once in original content, jump forward to the next instance of the word
                while (replacedWordsIndexes.Contains(indexOfOriginalWord))
                {
                    indexOfOriginalWord = content.IndexOf(originalWord, indexOfOriginalWord + originalWord.Length);
                }

                replacedWordsIndexes.Add(indexOfOriginalWord);

                // Replace original word with hyphenated version
                content = content.Remove(indexOfOriginalWord, originalWord.Length);
                content = content.Insert(indexOfOriginalWord, capitalizedMatchingTerm);
            }

            // Check if final result matches original
            bool contentEqualsModelContent = inputModel.Content.Equals(content.Replace(invisibleHyphenChar, string.Empty));
            if (!contentEqualsModelContent)
            {
                return 0;
            }

            inputModel.Content = content;
            return 1;
        }

        private string CapitalizeMatchingTerm(string originalWord, string matchingTerm)
        {
            var matchingTermCopy = new string(matchingTerm);
            var idxCompensation = 0;

            if (originalWord.Any(c => char.IsUpper(c)))
            {
                for (int i = 0; i < originalWord.Length; i++)
                {
                    if (char.IsLower(originalWord[i]))
                    {
                        continue;
                    }

                    if (matchingTermCopy[i].Equals('-'))
                    {
                        idxCompensation++;
                    }

                    matchingTermCopy = matchingTermCopy.Remove(i + idxCompensation, 1);
                    matchingTermCopy = matchingTermCopy.Insert(i + idxCompensation, originalWord[i].ToString());
                }
            }

            return matchingTermCopy;
        }

        private async Task SetArticlePhoto(ArticleCreateInputModel inputModel)
        {
            if (inputModel.Image != null)
            {
                inputModel.ImageUrl = await this.cloudinaryService
                .UploadPicture(inputModel.Image, inputModel.Image.FileName.Split('.')[0]);
            }

            if (inputModel.PreviewImage != null)
            {
                inputModel.PreviewImageUrl = await this.cloudinaryService
                .UploadPicture(inputModel.PreviewImage, inputModel.PreviewImage.FileName.Split('.')[0]);
            }
        }

        private async Task<string> GetAsync(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
