using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TciDataLinks.Models
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

        [Required(ErrorMessage = "نام راک الزامی میباشد!")]
        [Display(Name = "راک")]
        public string Rack { get; set; }

        [Required(ErrorMessage = "شماره ردیف راک الزامی می باشد!")]
        [Display(Name = "شماره ردیف راک")]
        public int RackRow { get; set; }

    }
}
