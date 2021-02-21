using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TciDataLinks.ViewModels
{
    public class PortViewModel
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Connection { get; set; }
        public int EndPointIndex { get; set; }
        public string PortNumber { get; set; }
    }
}
