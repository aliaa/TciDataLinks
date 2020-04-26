using AliaaCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TciDataLinks.Models
{
    public enum PlaceType
    {
        [Display(Name = "شهر")]
        City,
        [Display(Name = "مرکز")]
        Center,
        [Display(Name = "ساختمان")]
        Building,
        [Display(Name = "سالن/اتاق")]
        Room,
        [Display(Name = "راک")]
        Rack,
        [Display(Name = "دستگاه")]
        Device,
        [Display(Name = "رابط Passive")]
        PatchPanel
    }

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
        public IEnumerable<PlaceBase> SubItems { get; set; }

        public override string ToString()
        {
            return Utils.GetDisplayNameOfMember(typeof(PlaceType), Type.ToString()) + " " + Current.ToString();
        }
    }
}
