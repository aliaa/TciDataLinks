using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TciDataLinks.ViewModels
{
    public class PortViewModel
    {
        public ObjectId Connection { get; set; }
        public int EndPointIndex { get; set; }
        public string PortNumber { get; set; }
    }
}
