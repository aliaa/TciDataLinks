using System.Collections.Generic;
using TciCommon.Models;
using TciDataLinks.Models;

namespace TciDataLinks.ViewModels
{
    public class PlaceIndexViewModel
    {
        public Dictionary<string, List<CommCenter>> Centers { get; set; }

        public List<DeviceViewModel> DeviceSearchResult { get; set; }

        public List<PassiveViewModel> PassiveSearchResult { get; set; }
    }
}
