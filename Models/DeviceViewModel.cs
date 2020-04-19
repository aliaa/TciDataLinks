using AliaaCommon;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace TciDataLinks.Models
{
    public class DeviceViewModel : PlaceBasedViewModel
    {
        [Display(Name = "نوع دستگاه")]
        public Device.DeviceType Type { get; set; }

        [Display(Name = "نوع شبکه")]
        public Device.NetworkType Network { get; set; }

        [Required(ErrorMessage ="مدل دستگاه الزامی می باشد!")]
        [Display(Name = "مدل دستگاه")]
        public string Model { get; set; }

        [Remote("DeviceAddressIsValid", "Device", AdditionalFields = "Id", ErrorMessage = "آدرس وارد شده قبلا موجود میباشد!")]
        [Display(Name = "آدرس IP یا NodeID")]
        public string Address { get; set; }

        public override string ToString()
        {
            return Utils.GetDisplayNameOfMember(typeof(Device.DeviceType), Type.ToString()) + " " + Model;
        }
    }
}
