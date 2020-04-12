using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TciCommon.Models;

namespace TciDataLinks.Models
{
    public class CentersViewModel
    {
        public Dictionary<string, List<CommCenter>> Centers { get; set; }
    }
}
