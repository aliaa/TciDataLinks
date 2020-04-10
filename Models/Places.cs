using EasyMongoNet;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TciDataLinks.Models
{
    [CollectionIndex(new string[] { nameof(Parent) })]
    public abstract class PlaceBase : MongoEntity
    {
        public ObjectId Parent { get; set; }
        public string Name { get; set; }
    }

    public class Building : PlaceBase { }

    public class Room : PlaceBase { }

    public class Rack : PlaceBase
    {
        [Display(Name = "ظرفیت")]
        public int Capacity { get; set; }
    }
}
