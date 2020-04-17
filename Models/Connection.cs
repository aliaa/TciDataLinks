using EasyMongoNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TciDataLinks.Models
{
    public class Connection : MongoEntity
    {
        public List<EndPoint> EndPoints { get; set; } = new List<EndPoint>();
    }
}
