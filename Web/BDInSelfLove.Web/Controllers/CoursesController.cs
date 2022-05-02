namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using System.Web.Helpers;
    using AutoMapper.QueryableExtensions;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Courses;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.Infrastructure.Helpers;
    using BDInSelfLove.Web.Infrastructure.ModelBinders;
    using BDInSelfLove.Web.InputModels.Courses;
    using BDInSelfLove.Web.ViewModels.Courses;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Net.Http.Headers;
    using Newtonsoft.Json;

    public class CoursesController : Controller
    {
        // TODO: these shouldn't be in code
        private const string LibraryId = "35207";
        private const string AccessKey = "95e3d483-9119-40ab-a1e92056c5a3-9cb2-4728";
        private const string BaseUri = "https://video.bunnycdn.com/library/" + LibraryId;

        private static readonly FormOptions DefaultFormOptions = new FormOptions();
        private readonly ICourseService courseService;
        private readonly UserManager<ApplicationUser> userManager;


        public CoursesController(ICourseService courseService,
                UserManager<ApplicationUser> userManager)
        {
            this.courseService = courseService;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new CoursesIndexViewModel()
            {
                Courses = await this.courseService.GetAll()
                .To<CourseViewModel>()
                .ToListAsync(),
            };

            return this.View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse(CourseCreateInputModel inputModel)
        {
            //var client = new HttpClient();
            //var request = new HttpRequestMessage
            //{
            //    Method = HttpMethod.Post,
            //    RequestUri = new Uri(BaseUri + "/collections"),
            //    Headers =
            //        {
            //            { "Accept", "application/json" },
            //            { "AccessKey", AccessKey },
            //        },
            //    Content = new StringContent($"{{\"name\":\"{inputModel.Title}\"}}")
            //    {
            //        Headers =
            //                {
            //                    ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/*+json"),
            //                },
            //    },
            //};

            //var collectionCreatedResponse = new BunnyCollectionCreatedResponse();
            //using (var response = await client.SendAsync(request))
            //{
            //    response.EnsureSuccessStatusCode();
            //    var body = await response.Content.ReadAsStringAsync();
            //    collectionCreatedResponse = JsonConvert.DeserializeObject<BunnyCollectionCreatedResponse>(body);
            //}

            await this.courseService.CreateCourse(Guid.NewGuid().ToString(), inputModel.Title);
            return this.View("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Course(string id)
        {
            var courseVideos = await this.courseService.GetById(id)
                .To<CourseVideoPreviewViewModel>()
                .ToListAsync();

            this.TempData["CourseId"] = id;

            return this.View(courseVideos);
        }

        [HttpGet]
        public async Task<IActionResult> CourseVideo(string id)
        {
            var user = await this.userManager.GetUserAsync(this.User);

            if (user == null ||
                !user.Courses.Any(c => c.Id == id))
            {
                return this.View(await this.courseService.GetCoursePreviewVideo(id)
                    .To<CourseVideoViewModal>().FirstOrDefaultAsync());
            }

            return this.View(await this.courseService.GetCourseVideo(id)
                .To<CourseVideoViewModal>().FirstOrDefaultAsync());
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        [DisableFormValueModelBindingAttribute]
        [RequestSizeLimit(1073741824)]
        [RequestFormLimits(MultipartBodyLengthLimit = 1073741824)]
        public async Task<IActionResult> CreateCourseVideo()
        {
            // TODO: verification token
            if (!MultipartRequestHelper.IsMultipartContentType(this.Request.ContentType))
            {
                throw new BadHttpRequestException("Not a multipart request");
            }

            var boundary = MultipartRequestHelper.GetBoundary(
                          MediaTypeHeaderValue.Parse(this.Request.ContentType),
                          DefaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, this.Request.Body);

            var section = await reader.ReadNextSectionAsync();
            var courseVideoGuid = string.Empty;
            var courseGuid = string.Empty;
            var fileName = string.Empty;

            while (section != null)
            {
                if (section == null)
                {
                    throw new BadHttpRequestException("No sections in multipart defined");
                }

                if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition))
                {
                    throw new BadHttpRequestException("No content disposition in multipart defined");
                }

                var pi4ka = contentDisposition.DispositionType.Value;

                if (contentDisposition.Name.Value.Equals("CourseId"))
                {
                    courseGuid = await section.ReadAsStringAsync();
                }
                else if (contentDisposition.IsFileDisposition())
                {
                    fileName = contentDisposition.FileNameStar.ToString();
                    if (string.IsNullOrEmpty(fileName))
                    {
                        fileName = contentDisposition.FileName.ToString();
                    }

                    if (string.IsNullOrEmpty(fileName))
                    {
                        throw new BadHttpRequestException("No filename defined.");
                    }

                    using var fileStream = section.Body;
                    courseVideoGuid = await this.SendFileSomewhere(fileStream, fileName);
                }

                section = await reader.ReadNextSectionAsync();
            }

            var result = await this.courseService.CreateCourseVideo(courseVideoGuid, fileName.Split('.')[0], courseGuid);

            return this.Created(nameof(CoursesController), null);
        }

        // This should probably not be inside the controller class
        private async Task<string> SendFileSomewhere(Stream stream, string fileName)
        {
            var client = new HttpClient();
            var createRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(BaseUri + "/videos"),
                Headers =
                    {
                        { "Accept", "application/json" },
                        { "AccessKey", AccessKey },
                    },
                Content = new StringContent($"{{\"title\":\"{fileName}\"}}")
                {
                    Headers =
                        {
                            ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/*+json"),
                        },
                },
            };

            var createdResponse = new BunnyVideoCreatedResponse();
            using (var response = await client.SendAsync(createRequest))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                createdResponse = JsonConvert.DeserializeObject<BunnyVideoCreatedResponse>(body);
            }

            var uploadRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(BaseUri + $"/videos/{createdResponse.Guid}"),
                Content = new StreamContent(stream),
                Headers =
                {
                    { "Accept", "application/json" },
                    { "AccessKey", AccessKey },
                },
            };
            using (var uploadResponse = await client.SendAsync(uploadRequest))
            {
                uploadResponse.EnsureSuccessStatusCode();
            }

            return createdResponse.Guid.ToString();
        }
    }
}
