using AliaaCommon;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TciDataLinks.Models
{
    public class NonNetworkRoomItem : NonNetworkItem
    {
        public enum NonNetworkRoomItemType
        {
            [Display(Name = "کولر ایستاده")]
            StandingAirConditioner,
            [Display(Name = "کولر دیواری")]
            WallAirConditioner,
            [Display(Name = "اینورتر بدون راک")]
            Inverter,
            [Display(Name = "PDB دیواری")]
            WallPDB,
            [Display(Name = "PDB انتهای ردیف")]
            EndRowPDB,
        }

        public override bool IsRackItem => false;

        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        [Display(Name = "نوع")]
        public NonNetworkRoomItemType Type { get; set; }

        public override string ToString()
        {
            return Utils.DisplayName(Type) + " \"" + Name + "\"  تعداد: " + Count;
        }
    }
}
