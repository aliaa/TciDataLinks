using EasyMongoNet;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TciDataLinks.Models
{
    [CollectionIndex(new string[] { nameof(Place) })]
    [BsonKnownTypes(typeof(NonNetworkRackItem), typeof(NonNetworkRoomItem))]
    public abstract class NonNetworkItem : MongoEntity
    {
        public ObjectId Place { get; set; }

        [Display(Name = "نام")]
        public string Name { get; set; }

        [BsonIgnore]
        public abstract bool IsRackItem { get; }
    }
}
