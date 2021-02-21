using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TciDataLinks.ViewModels
{
    public class EditUserViewModel : BaseUserViewModel
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [MinLength(6, ErrorMessage = "رمز عبور بایستی حداقل 6 کاراکتر باشد")]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; }

        [Display(Name = "غیر فعال شده")]
        public bool Disabled { get; set; }
    }
}
