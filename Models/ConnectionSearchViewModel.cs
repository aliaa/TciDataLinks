using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TciDataLinks.Models
{
    public class ConnectionSearchViewModel
    {
        public enum EndPointSearchType
        {
            [Display(Name = "همه اتصال ها")]
            All,
            [Display(Name = "فقط اتصال اول")]
            First,
            [Display(Name = "اتصالهای بعدی")]
            NotFirst,
            //[Display(Name = "فقط اتصال آخر")]
            //Last,
        }

        [Required(ErrorMessage = "انتخاب شهر اجباری است")]
        [Display(Name = "شهر")]
        public string City { get; set; }

        [Required(ErrorMessage = "انتخاب مرکز اجباری است")]
        [Display(Name = "مرکز")]
        public string Center { get; set; }

        [Display(Name = "ساختمان")]
        public string Building { get; set; }

        [Display(Name = "اتاق")]
        public string Room { get; set; }

        [Display(Name = "راک")]
        public string Rack { get; set; }

        [Display(Name = "دستگاه")]
        public string Device { get; set; }

        [Display(Name = "نحوه جستجوی اتصالها")]
        public EndPointSearchType SearchType { get; set; }

        public List<ConnectionViewModel> SearchResult { get; set; } = new List<ConnectionViewModel>();
    }
}
