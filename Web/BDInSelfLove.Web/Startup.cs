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
    using BDInSelfLove.Services.Data.Appointments;
    using BDInSelfLove.Services.Data.Articles;
    using BDInSelfLove.Services.Data.CloudinaryServices;
    using BDInSelfLove.Services.Data.Comments;
    using BDInSelfLove.Services.Data.Videos;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Messaging;
    using BDInSelfLove.Web.InputModels.Article;
    using BDInSelfLove.Web.ViewModels;
    using CloudinaryDotNet;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpOverrides;
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

            services.AddDbContext<ApplicationDbContext>(
                options => options.UseMySQL(this.configuration.GetConnectionString("MySql")));

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
            if (this.environment.EnvironmentName.Equals("testing"))
            {
                services.AddAuthentication();
            }
            else
            {
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
            }

            // Cookies setup
            services.Configure<CookiePolicyOptions>(
                options =>
                {
                    options.CheckConsentNeeded = context => true;
                });

            // TODO: this doesn't seem to be used anywhere
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
            services.AddTransient<IAppointmentService, AppointmentService>();
            services.AddTransient<ICommentService, CommentService>();

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
