using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TciDataLinks.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "نام کاربری الزامیست!")]
        [Display(Name = "نام کاربری")]
        public string Username { get; set; }

        [Required(ErrorMessage = "رمز عبور الزامیست!")]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}
