using AliaaCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using TciDataLinks.Models;

namespace TciDataLinks.ViewModels
{

    public class PlaceViewModel
    {
        public PlaceType Type { get; set; }
        public PlaceBase Current
        {
            get
            {
                switch (Type)
                {
                    case PlaceType.City:
                        return City;
                    case PlaceType.Center:
                        return Center;
                    case PlaceType.Building:
                        return Building;
                    case PlaceType.Room:
                        return Room;
                    case PlaceType.Rack:
                        return Rack;
                    default:
                        return null;
                }
            }
        }

        public PlaceType NextLevelType
        {
            get
            {
                switch (Type)
                {
                    case PlaceType.City:
                        return PlaceType.Center;
                    case PlaceType.Center:
                        return PlaceType.Building;
                    case PlaceType.Building:
                        return PlaceType.Room;
                    case PlaceType.Room:
                        return PlaceType.Rack;
                    case PlaceType.Rack:
                        return PlaceType.Device;
                    default:
                        throw new Exception("No next level is defined.");
                }
            }
        }

        public PlaceBase City { get; set; }
        public PlaceBase Center { get; set; }
        public PlaceBase Building { get; set; }
        public PlaceBase Room { get; set; }
        public PlaceBase Rack { get; set; }
        public IEnumerable<PlaceBase> SubItems { get; set; } = Enumerable.Empty<PlaceBase>();
        public IEnumerable<NonNetworkItem> NonNetworkItems { get; set; } = Enumerable.Empty<NonNetworkItem>();

        public override string ToString()
        {
            return Utils.DisplayName(Type) + " " + Current.ToString();
        }
    }
}
