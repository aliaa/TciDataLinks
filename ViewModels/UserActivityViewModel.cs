using EasyMongoNet.Model;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TciDataLinks.ViewModels
{
    public class UserActivityViewModel
    {
        public string User { get; set; }

        public DateTime Time { get; set; } = DateTime.Now;

        public ActivityType ActivityType { get; set; }

        public string Type { get; set; }

        public ObjectId ObjId { get; set; }
    }
}
