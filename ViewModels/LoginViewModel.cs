using System.ComponentModel.DataAnnotations;

namespace TciDataLinks.ViewModels
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
