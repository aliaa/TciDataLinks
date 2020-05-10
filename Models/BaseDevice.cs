using EasyMongoNet;
using MongoDB.Bson;

namespace TciDataLinks.Models
{
    public abstract class BaseDevice : MongoEntity
    {
        public ObjectId Rack { get; set; }
        public int RackRow { get; set; }
    }
}
