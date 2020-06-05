using EasyMongoNet;
using MongoDB.Bson;
using System;

namespace TciDataLinks.Models
{
    [CollectionIndex(new string[] { nameof(IdInt) }, Unique = true)]
    public class Connection : MongoEntity
    {
        public int IdInt { get; set; }
    }
}
