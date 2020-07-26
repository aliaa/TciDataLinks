using EasyMongoNet;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TciDataLinks.Models
{
    [CollectionSave(WriteLog = true)]
    public class Passive : BaseDevice
    {
        public enum PassiveTypeEnum
        {
            [Display(Name = "پچ پنل")]
            PatchPanel,
            [Display(Name = "تجهیزات انتقال")]
            Transmissional
        }

        public enum PatchPanelTypeEnum
        {
            [Display(Name = "مشخص نشده")]
            None,
            PP_RJ45,
            PP_FC,
            PP_SC,
            PP_LC,
            OCDF_SC,
            OCDF_FC,
            DDF_COAX
        }

        [BsonRepresentation(BsonType.String)]
        public PassiveTypeEnum Type { get; set; }

        [BsonRepresentation(BsonType.String)]
        public PatchPanelTypeEnum PatchPanelType { get; set; }

        [BsonRepresentation(BsonType.String)]
        public TransmissionSystemType TransmissionType { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
