
using System.ComponentModel.DataAnnotations;

namespace TciDataLinks.Models
{
    public enum Permission
    {
        [Display(Name = "ویرایش داده ها")]
        EditData,
        [Display(Name = "مدیریت کاربران")]
        ManageUsers,
    }
}
