using System.ComponentModel.DataAnnotations;

namespace TciDataLinks.ViewModels
{
    public class AddUserViewModel : BaseUserViewModel
    {
        [MinLength(6, ErrorMessage = "رمز عبور بایستی حداقل 6 کاراکتر باشد!")]
        [Required]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "رمز عبور و تکرار آن بایستی برابر باشند!")]
        [Required]
        [Display(Name = "تکرار رمز عبور")]
        public string PasswordRepeat { get; set; }
    }
}
