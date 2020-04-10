
using System.ComponentModel.DataAnnotations;

namespace TciDataLinks.Models
{
    public enum Permission
    {
        [Display(Name = "مدیریت کاربران")]
        ManageUsers,
        [Display(Name = "ویرایش داده ها")]
        EditData
    }
}
