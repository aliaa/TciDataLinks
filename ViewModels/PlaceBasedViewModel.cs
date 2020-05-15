using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;
using TciDataLinks.Models;

namespace TciDataLinks.ViewModels
{
    public abstract class PlaceBasedViewModel
    {
        public ObjectId Id { get; set; }

        [Required(ErrorMessage = "انتخاب شهر الزامی می باشد!")]
        [Display(Name = "شهر")]
        public ObjectId City { get; set; }

        [Required(ErrorMessage = "انتخاب مرکز الزامی می باشد!")]
        [Display(Name = "مرکز")]
        public ObjectId Center { get; set; }

        [Required(ErrorMessage = "نام ساختمان الزامی می باشد!")]
        [Display(Name = "ساختمان")]
        public string Building { get; set; }

        [Required(ErrorMessage = "نام اتاق الزامی می باشد!")]
        [Display(Name = "اتاق")]
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

        public ObjectId Rack { get; set; }
    }
}
