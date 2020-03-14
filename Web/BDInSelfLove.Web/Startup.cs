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
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Messaging;
    using BDInSelfLove.Web.Filters;
    using BDInSelfLove.Web.Infrastructure.ModelBinders;
    using BDInSelfLove.Web.Middlewares;
    using BDInSelfLove.Web.ViewModels;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
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

            services.Configure<CookiePolicyOptions>(
                options =>
                    {
                        options.CheckConsentNeeded = context => true;
                        options.MinimumSameSitePolicy = SameSiteMode.None;
                    });

            services.AddControllersWithViews( configure =>
            {
                // FILTERS EXERCISE
                // GLOBAL SCOPE OF FILTER - APPLIES EVERY TIME(BEFORE/AFTER EACH REQUEST)
                // configure.Filters.Add(new AddHeaderActionFilterAttribute());
                
                // BINDING EXERCISE
                configure.ModelBinderProviders.Insert(0, new YearModelBinderProvider());
            });
            services.AddRazorPages()
                .AddRazorRuntimeCompilation();

            services.AddSingleton(this.configuration);

            // Data repositories
            services.AddScoped(typeof(IDeletableEntityRepository<>), typeof(EfDeletableEntityRepository<>));
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped<IDbQueryRunner, DbQueryRunner>();

            // Application services
            services.AddTransient<IEmailSender>(x => new SendGridEmailSender(this.configuration["SendGrid:ApiKey"]));
            services.AddTransient<ISettingsService, SettingsService>();
            // FILTERS EXERCISE
            // Allows control over instantiation of filter.
            // APPLIES TO LOCAL USES OF FILTERS AS ATTRIBUTES AS OPPOSED TO THE GLOBAL IMPLEMENTATIONN IN THE .AddControllersWithViews method above
            // services.AddSingleton<AddHeaderAsyncActionFilterAttribute>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            AutoMapperConfig.RegisterMappings(typeof(ErrorViewModel).GetTypeInfo().Assembly);

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
            // MIDDLEWARE EXERCISE
            app.UseMiddleware<RedirectToGoogleIfNotHttps>();
            // MIDDLEWARE EXERCISE
            app.UseStaticFiles();
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
                        //endpoints.MapControllerRoute(
                        //    name: "exampleName",
                        //    pattern: "Example/{slug}/{id:int}",
                        //    defaults: new { controller = "Example", action = "ExampleAction" });

                        endpoints.MapRazorPages();
                    });
        }
    }
}
