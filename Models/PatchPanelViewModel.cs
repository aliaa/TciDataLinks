using EasyMongoNet;
using Omu.ValueInjecter;
using System.ComponentModel.DataAnnotations;

namespace TciDataLinks.Models
{
    public class PatchPanelViewModel : PlaceBasedViewModel
    {
        [Display(Name = "نوع راک")]
        public Rack.RackType RackType { get; set; }

        [Display(Name = "نوع پچ پنل")]
        public PatchPanel.PatchPanelType Type { get; set; }

        [Required(ErrorMessage = "نام اجباریست!")]
        [Display(Name = "نام")]
        public string Name { get; set; }

        public override string ToString() => Name;

        public string GetPlaceDisplay(IReadOnlyDbContext db)
        {
            return Mapper.Map<PatchPanel>(this).GetPlaceDisplay(db);
        }
    }
}
