﻿using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TciDataLinks.Models
{
    public class ConnectionViewModel : Connection
    {
        public List<EndPointViewModel> EndPoints { get; set; } = new List<EndPointViewModel>();
    }
}
