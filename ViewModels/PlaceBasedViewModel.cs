using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using TciDataLinks.Models;

namespace TciDataLinks.ViewModels
{
    public abstract class PlaceBasedViewModel
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [Display(Name = "نوع محل نصب")]
        public BaseDevice.DevicePlaceType PlaceType { get; set; }

        [Display(Name = "محل نصب")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Place { get; set; }

        [Required(ErrorMessage = "انتخاب شهر الزامی می باشد!")]
        [Display(Name = "شهر")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string City { get; set; }

        [Required(ErrorMessage = "انتخاب مرکز الزامی می باشد!")]
        [Display(Name = "مرکز")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Center { get; set; }

        [Required(ErrorMessage = "نام ساختمان الزامی می باشد!")]
        [Display(Name = "ساختمان")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Building { get; set; }

        [Required(ErrorMessage = "نام اتاق الزامی می باشد!")]
        [Display(Name = "اتاق")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Room { get; set; }

        [Required(ErrorMessage = "ردیف راک الزامی میباشد!")]
        [Display(Name = "ردیف راک")]
        public int RackLine { get; set; }

        [Required(ErrorMessage = "شماره راک الزامی میباشد!")]
        [Display(Name = "شماره راک")]
        public int RackIndex { get; set; }

        [Display(Name = "سمت راک")]
        public Rack.RackSide RackSide { get; set; }

        [Display(Name = "شماره ردیف داخل راک")]
        public int RackRow { get; set; }
    }
}
