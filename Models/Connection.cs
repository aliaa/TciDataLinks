using EasyMongoNet;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TciDataLinks.Models
{
    [CollectionSave(WriteLog = true)]
    [CollectionIndex(new string[] { nameof(IdInt) }, Unique = true)]
    public class Connection : MongoEntity
    {
        public int IdInt { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string CustomerId { get; set; }

        public string CustomerIcon { get; set; }
    }
}
