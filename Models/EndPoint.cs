using EasyMongoNet;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TciDataLinks.Models
{
    public class EndPoint : MongoEntity
    {
        public enum PortTypeEnum
        {
            Electrical,
            Optical,
            Coaxial
        }
        public enum SpeedUnitEnum
        {
            Kbps,
            Mbps,
            Gbps,
        }

        public ObjectId Device { get; set; }
        [BsonRepresentation(BsonType.String)]
        public PortTypeEnum PortType { get; set; }
        public string PortNumber { get; set; }
        public int Speed { get; set; }
        [BsonRepresentation(BsonType.String)]
        public SpeedUnitEnum SpeedUnit { get; set; }


    }
}
