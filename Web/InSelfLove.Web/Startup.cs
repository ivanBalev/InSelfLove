namespace InSelfLove.Web
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    using CloudinaryDotNet;
    using InSelfLove.Data;
    using InSelfLove.Data.Common;
    using InSelfLove.Data.Common.Repositories;
    using InSelfLove.Data.Models;
    using InSelfLove.Data.Repositories;
    using InSelfLove.Data.Seeding;
    using InSelfLove.Services.Data.Appointments;
    using InSelfLove.Services.Data.Articles;
    using InSelfLove.Services.Data.CloudinaryServices;
    using InSelfLove.Services.Data.Comments;
    using InSelfLove.Services.Data.Courses;
    using InSelfLove.Services.Data.Recaptcha;
    using InSelfLove.Services.Data.Stripe;
    using InSelfLove.Services.Data.Videos;
    using InSelfLove.Services.Mapping;
    using InSelfLove.Services.Messaging;
    using InSelfLove.Web.Controllers.Helpers;
    using InSelfLove.Web.InputModels.Article;
    using InSelfLove.Web.ViewModels;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.ResponseCompression;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Stripe;

    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            this.configuration = configuration;
            this.environment = environment;
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            // Logging
            if (!this.environment.EnvironmentName.Equals("testing"))
            {
                services.AddLogging(loggingBuilder =>
                {
                    var loggingSection = this.configuration.GetSection("Logging");
                    loggingBuilder.AddFile(loggingSection);
                });
            }

            // Localization
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var cultures = new List<CultureInfo>
                {
                    new CultureInfo("en"),
                    new CultureInfo("bg"),
                };

                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("bg");
                options.SupportedCultures = cultures;
                options.SupportedUICultures = cultures;
                options.RequestCultureProviders = new List<IRequestCultureProvider>
                    {
                        new CookieRequestCultureProvider(),
                        new QueryStringRequestCultureProvider(),
                    };
            });

            // MVC
            services.AddMvc()
                .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();

            // Bundling & Minification
            services.AddWebOptimizer(pipeline =>
            {
                pipeline.AddCssBundle(
                    "/css/bundle.css",
                    "lib/bootstrap/dist/css/bootstrap.min.css",
                    "lib/lite-youtube-embed/src/lite-yt-embed.min.css",
                    "lib/leaflet.js/dist/leaflet.css",
                    "lib/the-datepicker.js/dist/the-datepicker.min.css",
                    "Custom/css/style.css");
                pipeline.MinifyCssFiles("Custom/css/calendar.css", "Custom/css/stripe-style.css");

                pipeline.AddJavaScriptBundle(
                    "/js/layout.js",
                    "/lib/popper.js/umd/popper.min.js",
                    "/lib/bootstrap/dist/js/bootstrap.min.js",
                    "/Custom/js/pwa.js",
                    "/Custom/js/copyrightjs.js",
                    "/Custom/js/collapseNavbar.js",
                    "/Custom/js/cookieConsent.js",
                    "/Custom/js/timezone.js",
                    "/lib/lite-youtube-embed/src/lite-yt-embed.min.js",
                    "/Custom/js/homeVideoTitleLengthAdjust.js",
                    "/Custom/js/equalizeArticleHeight.js",
                    "/Custom/js/equalizeVideoHeight.js",
                    "/Custom/js/searchNav.js",
                    "/Custom/js/searchPagination.js");

                pipeline.AddJavaScriptBundle(
                    "/js/comments.js",
                    "/Custom/js/commentsHideDisplayItems.js",
                    "/Custom/js/editComment.js",
                    "/Custom/js/deleteComment.js",
                    "/Custom/js/addComment.js");

                pipeline.AddJavaScriptBundle(
                    "/js/cloudinary.js",
                    "/lib/cloudinary/cloudinary-core-shrinkwrap.min.js",
                    "/Custom/js/cloudinary.js");

                pipeline.AddJavaScriptBundle(
                    "/js/calendar.js",
                    "/lib/fullcalendar/index.global.min.js",
                    "/Custom/js/fullcalendar.js");

                pipeline.AddJavaScriptBundle(
                    "/js/contacts.js",
                    "/lib/leaflet.js/dist/leaflet.js",
                    "/Custom/js/leaflet.js",
                    "/Custom/js/contacts.js");
            });

            // Database connection
            var connString = this.configuration.GetConnectionString("MySql");
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseMySql(connString, ServerVersion.AutoDetect(connString)));

            // Identity & roles
            services.AddDefaultIdentity<ApplicationUser>(IdentityOptionsProvider.GetIdentityOptions)
                .AddRoles<ApplicationRole>().AddEntityFrameworkStores<ApplicationDbContext>();

            // Cloudinary setup
            var cloudinaryCredentials = new CloudinaryDotNet.Account(
                this.configuration["Cloudinary:CloudName"],
                this.configuration["Cloudinary:ApiKey"],
                this.configuration["Cloudinary:ApiSecret"]);
            Cloudinary cloudinaryUtility = new Cloudinary(cloudinaryCredentials);
            services.AddSingleton(cloudinaryUtility);

            // Response caching
            services.AddResponseCaching();

            // Authentication
            var authBuilder = services.AddAuthentication();

            // External Logins
            if (!this.environment.EnvironmentName.Equals("testing"))
            {
                // Google
                authBuilder.AddGoogle(googleOptions =>
                {
                    IConfigurationSection googleAuthNSection =
                                   this.configuration.GetSection("Authentication:Google");

                    googleOptions.ClientId = googleAuthNSection["ClientId"];
                    googleOptions.ClientSecret = googleAuthNSection["ClientSecret"];
                })
                // Facebook
               .AddFacebook(facebookOptions =>
               {
                   facebookOptions.AppId = this.configuration["Authentication:Facebook:AppId"];
                   facebookOptions.AppSecret = this.configuration["Authentication:Facebook:AppSecret"];
               });
            }

            // Cookies setup
            services.Configure<CookiePolicyOptions>(
            options =>
            {
                options.CheckConsentNeeded = context => true;
            });

            // Controllers & Views
            services.AddControllersWithViews(configure =>
            {
                configure.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            // Antiforgery
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
            });

            // Razor pages
            services.AddRazorPages().AddRazorRuntimeCompilation();

            // Singleton config
            services.AddSingleton(this.configuration);

            // Data repositories
            services.AddScoped(typeof(IDeletableEntityRepository<>), typeof(EfDeletableEntityRepository<>));
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped<IDbQueryRunner, DbQueryRunner>();

            // View rendering
            services.AddScoped<IViewRender, ViewRender>();

            // Appointment emails
            services.AddScoped<IAppointmentEmailHelper, AppointmentEmailHelper>();

            // Response compression
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                // WebOptimizer output type is text/js which is not recognized by default
                // WebOptimizer is responsible for bundling & minification
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "text/javascript" });
            });

            // Transient app services
            services.AddTransient<IEmailSender>(x => new SendGridEmailSender(this.configuration["SendGrid:ApiKey"]));
            services.AddTransient<IArticleService, ArticleService>();
            services.AddTransient<IVideoService, VideoService>();
            services.AddTransient<ICloudinaryService, CloudinaryService>();
            services.AddTransient<IAppointmentService, AppointmentService>();
            services.AddTransient<ICommentService, CommentService>();
            services.AddTransient<IRecaptchaService, RecaptchaService>();
            services.AddTransient<ICourseService, CourseService>();
            services.AddTransient<IStripeService, StripeService>();

            // Development exceptions
            services.AddDatabaseDeveloperPageExceptionFilter();
        }

        public void Configure(IApplicationBuilder app)
        {
            // Headers forwarded by nginx
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            });

            // Localization
            app.UseRequestLocalization(app.ApplicationServices.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

            // Automapper
            // Register ViewModels, InputModels & DataModels assemblies
            AutoMapperConfig.RegisterMappings(
                typeof(ErrorViewModel).GetTypeInfo().Assembly,
                typeof(ArticleEditInputModel).GetTypeInfo().Assembly,
                typeof(Article).GetTypeInfo().Assembly);

            // Seed data on app startup
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                if (this.environment.IsDevelopment())
                {
                    dbContext.Database.Migrate();
                }

                new ApplicationDbContextSeeder().SeedAsync(dbContext, serviceScope.ServiceProvider).GetAwaiter().GetResult();
            }

            // Stripe
            StripeConfiguration.ApiKey = this.configuration["Stripe:ApiKey"];

            // Exceptions
            if (this.environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Apply pending migrations
                app.UseMigrationsEndPoint();

            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

                // Enforce HTTPS
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

            // Response compression, bundling, minification & caching
            app.UseResponseCompression();
            app.UseWebOptimizer();
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse =
                r =>
                {
                    string path = r.File.PhysicalPath;
                    if (path.EndsWith(".css") || path.EndsWith(".js") || path.EndsWith(".gif") ||
                    path.EndsWith(".jpg") || path.EndsWith(".png") || path.EndsWith(".svg") ||
                    path.EndsWith(".ttf"))
                    {
                        TimeSpan maxAge = new TimeSpan(1, 0, 0, 0);
                        r.Context.Response.Headers.Append("Cache-Control", "public, max-age=" + maxAge.TotalSeconds.ToString("0"));
                    }
                },
            });
            app.UseResponseCaching();

            // Cookies
            app.UseCookiePolicy();

            // Authentication
            app.UseAuthentication();

            // Order of Routing, Authorization & Endpoints setup
            // needs to stay like this for config to work correctly
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(
                endpoints =>
                    {
                        // Default setup
                        endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

                        // Setup for identity pages in Areas folder
                        endpoints.MapControllerRoute("areaRoute", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                        endpoints.MapRazorPages();
                    });
        }
    }
}
