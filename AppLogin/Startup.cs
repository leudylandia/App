using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLogin.Helpers;
using AppLogin.Models;
using AppLogin.Models.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppLogin
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<LoginDbContext>(option => option.UseSqlServer(Configuration.GetConnectionString("DefaultC")));

            services.AddTransient<SeedDb>();

            //Inject
            services.AddScoped<IUserHelper, UserHelper>();

            services.AddIdentity<User, IdentityRole>(cfg =>
            {
                cfg.User.RequireUniqueEmail = true;
                cfg.Password.RequireDigit = false;
                cfg.Password.RequiredUniqueChars = 0;
                cfg.Password.RequireNonAlphanumeric = false;
                cfg.Password.RequireUppercase = false;
                cfg.Password.RequiredLength = 4;
                cfg.Password.RequireLowercase = false;
            }).AddEntityFrameworkStores<LoginDbContext>().AddDefaultTokenProviders();


           // services.AddIdentity<User, IdentityRole>(cfg =>
           //{
           //    cfg.User.RequireUniqueEmail = true;
           //    cfg.Password.RequireDigit = false;
           //    cfg.Password.RequiredUniqueChars = 0;
           //    cfg.Password.RequireNonAlphanumeric = false;
           //    cfg.Password.RequireUppercase = false;
           //    cfg.Password.RequiredLength = 4;
           //});

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
