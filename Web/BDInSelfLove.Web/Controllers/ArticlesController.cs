namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

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
    using Newtonsoft.Json.Linq;

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
            // Get info for client & create view model
            var viewModel = AutoMapperConfig.MapperInstance
                .Map<ArticleViewModel>(
                await this.articleService.GetBySlug(slug, this.UserTimezoneIdFromCookie));

            // Return 404 if article doesn't exist
            if (viewModel == null)
            {
                return this.NotFound();
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
            int syllablesResult = await this.EnterContentSyllables(inputModel);
            await this.SetArticlePhoto(inputModel);

            string slug = await this.articleService
                .Create(AutoMapperConfig.MapperInstance.Map<Article>(inputModel));

            if (syllablesResult != 1)
            {
                this.TempData["StatusMessage"] = "Syllabification error";
            }

            return this.RedirectToAction("Single", new { slug });
        }

        [HttpGet]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public async Task<IActionResult> Edit(int id)
        {
            // Get article & map to view/input model
            var model = await this.articleService.GetById(id)
                .To<ArticleEditInputModel>().FirstOrDefaultAsync();

            return this.View(model);
        }

        [HttpPost]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public async Task<IActionResult> Edit(ArticleEditInputModel inputModel)
        {
            await this.SetArticlePhoto(inputModel);

            // Edit article and return new slug(if title is updated)
            string slug = await this.articleService
                .Edit(AutoMapperConfig.MapperInstance.Map<Article>(inputModel));

            // use new slug to open edited article
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
            // [^а-яА-яA-Za-z-–!?,.:;\\s\\d„“\"'()\\/&@№$#%*()_+]+

            var invisibleHyphenChar = "&shy;";

            var content = inputModel.Content;
            var uri = "http://rechnik.chitanka.info/w/";

            // Remove short and duplicate words
            string[] longWords = Regex.Replace(content, "<[^>]+>|[^а-яА-Я-–]+", " ")
                                        .Split(new char[] { '-', '–', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Where(x => x.Length >= 5)
                                        .Select(x => x.ToLower())
                                        .Distinct()
                                        .ToArray();

            // Get result and filter only successful
            var result = (await this.GetAsync(longWords.Select(x => uri + x)))
                .Where(x => x.Status.ToString() != "Faulted")
                .Select(x => x.Result).ToList();

            // Get all redirects (meaning word is not in infinitive)
            var redirectRegex = new Regex(@"<p class=""data"">(.|\n)+<a href=""/w/(?<href>[^<]+)"">(.|\n)+<\/p>");
            var redirectIndices = new List<int>();
            var redirectUris = new List<string>();

                                     // TODO: //[^\s—]+
            var titleRegex = new Regex(@"<title>[\w\W]*<\/title>");
            var successfulWords = new List<string>();

            for (int i = 0; i < result.Count; i++)
            {
                // Keep track of all successful requests                     
                var title = Regex.Replace(titleRegex.Match(result[i]).Value, "[^а-яА-Я]+", " ")
                   .Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
                successfulWords.Add(title);

                var redirectHrefs = redirectRegex.Matches(result[i]);
               
                if (redirectHrefs.Count > 0)
                {
                    // Keep track of replaced words indices in original collection
                    redirectIndices.Add(i);

                    // Enter redirect uri in list for parallel requests after
                    redirectUris.Add(uri + redirectHrefs.First().Groups["href"].Value);
                }
            }

            // All results should be successful as unsuccessful ones were
            // already filtered in original results collection
            var redirectsResult = (await this.GetAsync(redirectUris))
                .Select(x => x.Result).ToList();

            // Replace redirects in original collection with new results
            for (int i = 0; i < redirectsResult.Count; i++)
            {
                result[redirectIndices[i]] = redirectsResult[i];
            }

            for (int i = 0; i < result.Count; i++)
            {
                var html = result[i];

                // TODO: are html results in same order as original words? how to filter unsuccessful requests from longwords collection?
                var originalWord = successfulWords[i];
                var termsRegex = new Regex(@"<td><a href=[^>]+>(?<term>.{5,})</a>");

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

                // Get word index in original content
                var indexOfOriginalWord = content.ToLower().IndexOf(originalWord);

                while (indexOfOriginalWord != -1)
                {
                    var insertionCount = 0;

                    // Replace word in content
                    for (int j = 0; j < matchingTerm.Length; j++)
                    {
                        if (matchingTerm[j] == '-')
                        {
                            content = content.Insert(
                                indexOfOriginalWord + j + (insertionCount * (invisibleHyphenChar.Length - 1)),
                                invisibleHyphenChar);

                            insertionCount++;
                        }
                    }

                    // Make sure all instances of the word are hyphenated
                    indexOfOriginalWord = content.ToLower().IndexOf(originalWord,
                        indexOfOriginalWord + originalWord.Length + (insertionCount * invisibleHyphenChar.Length));
                }
            }

            // Check if final result matches original
            bool contentEqualsModelContent = inputModel.Content.Replace(invisibleHyphenChar, string.Empty).Equals(content.Replace(invisibleHyphenChar, string.Empty));
            if (!contentEqualsModelContent)
            {
                return 0;
            }

            inputModel.Content = content;
            return 1;
        }

        private async Task<List<Task<string>>> GetAsync(IEnumerable<string> uris)
        {
            var httpClient = new HttpClient();
            var taskList = new List<Task<string>>();

            foreach (var uri in uris)
            {
                // by not awaiting each call, we achieve parallelism
                taskList.Add(httpClient.GetStringAsync(uri));
            }

            try
            {
                // asynchronously wait until all tasks are complete
                await Task.WhenAll(taskList.ToArray());
            }
            catch (Exception ex)
            {
            }

            return taskList;
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
    }
}
