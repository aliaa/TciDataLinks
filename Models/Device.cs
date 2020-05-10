using AliaaCommon;
using EasyMongoNet;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TciCommon.Models;

namespace TciDataLinks.Models
{
    [CollectionIndex(new string[] { nameof(Address) })]
    public class Device : BaseDevice
    {
        public enum DeviceType
        {
            Router,
            [Display(Name = "Ethernet Switch")]
            EthernetSwitch,
            BRAS,
            [Display(Name = "Wireless Controller")]
            WirelessController,
            DSLAM,
            MSAN,
            OLT,
            Tellabs,
            Server,
            Firewall
        }

        public enum NetworkType
        {
            Cisco,
            Huawei,
            ZTE,
            Tellabs
        }

        [BsonRepresentation(BsonType.String)]
        public DeviceType Type { get; set; }
        [BsonRepresentation(BsonType.String)]
        public NetworkType Network { get; set; }
        public string Model { get; set; }
        public string Address { get; set; }

        public override string ToString()
        {
            return Utils.GetDisplayNameOfMember(typeof(DeviceType), Type.ToString()) + " " + Model;
        }

        public string GetPlaceDisplay(IReadOnlyDbContext db)
        {
            var rack = db.FindById<Rack>(Rack);
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
                .Append("دستگاه ").Append(ToString());
            return sb.ToString();
        }
    }
}
