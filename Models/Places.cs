using EasyMongoNet;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TciDataLinks.Models
{
    public enum PlaceType
    {
        [Display(Name = "شهر")]
        City,
        [Display(Name = "مرکز")]
        Center,
        [Display(Name = "ساختمان")]
        Building,
        [Display(Name = "سالن/اتاق")]
        Room,
        [Display(Name = "راک")]
        Rack,
        [Display(Name = "دستگاه")]
        Device,
        [Display(Name = "رابط Passive")]
        PatchPanel
    }

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
}
