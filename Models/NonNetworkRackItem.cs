using AliaaCommon;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TciDataLinks.Models
{
    public class NonNetworkRackItem : NonNetworkItem
    {
        public enum NonNetworkRackItemType
        {
            [Display(Name = "اینورتر راکی")]
            Inverter,
            [Display(Name = "PDB راکی")]
            PDB,
        }

        public override bool IsRackItem => true;

        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        [Display(Name = "نوع")]
        public NonNetworkRackItemType Type { get; set; }

        public override string ToString()
        {
            return DisplayUtils.DisplayName(Type) + " \"" + Name + "\"  تعداد: " + Count ;
        }
    }
}
