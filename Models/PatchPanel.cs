using EasyMongoNet;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TciDataLinks.Models
{
    public class PatchPanel : MongoEntity
    {
        public enum PatchPanelType
        {
            PP_RJ45,
            PP_FC,
            PP_SC,
            PP_LC,
            OCDF_SC,
            OCDF_FC,
            DDF_COAX
        }

        public ObjectId Rack { get; set; }
        public int RackRow { get; set; }

        [BsonRepresentation(BsonType.String)]
        public PatchPanelType Type { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return RackRow + " : " + Name;
        }
    }
}
