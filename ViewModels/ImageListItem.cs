using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TciDataLinks.ViewModels
{
    public struct ImageListItem
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public string Icon { get; set; }

        public static readonly ImageListItem[] CustomerIcons = new ImageListItem[] 
        {
            new ImageListItem { Text = "سیاه",          Value = "black",    Icon = "customer-black.png" },
            new ImageListItem { Text = "قرمز",          Value = "red",      Icon = "customer-red.png" },
            new ImageListItem { Text = "نارنجی",        Value = "orange",   Icon = "customer-orange.png" },
            new ImageListItem { Text = "زرد",           Value = "yellow",   Icon = "customer-yellow.png" },
            new ImageListItem { Text = "لیمویی",        Value = "lime",     Icon = "customer-lime.png" },
            new ImageListItem { Text = "سبز",           Value = "green",    Icon = "customer-green.png" },
            new ImageListItem { Text = "فیروزه ای",    Value = "cyan",     Icon = "customer-cyan.png" },
            new ImageListItem { Text = "آبی",           Value = "blue",     Icon = "customer-blue.png" },
            new ImageListItem { Text = "صورتی",         Value = "pink",     Icon = "customer-pink.png" },
        };
    }
}
