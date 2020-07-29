using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TciDataLinks.ViewModels
{
    public class ImageListItem
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public string FileName { get; set; }

        public static readonly ImageListItem[] CustomerIcons = new ImageListItem[] 
        {
            new ImageListItem { Text = "سیاه",          Value = "black",    FileName = "customer-black.png" },
            new ImageListItem { Text = "قرمز",          Value = "red",      FileName = "customer-red.png" },
            new ImageListItem { Text = "نارنجی",        Value = "orange",   FileName = "customer-orange.png" },
            new ImageListItem { Text = "زرد",           Value = "yellow",   FileName = "customer-yellow.png" },
            new ImageListItem { Text = "لیمویی",        Value = "lime",     FileName = "customer-lime.png" },
            new ImageListItem { Text = "سبز",           Value = "green",    FileName = "customer-green.png" },
            new ImageListItem { Text = "فیروزه ای",    Value = "cyan",     FileName = "customer-cyan.png" },
            new ImageListItem { Text = "آبی",           Value = "blue",     FileName = "customer-blue.png" },
            new ImageListItem { Text = "صورتی",         Value = "pink",     FileName = "customer-pink.png" },
        };
    }
}
