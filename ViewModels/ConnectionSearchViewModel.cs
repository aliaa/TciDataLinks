using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TciDataLinks.Models;

namespace TciDataLinks.ViewModels
{
    public class ConnectionSearchViewModel
    {
        public enum EndPointSearchType
        {
            [Display(Name = "همه اتصال ها")]
            All,
            [Display(Name = "فقط اتصال اول")]
            First,
            [Display(Name = "اتصالهای بعدی")]
            NotFirst,
            //[Display(Name = "فقط اتصال آخر")]
            //Last,
        }

        public enum DeviceNetworkType
        {
            [Display(Name = "همه")]
            All,
            [Display(Name = "بین شبکه ای")]
            InterNetwork,
            Cisco = Models.Device.NetworkType.Cisco+2,
            Huawei = Models.Device.NetworkType.Huawei+2,
            ZTE = Models.Device.NetworkType.ZTE+2,
            Tellabs = Models.Device.NetworkType.Tellabs+2,
            WiFi = Models.Device.NetworkType.WiFi+2,
        }

        [Required(ErrorMessage = "انتخاب شهر اجباری است")]
        [Display(Name = "شهر")]
        public string City { get; set; }

        //[Required(ErrorMessage = "انتخاب مرکز اجباری است")]
        [Display(Name = "مرکز")]
        public string Center { get; set; }

        [Display(Name = "ساختمان")]
        public string Building { get; set; }

        [Display(Name = "اتاق")]
        public string Room { get; set; }

        [Display(Name = "راک")]
        public string Rack { get; set; }

        [Display(Name = "دستگاه")]
        public string Device { get; set; }

        [Display(Name = "نوع پورت")]
        public EndPoint.PortTypeEnum? PortType { get; set; }

        [Display(Name = "نوع ماژول")]
        public EndPoint.ModuleType? Module { get; set; }

        [Display(Name = "نوع پچ کورد")]
        public EndPoint.PatchCordType? PatchCord { get; set; }

        [Display(Name = "نوع کانکتور")]
        public EndPoint.ConnectorType? Connector { get; set; }

        [Display(Name = "نحوه جستجوی اتصالها")]
        public EndPointSearchType SearchType { get; set; }

        [Display(Name = "پروتکشن دیتا")]
        public bool? DataProtection { get; set; }

        [Display(Name = "پروتکشن سیستمهای انتقال")]
        public bool? TransmissionProtection { get; set; }

        [Display(Name = "اطلاعات اتصال ناقص است؟")]
        public bool? Incomplete { get; set; }

        [Display(Name = "نوع شبکه")]
        public DeviceNetworkType NetworkType { get; set; }

        public long TotalLinksCount { get; set; }

        public List<ConnectionViewModel> SearchResult { get; set; } = null;
    }
}
