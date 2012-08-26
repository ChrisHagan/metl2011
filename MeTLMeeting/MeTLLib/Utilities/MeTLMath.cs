using System;

namespace MeTLLib.Utilities
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
