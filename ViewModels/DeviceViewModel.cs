using AliaaCommon;
using EasyMongoNet;
using Microsoft.AspNetCore.Mvc;
using Omu.ValueInjecter;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TciDataLinks.Controllers;
using TciDataLinks.Models;

namespace TciDataLinks.ViewModels
{
    public class DeviceViewModel : PlaceBasedViewModel
    {
        [Display(Name = "نوع دستگاه")]
        public Device.DeviceType Type { get; set; }

        [Display(Name = "نوع شبکه")]
        public Device.NetworkType Network { get; set; }

        [Required(ErrorMessage = "مدل دستگاه الزامی می باشد!")]
        [Display(Name = "مدل دستگاه")]
        public string Model { get; set; }

        [Remote(nameof(DeviceController.DeviceAddressIsValid), "Device", AdditionalFields = nameof(Id), ErrorMessage = "آدرس وارد شده قبلا موجود میباشد!")]
        [Display(Name = "آدرس IP یا NodeID")]
        public string Address { get; set; }

        //public List<> Ports { get; set; }
        public List<UserActivityViewModel> Logs { get; set; }

        public override string ToString()
        {
            return Utils.DisplayName(typeof(Device.DeviceType), Type.ToString()) + " " + Model;
        }

        public string GetPlaceDisplay(IReadOnlyDbContext db)
        {
            return Mapper.Map<Device>(this).GetPlaceDisplay(db);
        }
    }
}
