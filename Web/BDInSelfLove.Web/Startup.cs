namespace BDInSelfLove.Web
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;

    using BDInSelfLove.Data;
    using BDInSelfLove.Data.Common;
    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Data.Repositories;
    using BDInSelfLove.Data.Seeding;
    using BDInSelfLove.Services.Data;
    using BDInSelfLove.Services.Data.Calendar;
    using BDInSelfLove.Services.Data.CloudinaryService;
    using BDInSelfLove.Services.Data.CommentService;
    using BDInSelfLove.Services.Data.Search;
    using BDInSelfLove.Services.Data.User;
    using BDInSelfLove.Services.Data.Video;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Messaging;
    using BDInSelfLove.Services.Models.Article;
    using BDInSelfLove.Web.InputModels.Article;
    using BDInSelfLove.Web.ViewComponents.Models.Video;
    using BDInSelfLove.Web.ViewModels;
    using BDInSelfLove.Web.ViewModels.Article;
    using CloudinaryDotNet;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
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

            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(this.configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<ApplicationUser>(IdentityOptionsProvider.GetIdentityOptions)
                .AddRoles<ApplicationRole>().AddEntityFrameworkStores<ApplicationDbContext>();

            // Cloudinary setup
            Account cloudinaryCredentials = new Account(
                this.configuration["Cloudinary:CloudName"],
                this.configuration["Cloudinary:ApiKey"],
                this.configuration["Cloudinary:ApiSecret"]);

            Cloudinary cloudinaryUtility = new Cloudinary(cloudinaryCredentials);

            services.AddSingleton(cloudinaryUtility);

            services.AddResponseCaching();

            // External Logins
            services.AddAuthentication()
                .AddGoogle(googleOptions =>
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

            // Cookies setup
            services.Configure<CookiePolicyOptions>(
                options =>
                {
                    options.CheckConsentNeeded = context => true;
                });

            var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions()
            {
                Path = "/",
                HttpOnly = false,
                IsEssential = true,
                Expires = DateTime.Now.AddMonths(1),
            };

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

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            // Application services
            services.AddTransient<IEmailSender>(x => new SendGridEmailSender(this.configuration["SendGrid:ApiKey"]));
            services.AddTransient<IArticleService, ArticleService>();
            services.AddTransient<IVideoService, VideoService>();
            services.AddTransient<ICloudinaryService, CloudinaryService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IAppointmentService, AppointmentService>();
            services.AddTransient<ICommentService, CommentService>();
            services.AddTransient<ISearchService, SearchService>();

            // Logging
            services.AddLogging(loggingBuilder =>
            {
                var loggingSection = this.configuration.GetSection("Logging");
                loggingBuilder.AddFile(loggingSection);
            });

            // Development exceptions
            services.AddDatabaseDeveloperPageExceptionFilter();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRequestLocalization(app.ApplicationServices.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

            AutoMapperConfig.RegisterMappings(
                typeof(ErrorViewModel).GetTypeInfo().Assembly,
                typeof(ArticleServiceModel).GetTypeInfo().Assembly,
                typeof(ArticleViewModel).GetTypeInfo().Assembly,
                typeof(HomeVideoViewModel).GetTypeInfo().Assembly,
                typeof(ArticleEditInputModel).GetTypeInfo().Assembly);

            // Seed data on application startup
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                if (env.IsDevelopment())
                {
                    dbContext.Database.Migrate();
                }

                new ApplicationDbContextSeeder().SeedAsync(dbContext, serviceScope.ServiceProvider).GetAwaiter().GetResult();
            }

            if (env.IsDevelopment())
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
                        TimeSpan maxAge = new TimeSpan(7, 0, 0, 0);
                        r.Context.Response.Headers.Append("Cache-Control", "public, max-age=" + maxAge.TotalSeconds.ToString("0"));
                    }
                },
            });

            // TODO: responsecompression is not working
            app.UseResponseCompression();
            app.UseResponseCaching();
            app.UseCookiePolicy();

            app.UseRouting();
            //TODO: REMOVE HTTPS REDIRECTION FOR AZURE BUT ADD WHEN DEPLOYING ELSEWHERE!

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(
                endpoints =>
                    {
                        endpoints.MapControllerRoute("areaRoute", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                        // Below is used for SEO (not only show Articles/Single/{id}, but the article's name after the id /{id}/{articleName})
                        //endpoints.MapControllerRoute("forumCategory", "f/{name:minlength(3)}", new { controller = "Categories", action = "ByName" });
                        endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                        endpoints.MapRazorPages();
                    });
        }
    }
}
