namespace InSelfLove.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using InSelfLove.Data.Common.Repositories;
    using InSelfLove.Data.Models;
    using InSelfLove.Services.Data.Articles;
    using InSelfLove.Services.Data.CloudinaryServices;
    using InSelfLove.Services.Data.Helpers;
    using InSelfLove.Services.Mapping;
    using InSelfLove.Web.Controllers.Helpers;
    using InSelfLove.Web.InputModels.Article;
    using InSelfLove.Web.ViewModels.Article;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class ArticlesController : PaginationHelper
    {
        private readonly ICloudinaryService cloudinaryService;
        private readonly IArticleService articleService;
        private readonly IDeletableEntityRepository<Article> articleRepository;

        public ArticlesController(
            ICloudinaryService cloudinaryService,
            IArticleService articleService,
            IDeletableEntityRepository<Article> articleRepository)
            : base(articleService)
        {
            this.cloudinaryService = cloudinaryService;
            this.articleService = articleService;
            this.articleRepository = articleRepository;
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
            var viewModel = AutoMapperConfig.MapperInstance.Map<ArticleViewModel>(
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
        [Authorize(Roles = AppConstants.AdministratorRoleName)]
        public IActionResult Create()
        {
            return this.View();
        }

        [HttpPost]
        [Route("Articles/Create")]
        [Authorize(Roles = AppConstants.AdministratorRoleName)]
        public async Task<IActionResult> Create(ArticleCreateInputModel inputModel)
        {
            // Enter content syllables for better UX
            inputModel.Content = await this.EnterContentSyllables(inputModel.Content);

            await this.SetArticlePhoto(inputModel);

            string slug = await this.articleService
                .Create(AutoMapperConfig.MapperInstance.Map<Article>(inputModel));

            return this.RedirectToAction(nameof(this.Single), new { slug });
        }

        [HttpGet]
        [Authorize(Roles = AppConstants.AdministratorRoleName)]
        public async Task<IActionResult> Edit(int id)
        {
            // Get article & map to view/input model
            var model = await this.articleService.GetById(id)
                .To<ArticleEditInputModel>().FirstOrDefaultAsync();

            return this.View(model);
        }

        [HttpPost]
        [Authorize(Roles = AppConstants.AdministratorRoleName)]
        public async Task<IActionResult> Edit(ArticleEditInputModel inputModel)
        {
            // Enter content syllables for better UX
            // TODO: Error handling (try catch? )
            //inputModel.Content = await this.EnterContentSyllables(inputModel.Content);
            var img = SixLabors.ImageSharp.Image.Load(inputModel.Image.OpenReadStream());
           
            inputModel.ImageWidth = img.Width;
            inputModel.ImageHeight = img.Height;

            await this.SetArticlePhoto(inputModel);

            // Edit article and return new slug(if title is updated)
            string slug = await this.articleService
                .Edit(AutoMapperConfig.MapperInstance.Map<Article>(inputModel));

            // use new slug to open edited article
            return this.RedirectToAction(nameof(this.Single), new { slug });
        }

        [HttpPost]
        [Authorize(Roles = AppConstants.AdministratorRoleName)]
        public async Task<IActionResult> Delete(int id)
        {
            await this.articleService.Delete(id);
            return this.RedirectToAction(nameof(this.Index));
        }

        // Temporary function to syllabify all articles in db at the same time
        [HttpGet]
        [Route("Articles/SyllabifyAllArticles")]
        [Authorize(Roles = AppConstants.AdministratorRoleName)]
        public async Task<IActionResult> SyllabifyAllArticles()
        {
            var allArticles = await this.articleRepository.All().ToListAsync();

            foreach (var article in allArticles)
            {
                article.Content = await this.EnterContentSyllables(article.Content);
                this.articleRepository.Update(article);
                await this.articleRepository.SaveChangesAsync();

                // Avoid overloading the server
                System.Threading.Thread.Sleep(2000);
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        // Helper methods
        private async Task<string> EnterContentSyllables(string articleContent)
        {
            // Character designating hyphenation for client
            var invisibleHyphenChar = "&shy;";

            // Create copy of original content for comparison at the end
            var content = new string(articleContent);

            // Base uri for requests
            var uri = "http://rechnik.chitanka.info/w/";

            // Remove short, duplicate words, non-letter characters & tags
            // (hyphen, en dash, em dash)
            string[] longWords = Regex.Replace(content.ToLower(), "<[^>]+>|[^а-я-–—]+", " ")
                                      .Split(new char[] { '-', '–', '—', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Where(x => x.Length >= 5)
                                      .Distinct()
                                      .ToArray();

            // Get full html results and leave only successful
            var htmlResults = (await this.GetAsync(longWords.Select(x => uri + x)))
                .Where(x => x.Status.ToString() != "Faulted")
                .Select(x => x.Result).ToList();

            // Replace redirect html with infinitive html & return successful words
            // Can't use longWords collection further as some words most likely
            // didn't return a successful result and we need to keep the order of
            // collections the same for comparison in InsertHyphens method
            List<string> successfulWords = await this.HandleRedirects(htmlResults, uri);

            // Insert hyphens in original content for each unique word
            content = this.InsertHyphens(invisibleHyphenChar, content, htmlResults, successfulWords);

            // Check if final result matches original
            bool contentEqualsModelContent = articleContent
                                                .Replace(invisibleHyphenChar, string.Empty)
                                                .Equals(content.Replace(invisibleHyphenChar, string.Empty));

            if (!contentEqualsModelContent)
            {
                // Inform client there's been an error
                this.TempData["StatusMessage"] = "Syllabification error. No changes to original text made";

                // Return unchanged content
                return articleContent;
            }
            else
            {
                // Return changed content
                return content;
            }
        }

        private string InsertHyphens(string invisibleHyphenChar, string content, List<string> htmlResults, List<string> successfulWords)
        {
            // Matches all variants of the word in html (infinitive, suffixes, plural etc.)
            var termsRegex = new Regex(@"<td><a href=[^>]+>(?<term>.{5,})</a>");

            for (int i = 0; i < htmlResults.Count; i++)
            {
                var html = htmlResults[i];
                var originalWord = successfulWords[i];

                // Get word variant that matches our original word
                var matchingTerm = termsRegex.Matches(html).Select(m => m.Groups["term"].Value)
                                                           .FirstOrDefault(t => t
                                                           .Replace("-", string.Empty)
                                                           .Equals(originalWord));

                // Make sure original word and response word match 100%
                if (matchingTerm == null ||
                    !originalWord.Equals(matchingTerm.Replace("-", string.Empty)))
                {
                    continue;
                }

                // Get word index in original content
                var indexOfOriginalWord = content.ToLower().IndexOf(originalWord);

                // Loop through all instances of the word in content and insert hyphens
                while (indexOfOriginalWord != -1)
                {
                    // Stored for compensating lengthening of text due to insertion
                    var insertionCount = 0;

                    // Find hyphens in result from rechnik.chitanka.info
                    for (int j = 0; j < matchingTerm.Length; j++)
                    {
                        if (matchingTerm[j] == '-')
                        {
                            // Insert hyphen in content
                            content = content.Insert(
                                indexOfOriginalWord + j + (insertionCount * (invisibleHyphenChar.Length - 1)),
                                invisibleHyphenChar);

                            insertionCount++;
                        }
                    }

                    // Find next instance of the same word in our original content
                    // Compensate for current instance + length of inserted hyphens
                    indexOfOriginalWord = content.ToLower().IndexOf(
                        originalWord,
                        indexOfOriginalWord + originalWord.Length + (insertionCount * invisibleHyphenChar.Length));
                }
            }

            return content;
        }

        private async Task<List<string>> HandleRedirects(List<string> htmlResults, string uri)
        {
            // Regex to identify redirects (meaning word is not in infinitive)
            var redirectRegex = new Regex(@"<p class=""data"">(.|\n)+<a href=""/w/(?<href>[^<]+)"">(.|\n)+<\/p>");

            // Keeps track of the redirects' indices in original collection
            // We need to keep the order of results
            var redirectIndices = new List<int>();

            // Collects all redirect uris for another parallel request
            var redirectUris = new List<string>();

            // Title element from html containts the original word we requested
            // Easiest way I found to keep trach of which requests were successful
            // We later need the original word to extract its hyphenation from html
            var titleRegex = new Regex(@"<title>[\w\W]*<\/title>");

            // titleRegex gets the successful words, stored in this collection
            var successfulWords = new List<string>();

            for (int i = 0; i < htmlResults.Count; i++)
            {
                var currentHtml = htmlResults[i];

                // Remove tags & non-text characters from title element
                // and extract only the requested word
                var title = Regex.Replace(titleRegex.Match(currentHtml).Value, "[^а-яА-Я]+", " ")
                   .Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];

                // Store the requested word in collection
                successfulWords.Add(title);

                // if redirects on page ->
                // Need to go to infinitive page for hyphenation info
                var redirectHrefs = redirectRegex.Matches(currentHtml);
                if (redirectHrefs.Count > 0)
                {
                    // Keep track of replaced words indices in original collection
                    redirectIndices.Add(i);

                    // Enter redirect uri in list for parallel requests later
                    redirectUris.Add(uri + redirectHrefs.First().Groups["href"].Value);
                }
            }

            // All results will be successful - unsuccessful ones
            // already filtered in original results collection
            var redirectsResults = (await this.GetAsync(redirectUris))
                .Select(x => x.Result).ToList();

            // Replace redirects in original collection with new results
            for (int i = 0; i < redirectsResults.Count; i++)
            {
                // Keeping the order words were originally entered in
                htmlResults[redirectIndices[i]] = redirectsResults[i];
            }

            return successfulWords;
        }

        private async Task<List<Task<string>>> GetAsync(IEnumerable<string> uris)
        {
            var httpClient = new HttpClient();
            httpClient.Timeout = new TimeSpan(0, 0, 8);

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
                // Upload to cloud & store link reference
                inputModel.ImageUrl = await this.cloudinaryService
                .UploadPicture(inputModel.Image, inputModel.Image.FileName.Split('.')[0]);
            }

            if (inputModel.PreviewImage != null)
            {
                // Upload to cloud & store link reference
                inputModel.PreviewImageUrl = await this.cloudinaryService
                .UploadPicture(inputModel.PreviewImage, inputModel.PreviewImage.FileName.Split('.')[0]);
            }
        }
    }
}
