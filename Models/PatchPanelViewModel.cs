using System.ComponentModel.DataAnnotations;

namespace TciDataLinks.Models
{
    public class PatchPanelViewModel : PlaceBasedViewModel
    {
        [Display(Name = "نوع راک")]
        public Rack.RackType RackType { get; set; }

        [Display(Name = "نوع پچ پنل")]
        public PatchPanel.PatchPanelType Type { get; set; }
        [Display(Name = "نام")]
        public string Name { get; set; }

        public override string ToString() => Name;
    }
}
