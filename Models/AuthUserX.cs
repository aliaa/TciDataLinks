using AliaaCommon.Models;
using EasyMongoNet;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TciDataLinks.Models
{
    [CollectionOptions(Name = nameof(AuthUser))]
    public class AuthUserX : AuthUser
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public List<Permission> Permissions { get; set; } = new List<Permission>();

    }

    public static class AuthUserXDBExtention
    {
        public static AuthUserX CheckAuthentication(this IDbContext DB, string username, string password, bool passwordIsHashed = false)
        {
            string hash;
            if (passwordIsHashed)
                hash = password;
            else
                hash = AuthUserDBExtention.GetHash(password);
            return DB.FindFirst<AuthUserX>(u => u.Username == username && u.HashedPassword == hash && u.Disabled != true);
        }
    }
}
