using EasyMongoNet;
using MongoDB.Bson;
using System.Text;
using TciCommon.Models;

namespace TciDataLinks.Models
{
    public abstract class BaseDevice : MongoEntity
    {
        public ObjectId Rack { get; set; }
        public int RackRow { get; set; }

        public string GetPlaceDisplay(IReadOnlyDbContext db)
        {
            var rack = db.FindById<Rack>(Rack);
            var room = db.FindById<Room>(rack.Parent);
            var building = db.FindById<Building>(room.Parent);
            var center = db.FindById<CommCenter>(building.Parent); ;
            var city = db.FindById<City>(center.City);

            StringBuilder sb = new StringBuilder();
            sb.Append(city.Name).Append(" &lArr; ")
                .Append("مرکز ").Append(center.Name).Append(" &lArr; ")
                .Append("ساختمان ").Append(building.Name).Append(" &lArr; ")
                .Append("اتاق/سالن ").Append(room.Name).Append(" &lArr; ")
                .Append("راک ").Append(rack.Name).Append(" &lArr; ")
                .Append("دستگاه ").Append(ToString());
            return sb.ToString();
        }
    }
}
