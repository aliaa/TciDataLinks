using EasyMongoNet;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using TciDataLinks.Models;

namespace TciDataLinks.ViewModels
{
    public class EndPointViewModel : EndPoint
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Center { get; set; }

        public List<PassiveConnectionViewModel> PassiveConnectionViewModels { get; set; } = new List<PassiveConnectionViewModel>();

        public string GetPlaceDisplay(IReadOnlyDbContext db)
        {
            var device = db.FindById<Device>(Device);
            return device.GetPlaceDisplay(db);
        }
    }
}
