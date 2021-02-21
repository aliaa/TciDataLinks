using AliaaCommon;
using EasyMongoNet;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TciCommon.Models;

namespace TciDataLinks.Models
{
    public abstract class BaseDevice : MongoEntity
    {
        public enum DevicePlaceType
        {
            [Display(Name = "راک")]
            Rack,
            [Display(Name = "کافو")]
            Kafu
        }

        [Display(Name = "نوع محل نصب")]
        [BsonRepresentation(BsonType.String)]
        public DevicePlaceType PlaceType { get; set; }

        [Display(Name = "محل نصب")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Place { get; set; }

        public int RackRow { get; set; }

        public string GetPlaceDisplay(IReadOnlyDbContext db)
        {
            StringBuilder sb = new StringBuilder();
            if (PlaceType == DevicePlaceType.Rack)
            {
                var rack = db.FindById<Rack>(Place);
                var room = db.FindById<Room>(rack.Parent);
                var building = db.FindById<Building>(room.Parent);
                var center = db.FindById<CommCenter>(building.Parent);
                var city = db.FindById<City>(center.City);

                sb.Append(city.Name).Append(" &lArr; ")
                    .Append("مرکز ").Append(center.Name).Append(" &lArr; ")
                    .Append("ساختمان ").Append(building.Name).Append(" &lArr; ")
                    .Append("اتاق/سالن ").Append(room.Name).Append(" &lArr; ")
                    .Append("راک ").Append(rack.Name).Append(" &lArr; ")
                    .Append("دستگاه ").Append(ToString());
            }
            else if (PlaceType == DevicePlaceType.Kafu)
            {
                var kafu = db.FindById<Kafu>(Place);
                var center = db.FindById<CommCenter>(kafu.CommCenter);
                var city = db.FindById<City>(center.City);

                sb.Append(city.Name).Append(" &lArr; ")
                    .Append("مرکز ").Append(center.Name).Append(" &lArr; ")
                    .Append("کافو ").Append(DisplayUtils.DisplayName(kafu.Type))
                    .Append(" \"").Append(kafu.Name).Append("\"");
            }
            else
                throw new NotImplementedException();

            return sb.ToString();
        }

        public string GetCenterId(IReadOnlyDbContext db)
        {
            if (PlaceType == DevicePlaceType.Rack)
            {
                var rack = db.FindById<Rack>(Place);
                var room = db.FindById<Room>(rack.Parent);
                var building = db.FindById<Building>(room.Parent);
                return building.Parent;
            }
            else if (PlaceType == DevicePlaceType.Kafu)
            {
                var kafu = db.FindById<Kafu>(Place);
                return kafu.CommCenter;
            }
            throw new NotImplementedException();
        }
    }
}
