using MongoDB.Bson;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TciDataLinks.Models;

namespace TciDataLinks.ViewModels
{
    public class EditUserViewModel : BaseUserViewModel
    {
        public ObjectId Id { get; set; }

        [MinLength(6, ErrorMessage = "رمز عبور بایستی حداقل 6 کاراکتر باشد")]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; }

        [Display(Name = "غیر فعال شده")]
        public bool Disabled { get; set; }
    }
}
