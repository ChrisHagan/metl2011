﻿using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace SandRibbon.Providers
{
    class ColourInformationProvider
    {
        private static Dictionary<string, string> privateColourTable;
        private static Dictionary<string, string> ColourTable {
            get {
                if (privateColourTable != null) return privateColourTable;
                privateColourTable = new Dictionary<string, string> { 
                {"F0F8FF","Alice Blue"},{"FAEBD7","Antique White"},{"7FFFD4","Aquamarine"},
                {"F0FFFF","Azure"},{"F5F5DC","Beige"},{"FFE4C4","Bisque"},{"000000","Black"},
                {"FFEBCD","Blanched Almond"},{"0000FF","Blue"},{"8A2BE2","Blue Violet"},{"A52A2A","Brown"},
                {"DEB887","Burly Wood"},{"5F9EA0","Cadet Blue"},{"7FFF00","Chartreuse"},{"D2691E","Chocolate"},
                {"FF7F50","Coral"},{"6495ED","Cornflower Blue"},{"FFF8DC","Cornsilk"},{"DC143C","Crimson"},
                {"00FFFF","Cyan"},{"00008B","Dark Blue"},{"008B8B","Dark Cyan"},{"B8860B","Dark Goldenrod"},
                {"A9A9A9","Dark Gray"},{"006400","Dark Green"},{"BDB76B","Dark Khaki"},{"8B008B","Dark Magenta"},
                {"556B2F","Dark Olive Green"},{"FF8C00","Dark Orange"},{"9932CC","Dark Orchid"},{"8B0000","Dark Red"},
                {"E9967A","Dark Salmon"},{"8FBC8F","Dark Sea Green"},{"483D8B","Dark Slate Blue"},
                {"2F4F4F","Dark Slate Gray"},{"00CED1","Dark Turquoise"},{"9400D3","Dark Violet"},{"FF1493","Deep Pink"},
                {"00BFFF","Deep Sky Blue"},{"696969","Dim Gray"},{"1E90FF","Dodger Blue"},{"B22222","Firebrick"},
                {"FFFAF0","Floral White"},{"228B22","Forest Green"},{"DCDCDC","Gainsboro"},
                {"F8F8FF","Ghost White"},{"FFD700","Gold"},{"DAA520","Goldenrod"},{"808080","Gray"},{"008000","Green"},
                {"ADFF2F","Green Yellow"},{"F0FFF0","Honeydew"},{"FF69B4","Hot Pink"},{"CD5C5C","Indian Red"},
                {"4B0082","Indigo"},{"FFFFF0","Ivory"},{"F0E68C","Khaki"},{"E6E6FA","Lavender"},
                {"FFF0F5","Lavender Blush"},{"7CFC00","Lawn Green"},{"FFFACD","Lemon Chiffon"},{"ADD8E6","Light Blue"},
                {"F08080","Light Coral"},{"E0FFFF","Light Cyan"},{"FAFAD2","Light Goldenrod Yellow"},
                {"D3D3D3","Light Gray"},{"90EE90","Light Green"},{"FFB6C1","Light Pink"},{"FFA07A","Light Salmon"},
                {"20B2AA","Light SeaGreen"},{"87CEFA","Light Sky Blue"},{"778899","Light Slate Gray"},
                {"B0C4DE","Light SteelBlue"},{"FFFFE0","Light Yellow"},{"00FF00","Lime"},{"32CD32","Lime Green"},
                {"FAF0E6","Linen"},{"FF00FF","Magenta"},{"800000","Maroon"},{"66CDAA","Medium Aquamarine"},
                {"0000CD","Medium Blue"},{"BA55D3","Medium Orchid"},{"9370DB","Medium Purple"},
                {"3CB371","Medium Sea Green"},{"7B68EE","Medium Slate Blue"},{"00FA9A","Medium Spring Green"},
                {"48D1CC","Medium Turquoise"},{"C71585","Medium Violet Red"},{"191970","Midnight Blue"},
                {"F5FFFA","Mint Cream"},{"FFE4E1","Misty Rose"},{"FFE4B5","Moccasin"},{"FFDEAD","Navajo White"},
                {"000080","Navy"},{"FDF5E6","OldLace"},{"808000","Olive"},{"6B8E23","Olive Drab"},{"FFA500","Orange"},
                {"FF4500","Orange Red"},{"DA70D6","Orchid"},{"EEE8AA","Pale Goldenrod"},{"98FB98","Pale Green"},
                {"AFEEEE","Pale Turquoise"},{"DB7093","Pale Violet Red"},{"FFEFD5","Papaya Whip"},{"FFDAB9","Peach Puff"},
                {"CD853F","Peru"},{"FFC0CB","Pink"},{"DDA0DD","Plum"},{"B0E0E6","Powder Blue"},{"800080","Purple"},
                {"FF0000","Red"},{"BC8F8F","Rosy Brown"},{"4169E1","Royal Blue"},{"8B4513","Saddle Brown"},
                {"FA8072","Salmon"},{"F4A460","Sandy Brown"},{"2E8B57","Sea Green"},{"FFF5EE","Sea Shell"},
                {"A0522D","Sienna"},{"C0C0C0","Silver"},{"87CEEB","Sky Blue"},{"6A5ACD","Slate Blue"},
                {"708090","Slate Gray"},{"FFFAFA","Snow"},{"00FF7F","Spring Green"},{"4682B4","Steel Blue"},
                {"D2B48C","Tan"},{"008080","Teal"},{"D8BFD8","Thistle"},{"FF6347","Tomato"},
                {"40E0D0","Turquoise"},{"EE82EE","Violet"},{"F5DEB3","Wheat"},{"FFFFFF","White"},
                {"F5F5F5","White Smoke"},{"FFFF00","Yellow"},{"9ACD32","Yellow Green"}
                };
                return privateColourTable;
            }
        }
        private static byte variation = 30;
        private static bool isRoughlyGreater(float compared, float comparedTo)
        {
            return (compared > comparedTo + variation);
        }
        public static string ConvertToName(Color originalColor)
        {
            if (ColourTable.ContainsKey(originalColor.ToString().Substring(3)))
                return ColourTable[originalColor.ToString().Substring(3)].ToLower();
            var outputString = "";
            int A = Convert.ToInt32(originalColor.A);
            int R = Convert.ToInt32(originalColor.R);
            int G = Convert.ToInt32(originalColor.G);
            int B = Convert.ToInt32(originalColor.B);
            string primaryColour;
            if (R == 0 && G == 0 && B == 0)
                return "black";
            if (R == 255 && G == 255 && B == 255)
                return "white";
            if (R + G + B > 511)
                outputString += "light ";
            else if (R + G + B < 255)
                outputString += "dark ";
            if (R > G && R > B)
                primaryColour = "Red";
            else if (G > B && G > R)
                primaryColour = "Green";
            else if (B > G && B > R)
                primaryColour = "Blue";
            else if (R == B && R > G)
                primaryColour = "Purple";
            else if (G == R && G > B)
                primaryColour = "Yellow";
            else if (B == G && B > R)
                primaryColour = "Cyan";
            else primaryColour = "Grey";
            switch (primaryColour)
            {
                case "Red":
                    if (isRoughlyGreater(G, B))
                        outputString += "greenish ";
                    else if (isRoughlyGreater(B, G))
                        outputString += "blueish ";
                    outputString += "red";
                    break;
                case "Green":
                    if (isRoughlyGreater(R, B))
                        outputString += "reddish ";
                    else if (isRoughlyGreater(B, R))
                        outputString += "blueish ";
                    outputString += "green";
                    break;
                case "Blue":
                    if (isRoughlyGreater(G, R))
                        outputString += "greenish ";
                    else if (isRoughlyGreater(R, G))
                        outputString += "reddish ";
                    outputString += "blue";
                    break;
                case "Purple":
                    if (R - G < 100)
                        outputString += "greenish ";
                    outputString += "purple";
                    break;
                case "Yellow":
                    if (B - R < 100)
                        outputString += "orangish ";
                    outputString += "yellow";
                    break;
                case "Cyan":
                    if (G - R < 100)
                        outputString += "reddish ";
                    outputString += "cyan";
                    break;

                case "Grey":
                    outputString += "grey";
                    break;
            }
            return outputString;
        }
        public static string ConvertToName(String hexValue)
        {
            return "";
        }

    }
    public class ColourTools
    {
        public void HsvToRgb(double h, double S, double V, out int r, out int g, out int b)
        {
            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {
                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;
                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;
                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;
                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;
                    default:
                        R = G = B = V;
                        break;
                }
            }
            r = Clamp((int)(R * 255.0));
            g = Clamp((int)(G * 255.0));
            b = Clamp((int)(B * 255.0));
        }
        int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }
        public void RgbToHsv(int R, int G, int B, out double h, out double s, out double v)
        {
            double max = R;
            var dominantColour = "Red";
            if (G > R)
            {
                max = G;
                dominantColour = "Green";
            }
            if (B > R && B > G)
            {
                max = B;
                dominantColour = "Blue";
            }
            if (max == 0)
            {
                h = 0;
                s = 0;
                v = 1;
                return;
            }
            double min = R;
            if (G < R)
                min = G;
            if (B < R && B < G)
                min = B;
            if (min == 255)
            {
                h = 0;
                s = 0;
                v = 0;
                return;
            }
            double delta = max - min;
            double Hue;
            if (delta == 0)
                Hue = 0;
            else Hue = 0 + (((G - B) / delta) * 60);
            if (dominantColour == "Green")
                if (delta == 0)
                    Hue = 120;
                else Hue = 120 + (((B - R) / delta) * 60);
            if (dominantColour == "Blue")
                if (delta == 0)
                    Hue = 240;
                else Hue = 240 + (((R - G) / delta) * 60);
            while (Hue > 360)
                Hue = Hue - 360;
            while (Hue < 0)
                Hue = Hue + 360;
            h = Hue;
            if (((double)(delta) / max) < 0)
                s = 0;
            else if ((double)(delta / max) > 1)
                s = 1;
            else
                s = (double)(delta / max);
            if ((double)(max / 255) < 0)
                v = 0;
            else if ((double)(max / 255) > 1)
                v = 1;
            else
                v = (double)(max / 255);
        }
    }
}
