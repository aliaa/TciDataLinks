using System.ComponentModel.DataAnnotations;

namespace TciDataLinks.Models
{
    public enum TransmissionSystemType
    {
        [Display(Name = "مشخص نشده")]
        None,
        RTN,
        PTN,
        DWDM,
        [Display(Name = "رادیو WiFi")]
        Radio_WiFi,
        [Display(Name = "فیبر نوری")]
        FiberOptics,
        S200,
        S385,
        IBAS,
        Line_MUX,
        [Display(Name = "رادیو Combo")]
        Radio_Combo,
        [Display(Name = "رادیو Pasolink")]
        Radio_Pasolink,
        SDH,
        PDH,
        S320,
        WRI,
        NG,
    }
}
