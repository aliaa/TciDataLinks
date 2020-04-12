using AliaaCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TciDataLinks.Models
{
    public class DeviceViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "انتخاب شهر الزامی می باشد!")]
        [Display(Name = "شهر")]
        public string City { get; set; }
        
        [Required(ErrorMessage = "انتخاب مرکز الزامی می باشد!")]
        [Display(Name = "مرکز")]
        public string Center { get; set; }

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

        [Display(Name = "نوع دستگاه")]
        public Device.DeviceType Type { get; set; }

        [Display(Name = "نوع شبکه")]
        public Device.NetworkType Network { get; set; }

        [Required(ErrorMessage ="مدل دستگاه الزامی می باشد!")]
        [Display(Name = "مدل دستگاه")]
        public string Model { get; set; }

        [Display(Name = "آدرس IP یا NodeID")]
        public string Address { get; set; }

        public override string ToString()
        {
            return Utils.GetDisplayNameOfMember(typeof(Device.DeviceType), Type.ToString()) + " " + Model;
        }
    }
}
