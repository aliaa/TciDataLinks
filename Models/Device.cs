using AliaaCommon;
using EasyMongoNet;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TciDataLinks.Models
{
    [CollectionIndex(new string[] { nameof(Address) })]
    public class Device : MongoEntity
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

        public ObjectId Rack { get; set; }
        public int RackRow { get; set; }
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
    }
}
