using EasyMongoNet;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TciCommon.Models;

namespace TciDataLinks.Models
{
    public class EndPointViewModel : EndPoint
    {
        public int Index { get; set; }

        public ObjectId Room { get; set; }
        public ObjectId Rack { get; set; }

        public List<PassiveConnectionViewModel> PassiveConnectionViewModels { get; set; } = new List<PassiveConnectionViewModel>();

        public string GetPlaceDisplayName(IReadOnlyDbContext db)
        {
            var device = db.FindById<Device>(Device);
            var rack = db.FindById<Rack>(device.Rack);
            var room = db.FindById<Room>(rack.Parent);
            var building = db.FindById<Building>(room.Parent);
            var center = db.FindById<CommCenter>(building.Parent); ;
            var city = db.FindById<City>(center.City);

            StringBuilder sb = new StringBuilder();
            sb.Append(city.Name).Append(" > ")
                .Append("مرکز ").Append(center.Name).Append(" > ")
                .Append("ساختمان ").Append(building.Name).Append(" > ")
                .Append("اتاق/سالن ").Append(room.Name).Append(" > ")
                .Append("راک ").Append(rack.Name).Append(" > ")
                .Append("دستگاه ").Append(device.ToString());
            return sb.ToString();
        }
    }
}
