using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using AliaaCommon;
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
using Omu.ValueInjecter;
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
                        options.SerializerSettings.Converters.Add(new ObjectIdJsonConverter());
                        options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                        // Maintain property names during serialization. See:
                        // https://github.com/aspnet/Announcements/issues/194
                        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    })
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddSingleton<HtmlEncoder>(HtmlEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic));

            var settings = Configuration.GetSection("Settings").Get<Settings>();
            services.AddSingleton(settings);

            // Add mongodb service:
            string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "PersianCharsMap.json");
            var stringNormalizer = new StringNormalizer(filePath);
            services.AddSingleton(stringNormalizer);

            var connString = Configuration.GetValue<string>("MongoConnString");
            var customeConnections = Configuration.GetSection("CustomConnections").Get<List<CustomMongoConnection>>();
            foreach (var cc in customeConnections)
                if (cc.ConnectionString == null)
                    cc.ConnectionString = connString;

            var db = new MongoDbContext(
                Configuration.GetValue<string>("DBName"), connString,
                customConnections: customeConnections,
                objectPreprocessor: stringNormalizer);
            services.AddSingleton<IDbContext>(db);
            services.AddSingleton<IReadOnlyDbContext>(db);

            var cities = db.FindGetResults<City>(c => c.Province == ObjectId.Parse(settings.ProvinceId));
            services.AddSingleton(cities);

            services.Configure<IISServerOptions>(options =>
            {
                options.AutomaticAuthentication = false;
            });

            ConfigureMapper();
        }

        private static void ConfigureMapper()
        {
            Mapper.DefaultMap = (src, resType, tag) =>
            {
                var res = Activator.CreateInstance(resType);
                res.InjectFrom(src);

                var srcTypeProps = src.GetType().GetProperties();
                var resTypeProps = resType.GetProperties();

                foreach (var resProp in resTypeProps.Where(p => p.PropertyType == typeof(ObjectId)))
                {
                    var matchSrcProp = srcTypeProps.FirstOrDefault(p => p.Name == resProp.Name && p.PropertyType == typeof(string));
                    if (matchSrcProp != null)
                    {
                        string id = (string)matchSrcProp.GetValue(src);
                        if (ObjectId.TryParse(id, out ObjectId objId))
                            resProp.SetValue(res, objId);
                    }
                }
                foreach (var srcProp in srcTypeProps.Where(p => p.PropertyType == typeof(ObjectId)))
                {
                    var matchResProp = resTypeProps.FirstOrDefault(p => p.Name == srcProp.Name && p.PropertyType == typeof(string));
                    if (matchResProp != null)
                    {
                        var objId = (ObjectId)srcProp.GetValue(src);
                        matchResProp.SetValue(res, objId.ToString());
                    }
                }
                return res;
            };
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
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
