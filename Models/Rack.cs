using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TciDataLinks.Models
{
    public class Rack : PlaceBase
    {
        public Rack() : base(PlaceType.Rack) { }

        public enum RackType
        {
            [Display(Name = "19 اینچی")]
            Normal19Inch,
            DDF,
            OCDF,
            ODF,
        }

        public enum RackSide { A, B }

        [Display(Name = "ظرفیت")]
        public int Capacity { get; set; } = 46;

        [BsonRepresentation(BsonType.String)]
        public RackType Type { get; set; } = RackType.Normal19Inch;

        [Display(Name = "ردیف")]
        public int Line { get; set; }

        [Display(Name = "شماره")]
        public int Index { get; set; }

        [Display(Name = "سمت")]
        [BsonRepresentation(BsonType.String)]
        public RackSide Side { get; set; }

        [BsonIgnore]
        public override string Name
        {
            get => "ردیف " + Line + " شماره " + Index + " سمت " + Side;
            set { }
        }

        public override string ToString()
        {
            return Line + "-" + Index + "-" + Side;
        }
    }
}
