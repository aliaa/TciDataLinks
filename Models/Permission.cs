
using System.ComponentModel.DataAnnotations;

namespace TciDataLinks.Models
{
    public enum Permission
    {
        [Display(Name = "ویرایش مکانها و دستگاهها")]
        EditPlacesAndDevices,
        [Display(Name = "ویرایش لینک ها")]
        EditConnections,
        [Display(Name = "مدیریت کاربران")]
        ManageUsers,
        [Display(Name = "مشاهده لاگ کاربران")]
        ViewUserLogs,
        [Display(Name = "تغییر چینش گراف")]
        ChangeGraphOrders,
    }
}
