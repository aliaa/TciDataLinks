using EasyMongoNet;
using MongoDB.Bson;

namespace TciDataLinks.Models
{
    [CollectionSave(WriteLog = true)]
    [CollectionIndex(new string[] { nameof(IdInt) }, Unique = true)]
    public class Connection : MongoEntity
    {
        public int IdInt { get; set; }

        public ObjectId CustomerId { get; set; }

        public string CustomerIcon { get; set; }
    }
}
