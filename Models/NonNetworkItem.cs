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
        [BsonRepresentation(BsonType.ObjectId)]
        public string Place { get; set; }

        [Display(Name = "نام")]
        public string Name { get; set; }

        [Display(Name = "تعداد")]
        [Range(minimum: 1, maximum: 100)]
        public int Count { get; set; } = 1;

        [BsonIgnore]
        public abstract bool IsRackItem { get; }
    }
}
