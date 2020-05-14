using EasyMongoNet;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TciDataLinks.Models
{
    [CollectionSave(WriteLog = true)]
    public class PatchPanel : BaseDevice
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

        [BsonRepresentation(BsonType.String)]
        public PatchPanelType Type { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
