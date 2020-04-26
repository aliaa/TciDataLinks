using EasyMongoNet;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TciDataLinks.Models
{
    [CollectionIndex(new string[] { nameof(Parent) })]
    public class PlaceBase : MongoEntity
    {
        public ObjectId Parent { get; set; }
        public virtual string Name { get; set; }

        [BsonIgnore]
        public readonly PlaceType PlaceType;

        public PlaceBase(PlaceType type)
        {
            PlaceType = type;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class Building : PlaceBase 
    {
        public Building() : base(PlaceType.Building) { }
    }

    public class Room : PlaceBase 
    {
        public Room() : base(PlaceType.Room) { }
    }

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

        [Display(Name = "ظرفیت")]
        public int Capacity { get; set; } = 46;

        [BsonRepresentation(BsonType.String)]
        public RackType Type { get; set; } = RackType.Normal19Inch;

        [Display(Name = "ردیف")]
        public int Line { get; set; }

        [Display(Name = "شماره")]
        public int Index { get; set; }

        [BsonIgnore]
        public override string Name 
        { 
            get => Line + "/" + Index; 
            set
            {
                var vals = value.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (vals.Length < 2)
                    throw new Exception();
                Line = int.Parse(vals[0]);
                Index = int.Parse(vals[1]);
            }
        }

        public override string ToString()
        {
            return "ردیف " + Line + " شماره " + Index;
        }
    }
}
