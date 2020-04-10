using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TciDataLinks.Models
{
    public class ChangePasswordViewModel
    {
        [Display(Name ="رمز فعلی")]
        public string CurrentPassword { get; set; }

        [Display(Name = "رمز جدید")]
        [MinLength(5, ErrorMessage = "رمز عبور بایستی حداقل 5 کاراکتر باشد.")]
        public string NewPassword { get; set; }

        [Display(Name = "تکرار رمز جدید")]
        [Compare(nameof(NewPassword), ErrorMessage = "تکرار رمز با رمز جدید برابر نیست!")]
        public string RepeatNewPassword { get; set; }
    }
}
