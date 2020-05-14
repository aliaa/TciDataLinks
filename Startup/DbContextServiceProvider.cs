using AliaaCommon;
using EasyMongoNet;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TciDataLinks
{
    public static class DbContextServiceProvider
    {
        public static IDbContext AddMongDbContext(this IServiceCollection services, IConfiguration config)
        {
            string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), StringNormalizer.JSON_FILE_NAME);
            var stringNormalizer = new StringNormalizer(filePath);
            services.AddSingleton(stringNormalizer);

            var connString = config.GetValue<string>("MongoConnString");
            var customeConnections = config.GetSection("CustomConnections").Get<List<CustomMongoConnection>>();
            foreach (var cc in customeConnections)
                if (cc.ConnectionString == null)
                    cc.ConnectionString = connString;

            var db = new MongoDbContext(
                config.GetValue<string>("DBName"), connString,
                customConnections: customeConnections,
                objectPreprocessor: stringNormalizer);
            services.AddSingleton<IDbContext>(db);
            services.AddSingleton<IReadOnlyDbContext>(db);
            return db;
        }
    }
}
