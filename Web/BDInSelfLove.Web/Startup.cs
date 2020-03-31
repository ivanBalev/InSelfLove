namespace BDInSelfLove.Web
{
    using System.Reflection;

    using BDInSelfLove.Data;
    using BDInSelfLove.Data.Common;
    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Data.Repositories;
    using BDInSelfLove.Data.Seeding;
    using BDInSelfLove.Services.Data;
    using BDInSelfLove.Services.Data.Category;
    using BDInSelfLove.Services.Data.CloudinaryService;
    using BDInSelfLove.Services.Data.Comment;
    using BDInSelfLove.Services.Data.Post;
    using BDInSelfLove.Services.Data.Product;
    using BDInSelfLove.Services.Data.User;
    using BDInSelfLove.Services.Data.Video;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Messaging;
    using BDInSelfLove.Services.Models.Article;
    using BDInSelfLove.Web.Infrastructure.ModelBinders;
    using BDInSelfLove.Web.ViewComponents.Models.Video;
    using BDInSelfLove.Web.ViewModels;
    using BDInSelfLove.Web.ViewModels.Article;
    using CloudinaryDotNet;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

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

            // External Logins
            // TODO: Request different info from users preferring to use facebook login https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/facebook-logins?view=aspnetcore-3.1
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

            services.Configure<CookiePolicyOptions>(
                options =>
                    {
                        options.CheckConsentNeeded = context => true;
                        options.MinimumSameSitePolicy = SameSiteMode.None;
                    });

            services.AddControllersWithViews(configure =>
           {
               configure.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
               // FILTERS EXERCISE
               // GLOBAL SCOPE OF FILTER - APPLIES EVERY TIME(BEFORE/AFTER EACH REQUEST)
               // configure.Filters.Add(new AddHeaderActionFilterAttribute());

               // BINDING EXERCISE
               configure.ModelBinderProviders.Insert(0, new YearModelBinderProvider());
           });

            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
            });

            services.AddRazorPages()
                .AddRazorRuntimeCompilation();

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
            services.AddTransient<ICategoryService, CategoryService>();
            services.AddTransient<IPostService, PostService>();
            services.AddTransient<ICommentService, CommentService>();
            services.AddTransient<ICloudinaryService, CloudinaryService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IProductService, ProductService>();

            // FILTERS EXERCISE
            // Allows control over instantiation of filter.
            // APPLIES TO LOCAL USES OF FILTERS AS ATTRIBUTES AS OPPOSED TO THE GLOBAL IMPLEMENTATIONN IN THE .AddControllersWithViews method above
            // services.AddSingleton<AddHeaderAsyncActionFilterAttribute>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            AutoMapperConfig.RegisterMappings(
                typeof(ErrorViewModel).GetTypeInfo().Assembly,
                typeof(ArticleServiceModel).GetTypeInfo().Assembly,
                typeof(ArticleViewModel).GetTypeInfo().Assembly,
                typeof(HomeVideoViewModel).GetTypeInfo().Assembly);

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
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseResponseCompression();
            // MIDDLEWARE EXERCISE
            // app.UseMiddleware<RedirectToGoogleIfNotHttps>();
            // MIDDLEWARE EXERCISE

            // TODO: Get rid of this after done with SPA frontend - SECURITY RISK
            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                DefaultContentType = "text/plain",
            });
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(
                endpoints =>
                    {
                        endpoints.MapControllerRoute("areaRoute", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                        endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

                        // MAPPING EXERCISE
                        // endpoints.MapControllerRoute(
                        //    name: "exampleName",
                        //    pattern: "Example/{slug}/{id:int}",
                        //    defaults: new { controller = "Example", action = "ExampleAction" });

                        endpoints.MapRazorPages();
                    });
        }
    }
}
