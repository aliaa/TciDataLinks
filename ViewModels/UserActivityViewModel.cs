using EasyMongoNet.Model;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TciDataLinks.ViewModels
{
    public class UserActivityViewModel
    {
        public string User { get; set; }

        public DateTime Time { get; set; } = DateTime.Now;

        public ActivityType ActivityType { get; set; }

        public string Type { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ObjId { get; set; }
    }
}
