using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TciDataLinks
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement(Attributes = "asp-checkboxlist, asp-modelname")]
    public class CheckBoxListTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-checkboxlist")]
        public IEnumerable<SelectListItem> Items { get; set; }

        [HtmlAttributeName("asp-modelname")]
        public string ModelName { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Content.AppendHtml($@"<ul style=""list-style-type: none;"">");
            var i = 0;
            foreach (var item in Items)
            {
                var selected = item.Selected ? @"checked=""checked""" : "";
                var disabled = item.Disabled ? @"disabled=""disabled""" : "";
                var html = $@"<li><label><input type=""checkbox"" {selected} {disabled} id=""{ModelName}_{i}__Selected"" name=""{ModelName}[{i}].Selected"" value=""true"" /> {item.Text}</label>";
                html += $@"<input type=""hidden"" id=""{ModelName}_{i}__Value"" name=""{ModelName}[{i}].Value"" value=""{item.Value}"">";
                html += $@"<input type=""hidden"" id=""{ModelName}_{i}__Text"" name=""{ModelName}[{i}].Text"" value=""{item.Text}""></li>";

                output.Content.AppendHtml(html);

                i++;
            }
            output.Content.AppendHtml("</ul>");
            output.Attributes.SetAttribute("class", "checkboxlist");
        }
    }
}
