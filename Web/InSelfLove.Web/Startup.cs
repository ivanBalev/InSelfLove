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

        // This method gets called by the runtime. Use this method to add services to the container.
        public virtual void ConfigureServices(IServiceCollection services)
        {
            // Logging
            if (!this.environment.EnvironmentName.Equals("testing"))
            {
                // testing server.CreateClient() runs through this a second time,
                // causing an error(log file is already in use by server)
                services.AddLogging(loggingBuilder =>
                {
                    var loggingSection = this.configuration.GetSection("Logging");
                    loggingBuilder.AddFile(loggingSection);
                });
            }

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

            services.AddMvc()
                .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();

            // TODO: Make debugging easier
            // Bundling & Minification
            //if (!this.environment.IsDevelopment())
            //{
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
            //}


            var connString = this.configuration.GetConnectionString("MySql");
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseMySql(connString, ServerVersion.AutoDetect(connString)));

            services.AddDefaultIdentity<ApplicationUser>(IdentityOptionsProvider.GetIdentityOptions)
                .AddRoles<ApplicationRole>().AddEntityFrameworkStores<ApplicationDbContext>();

            // Cloudinary setup
            CloudinaryDotNet.Account cloudinaryCredentials = new CloudinaryDotNet.Account(
                this.configuration["Cloudinary:CloudName"],
                this.configuration["Cloudinary:ApiKey"],
                this.configuration["Cloudinary:ApiSecret"]);

            Cloudinary cloudinaryUtility = new Cloudinary(cloudinaryCredentials);

            services.AddSingleton(cloudinaryUtility);

            services.AddResponseCaching();

            var authBuilder = services.AddAuthentication();

            // External Logins
            if (!this.environment.EnvironmentName.Equals("testing"))
            {
                authBuilder.AddGoogle(googleOptions =>
                {
                    IConfigurationSection googleAuthNSection =
                                   this.configuration.GetSection("Authentication:Google");

                    googleOptions.ClientId = googleAuthNSection["ClientId"];
                    googleOptions.ClientSecret = googleAuthNSection["ClientSecret"];
                })
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

            services.AddControllersWithViews(configure =>
            {
                configure.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
            });

            services.AddRazorPages().AddRazorRuntimeCompilation();
            services.AddSingleton(this.configuration);

            // Data repositories
            services.AddScoped(typeof(IDeletableEntityRepository<>), typeof(EfDeletableEntityRepository<>));
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped<IDbQueryRunner, DbQueryRunner>();
            services.AddScoped<IViewRender, ViewRender>();

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                // WebOptimizer output type is text/js which is not recognized by default 
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "text/javascript" });
            });

            // Application services
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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            });

            app.UseRequestLocalization(app.ApplicationServices.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

            AutoMapperConfig.RegisterMappings(
                typeof(ErrorViewModel).GetTypeInfo().Assembly,
                typeof(ArticleEditInputModel).GetTypeInfo().Assembly,
                typeof(Article).GetTypeInfo().Assembly);

            // Seed data on application startup
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                if (this.environment.IsDevelopment())
                {
                    dbContext.Database.Migrate();
                }

                new ApplicationDbContextSeeder().SeedAsync(dbContext, serviceScope.ServiceProvider).GetAwaiter().GetResult();
            }

            StripeConfiguration.ApiKey = this.configuration["Stripe:ApiKey"];

            if (this.environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

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
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(
                endpoints =>
                    {
                        endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                        endpoints.MapControllerRoute("areaRoute", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                        endpoints.MapRazorPages();
                    });
        }
    }
}
