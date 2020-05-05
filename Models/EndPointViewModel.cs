using EasyMongoNet;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Text;
using TciCommon.Models;

namespace TciDataLinks.Models
{
    public class EndPointViewModel : EndPoint
    {
        public ObjectId Building { get; set; }

        public List<PassiveConnectionViewModel> PassiveConnectionViewModels { get; set; } = new List<PassiveConnectionViewModel>();

        public string GetPlaceDisplay(IReadOnlyDbContext db)
        {
            var device = db.FindById<Device>(Device);
            return device.GetPlaceDisplay(db);
        }
    }
}
