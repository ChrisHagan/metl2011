﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace SandRibbon
{
    class ColorLookup
    {
        public static List<System.Windows.Media.Color> s_mediaColors = new List<System.Windows.Media.Color>();
        public static List<System.Windows.Media.Color> GetMediaColors()
        {
            if (s_mediaColors.Count == 0)
            {
                s_mediaColors.Add(Colors.Brown);
                s_mediaColors.Add(Colors.Orange);
                s_mediaColors.Add(Colors.Pink);
                s_mediaColors.Add(Colors.Purple);
                s_mediaColors.Add(Colors.Black);
                //s_mediaColors.Add(Colors.White);
                s_mediaColors.Add(Colors.Blue);
                s_mediaColors.Add(Colors.Red);
                s_mediaColors.Add(Colors.Yellow);
                s_mediaColors.Add(Colors.Green);
                s_mediaColors.Add(Colors.AliceBlue);
                s_mediaColors.Add(Colors.AntiqueWhite);
                s_mediaColors.Add(Colors.Aqua);
                s_mediaColors.Add(Colors.Aquamarine);
                s_mediaColors.Add(Colors.Azure);
                s_mediaColors.Add(Colors.Beige);
                s_mediaColors.Add(Colors.Bisque);
                s_mediaColors.Add(Colors.BlanchedAlmond);
                s_mediaColors.Add(Colors.BlueViolet);
                s_mediaColors.Add(Colors.BurlyWood);
                s_mediaColors.Add(Colors.CadetBlue);
                s_mediaColors.Add(Colors.Chartreuse);
                s_mediaColors.Add(Colors.Chocolate);
                s_mediaColors.Add(Colors.Coral);
                s_mediaColors.Add(Colors.CornflowerBlue);
                s_mediaColors.Add(Colors.Cornsilk);
                s_mediaColors.Add(Colors.Crimson);
                s_mediaColors.Add(Colors.Cyan);
                s_mediaColors.Add(Colors.DarkBlue);
                s_mediaColors.Add(Colors.DarkCyan);
                s_mediaColors.Add(Colors.DarkGoldenrod);
                s_mediaColors.Add(Colors.DarkGray);
                s_mediaColors.Add(Colors.DarkGreen);
                s_mediaColors.Add(Colors.DarkKhaki);
                s_mediaColors.Add(Colors.DarkMagenta);
                s_mediaColors.Add(Colors.DarkOliveGreen);
                s_mediaColors.Add(Colors.DarkOrange);
                s_mediaColors.Add(Colors.DarkOrchid);
                s_mediaColors.Add(Colors.DarkRed);
                s_mediaColors.Add(Colors.DarkSalmon);
                s_mediaColors.Add(Colors.DarkSeaGreen);
                s_mediaColors.Add(Colors.DarkSlateBlue);
                s_mediaColors.Add(Colors.DarkSlateGray);
                s_mediaColors.Add(Colors.DarkTurquoise);
                s_mediaColors.Add(Colors.DarkViolet);
                s_mediaColors.Add(Colors.DeepPink);
                s_mediaColors.Add(Colors.DeepSkyBlue);
                s_mediaColors.Add(Colors.DimGray);
                s_mediaColors.Add(Colors.DodgerBlue);
                s_mediaColors.Add(Colors.Firebrick);
                s_mediaColors.Add(Colors.FloralWhite);
                s_mediaColors.Add(Colors.ForestGreen);
                s_mediaColors.Add(Colors.Fuchsia);
                s_mediaColors.Add(Colors.Gainsboro);
                s_mediaColors.Add(Colors.GhostWhite);
                s_mediaColors.Add(Colors.Gold);
                s_mediaColors.Add(Colors.Goldenrod);
                s_mediaColors.Add(Colors.Gray);
                s_mediaColors.Add(Colors.GreenYellow);
                s_mediaColors.Add(Colors.Honeydew);
                s_mediaColors.Add(Colors.HotPink);
                s_mediaColors.Add(Colors.IndianRed);
                s_mediaColors.Add(Colors.Indigo);
                s_mediaColors.Add(Colors.Ivory);
                s_mediaColors.Add(Colors.Khaki);
                s_mediaColors.Add(Colors.Lavender);
                s_mediaColors.Add(Colors.LavenderBlush);
                s_mediaColors.Add(Colors.LawnGreen);
                s_mediaColors.Add(Colors.LemonChiffon);
                s_mediaColors.Add(Colors.LightBlue);
                s_mediaColors.Add(Colors.LightCoral);
                s_mediaColors.Add(Colors.LightCyan);
                s_mediaColors.Add(Colors.LightGoldenrodYellow);
                s_mediaColors.Add(Colors.LightGray);
                s_mediaColors.Add(Colors.LightGreen);
                s_mediaColors.Add(Colors.LightPink);
                s_mediaColors.Add(Colors.LightSalmon);
                s_mediaColors.Add(Colors.LightSeaGreen);
                s_mediaColors.Add(Colors.LightSkyBlue);
                s_mediaColors.Add(Colors.LightSlateGray);
                s_mediaColors.Add(Colors.LightSteelBlue);
                s_mediaColors.Add(Colors.LightYellow);
                s_mediaColors.Add(Colors.Lime);
                s_mediaColors.Add(Colors.LimeGreen);
                s_mediaColors.Add(Colors.Linen);
                s_mediaColors.Add(Colors.Magenta);
                s_mediaColors.Add(Colors.Maroon);
                s_mediaColors.Add(Colors.MediumAquamarine);
                s_mediaColors.Add(Colors.MediumBlue);
                s_mediaColors.Add(Colors.MediumOrchid);
                s_mediaColors.Add(Colors.MediumPurple);
                s_mediaColors.Add(Colors.MediumSeaGreen);
                s_mediaColors.Add(Colors.MediumSlateBlue);
                s_mediaColors.Add(Colors.MediumSpringGreen);
                s_mediaColors.Add(Colors.MediumTurquoise);
                s_mediaColors.Add(Colors.MediumVioletRed);
                s_mediaColors.Add(Colors.MidnightBlue);
                s_mediaColors.Add(Colors.MintCream);
                s_mediaColors.Add(Colors.MistyRose);
                s_mediaColors.Add(Colors.Moccasin);
                s_mediaColors.Add(Colors.NavajoWhite);
                s_mediaColors.Add(Colors.Navy);
                s_mediaColors.Add(Colors.OldLace);
                s_mediaColors.Add(Colors.Olive);
                s_mediaColors.Add(Colors.OliveDrab);
                s_mediaColors.Add(Colors.OrangeRed);
                s_mediaColors.Add(Colors.Orchid);
                s_mediaColors.Add(Colors.PaleGoldenrod);
                s_mediaColors.Add(Colors.PaleGreen);
                s_mediaColors.Add(Colors.PaleTurquoise);
                s_mediaColors.Add(Colors.PaleVioletRed);
                s_mediaColors.Add(Colors.PapayaWhip);
                s_mediaColors.Add(Colors.PeachPuff);
                s_mediaColors.Add(Colors.Peru);
                s_mediaColors.Add(Colors.Plum);
                s_mediaColors.Add(Colors.PowderBlue);
                s_mediaColors.Add(Colors.RosyBrown);
                s_mediaColors.Add(Colors.RoyalBlue);
                s_mediaColors.Add(Colors.SaddleBrown);
                s_mediaColors.Add(Colors.Salmon);
                s_mediaColors.Add(Colors.SandyBrown);
                s_mediaColors.Add(Colors.SeaGreen);
                s_mediaColors.Add(Colors.SeaShell);
                s_mediaColors.Add(Colors.Sienna);
                s_mediaColors.Add(Colors.Silver);
                s_mediaColors.Add(Colors.SkyBlue);
                s_mediaColors.Add(Colors.SlateBlue);
                s_mediaColors.Add(Colors.SlateGray);
                s_mediaColors.Add(Colors.Snow);
                s_mediaColors.Add(Colors.SpringGreen);
                s_mediaColors.Add(Colors.SteelBlue);
                s_mediaColors.Add(Colors.Tan);
                s_mediaColors.Add(Colors.Teal);
                s_mediaColors.Add(Colors.Thistle);
                s_mediaColors.Add(Colors.Tomato);
                s_mediaColors.Add(Colors.Transparent);
                s_mediaColors.Add(Colors.Turquoise);
                s_mediaColors.Add(Colors.Violet);
                s_mediaColors.Add(Colors.Wheat);
                s_mediaColors.Add(Colors.WhiteSmoke);
                s_mediaColors.Add(Colors.YellowGreen);
            }
            return s_mediaColors;
        }
        public static Color ColorOf(String colorName)
        {
            var property = typeof(Colors).GetProperties().Where(p => p.Name.ToUpper().Equals(colorName.ToUpper())).Single();
            return (Color)property.GetValue(typeof(Colors), null);
        }
        public static string HumanReadableNameOf(Color color)
        {
            color = new Color{ A = 255, R = color.R, G = color.G, B = color.B};
            foreach (var p in typeof(Colors).GetProperties())
            {
                if (p.GetGetMethod().Invoke(null, null).Equals(color))
                {
                    return p.Name;
                }
            }
            return color.ToString();
        }
    }
}
