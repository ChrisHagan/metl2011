using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SandRibbon.Utils
{
    public class MeTLMath
    {
        public const double EPSILON = 0.0000001;
        public static bool ApproxEqual(double a, double b, double tolerance = EPSILON)
        {
            return Math.Abs(a - b) < tolerance;
        }
    }
}
