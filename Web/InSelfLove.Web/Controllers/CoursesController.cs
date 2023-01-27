namespace InSelfLove.Web.Controllers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using InSelfLove.Data.Models;
    using InSelfLove.Services.Data.CloudinaryServices;
    using InSelfLove.Services.Data.Courses;
    using InSelfLove.Services.Data.Helpers;
    using InSelfLove.Services.Data.Stripe;
    using InSelfLove.Services.Mapping;
    using InSelfLove.Web.Infrastructure.Helpers;
    using InSelfLove.Web.Infrastructure.ModelBinders;
    using InSelfLove.Web.InputModels.Courses;
    using InSelfLove.Web.ViewModels.Courses;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Net.Http.Headers;
    using Newtonsoft.Json;
    using Stripe;

    // TODO: this feature hasn't been released yet. All main functionality works correctly.
    public class CoursesController : Controller
    {
        private const string BaseUri = "https://video.bunnycdn.com/library/";

        private static readonly FormOptions DefaultFormOptions = new FormOptions();
        private readonly ICourseService courseService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ICloudinaryService cloudinaryService;
        private readonly IStripeService stripeService;
        private readonly ILogger<CoursesController> logger;
        private readonly IConfiguration configuration;

        public CoursesController(
            ICourseService courseService,
            UserManager<ApplicationUser> userManager,
            ICloudinaryService cloudinaryService,
            IStripeService stripeService,
            ILogger<CoursesController> logger,
            IConfiguration configuration)
        {
            this.courseService = courseService;
            this.userManager = userManager;
            this.cloudinaryService = cloudinaryService;
            this.stripeService = stripeService;
            this.logger = logger;
            this.configuration = configuration;
        }

        [HttpGet]
        [Authorize(Roles = AppConstants.AdministratorRoleName)]
        public async Task<IActionResult> Index()
        {
            var viewModel = new CoursesIndexViewModel()
            {
                Courses = await this.courseService.GetAll()
                .To<CoursePreviewViewModel>()
                .ToListAsync(),
            };

            return this.View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = AppConstants.AdministratorRoleName)]
        public async Task<IActionResult> CreateCourse(CourseCreateInputModel inputModel)
        {
            await this.SetThumbnailImage(inputModel);
            var priceId = await this.stripeService.CreateProduct(inputModel.Title, inputModel.Price);
            var id = await this.courseService.CreateCourse(inputModel.Title, inputModel.ThumbnailLink, priceId, inputModel.Price);
            return this.RedirectToAction("Course", new { id });
        }

        [HttpPost]
        [Authorize(Roles = AppConstants.AdministratorRoleName)]
        public async Task<IActionResult> BuyCourse(string courseId, string priceId)
        {
            var userId = this.userManager.GetUserId(this.User);

            // Set user reference info for post-payment redirect. Action Payment
            var clientReferenceInfo = $"userId: {userId}, courseId: {courseId}";
            var domain = this.HttpContext.Request.Scheme + "://" + this.HttpContext.Request.Host;
            var session = await this.stripeService.CreateSession(domain, priceId, "/stripe/success", "/stripe/cancel", clientReferenceInfo);

            this.Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        [Authorize(Roles = AppConstants.AdministratorRoleName)]
        public async Task<IActionResult> Payment()
        {
            var json = await new StreamReader(this.HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                // Validation
                var stripeEvent = EventUtility.ConstructEvent(
                  json,
                  this.Request.Headers["Stripe-Signature"],
                  this.configuration["Stripe:TestSecret"]);

                // Rudimentary error handling
                if (stripeEvent.Type.Contains("fail"))
                {
                    this.logger.LogError("STRIPE PAYMENT FAILED" + json);
                    return this.BadRequest();
                }

                // Handle the checkout.session.completed event
                if (stripeEvent.Type == Events.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                    await this.FulfillOrder(session);
                }

                return this.Ok();
            }
            catch (StripeException e)
            {
                // Rudimentary exception handling
                this.logger.LogError("STRIPE PAYMENT EXCEPTION" + e.Message + Environment.NewLine + "JSON: " + json);
                return this.BadRequest();
            }
        }

        [HttpGet]
        [Authorize(Roles = AppConstants.AdministratorRoleName)]
        public async Task<IActionResult> Course(string id)
        {
            var courseVideos = await this.courseService.GetById(id)
                .To<CourseViewModel>()
                .FirstOrDefaultAsync();

            return this.View(courseVideos);
        }

        [HttpGet]
        [Authorize(Roles = AppConstants.AdministratorRoleName)]
        public async Task<IActionResult> CourseVideo(string id, string courseId)
        {
            var user = await this.userManager.Users
                .Include(u => u.Courses)
                .FirstOrDefaultAsync(u => u.Id.Equals(this.userManager.GetUserId(this.User)));

            // TODO: make it impossible to request from the front end
            if (!this.User.IsInRole(AppConstants.AdministratorRoleName) &&
                (user == null || !user.Courses.Any(c => c.Id.Equals(courseId))))
            {
                // Return preview video for unpaid or unregistered users (user has tried to bypass frontend validation)
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
        [Authorize(Roles = AppConstants.AdministratorRoleName)]
        public async Task<IActionResult> CreateCourseVideo()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(this.Request.ContentType))
            {
                throw new BadHttpRequestException("Not a multipart request");
            }

            var boundary = MultipartRequestHelper.GetBoundary(
                          MediaTypeHeaderValue.Parse(this.Request.ContentType),
                          DefaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, this.Request.Body);

            // Read sections of multipart file
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

                // TODO: verify AntiForgery token
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

                    // Upload video to BunnyCDN
                    using var fileStream = section.Body;
                    courseVideoGuid = await this.UploadFile(fileStream, fileName);
                }

                section = await reader.ReadNextSectionAsync();
            }

            var result = await this.courseService.CreateCourseVideo(courseVideoGuid, fileName.Split('.')[0], courseGuid);
            return this.Created(nameof(CoursesController), null);
        }

        private async Task FulfillOrder(Stripe.Checkout.Session session)
        {
            // Get user reference info set in BuyCourse Action
            var userCourseInfo = session.ClientReferenceId.Split(", ");

            var payment = new Payment
            {
                ApplicationUserId = userCourseInfo[0].Split(": ")[1],
                CourseId = userCourseInfo[1].Split(": ")[1],
                StripeCustomerId = session.CustomerId,
                AmountTotal = (long)session.AmountTotal,
            };

            await this.stripeService.StorePayment(payment);

            // TODO: Send the customer a receipt email
        }


        // This should probably not be inside the controller class
        private async Task<string> UploadFile(Stream stream, string fileName)
        {
            var client = new HttpClient();
            var createRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(BaseUri + this.configuration["BunnyCDN:LibraryId"] + "/videos"),
                Headers =
                    {
                        { "Accept", "application/json" },
                        { "AccessKey",  this.configuration["BunnyCDN:AccessKey"] },
                    },
                Content = new StringContent($"{{\"title\":\"{fileName}\"}}")
                {
                    Headers =
                        {
                            ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/*+json"),
                        },
                },
            };

            // Crete video file on CDN (2-step process)
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
                RequestUri = new Uri(BaseUri + this.configuration["BunnyCDN:LibraryId"] + $"/videos/{createdResponse.Guid}"),
                Content = new StreamContent(stream),
                Headers =
                {
                    { "Accept", "application/json" },
                    { "AccessKey", this.configuration["BunnyCDN:AccessKey"] },
                },
            };

            // Upload video to CDN (2-nd step)
            using (var uploadResponse = await client.SendAsync(uploadRequest))
            {
                uploadResponse.EnsureSuccessStatusCode();
            }

            return createdResponse.Guid.ToString();
        }

        private async Task SetThumbnailImage(CourseCreateInputModel inputModel)
        {
            if (inputModel.ThumbnailImage != null)
            {
                inputModel.ThumbnailLink = await this.cloudinaryService
                .UploadPicture(inputModel.ThumbnailImage, inputModel.ThumbnailImage.FileName.Split('.')[0]);
            }
        }
    }
}
