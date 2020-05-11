using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
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
            get => Line + "/" + Index + "/" + Side;
            set
            {
                var vals = value.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (vals.Length < 3)
                    throw new Exception();
                Line = int.Parse(vals[0]);
                Index = int.Parse(vals[1]);
                Side = (RackSide)Enum.Parse(typeof(RackSide), vals[2]);
            }
        }

        public override string ToString()
        {
            return "ردیف " + Line + " شماره " + Index + " سمت " + Side;
        }
    }
}
