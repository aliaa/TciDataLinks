using EasyMongoNet;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using TciCommon.Models;
using TciDataLinks.Models;

namespace TciDataLinks
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
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).
                AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                });

            string permissionClaimName = nameof(Permission);
            services.AddAuthorization(options =>
            {
                foreach (string perm in Enum.GetNames(typeof(Permission)))
                    options.AddPolicy(perm, policy => policy.RequireAssertion(context =>
                    {
                        var permClaim = context.User.Claims.FirstOrDefault(c => c.Type == permissionClaimName);
                        return permClaim != null && permClaim.Value.Contains(perm);
                    }));
                options.AddPolicy("Admin", policy => policy.RequireClaim("IsAdmin"));
            });

            var mvcBuilder = services.AddControllersWithViews(config =>
            {
                config.ModelBinderProviders.Insert(0, new ObjectIdModelBinderProvider());
            });
#if (DEBUG)
            mvcBuilder.AddRazorRuntimeCompilation();
#endif
            services.AddRazorPages()
                .AddNewtonsoftJson(
                    options =>
                    {
                        options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                        // Maintain property names during serialization. See:
                        // https://github.com/aspnet/Announcements/issues/194
                        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    })
                .SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddControllersAsServices();

            services.AddSingleton<HtmlEncoder>(HtmlEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic));

            var settings = Configuration.GetSection("Settings").Get<Settings>();
            services.AddSingleton(settings);

            var db = services.AddMongDbContext(Configuration);
            var cities = db.FindGetResults<City>(c => c.Province == settings.ProvinceId);
            services.AddSingleton(cities);

            services.Configure<IISServerOptions>(options =>
            {
                options.AutomaticAuthentication = false;
            });

            ConfigureMapper.Configure();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Graph}/{action=Index}/{id?}");
            });
        }
    }
}
