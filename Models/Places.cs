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
        public string Name { get; set; }

        [BsonIgnore]
        public readonly PlaceType PlaceType;

        public PlaceBase(PlaceType type)
        {
            PlaceType = type;
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
    }
}
