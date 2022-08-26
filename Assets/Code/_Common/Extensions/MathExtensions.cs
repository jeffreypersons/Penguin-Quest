using System.Collections.Generic;
using UnityEngine;


namespace PQ.Common.Extensions
{
    /*
    Convenience methods for math related functionality such as conversions, x-y plane vector computations, etc
    
    Notes
    - Rotational functionality is for the x-y plane _unless_ otherwise stated, so that we can use lightweight trig rather
      than using the less intuitive (especially for 2d), and heavier/more-generalized 3d quaternion methods in unity
    */
    public static class MathExtensions
    {
        public static Vector2 PerpendicularCounterClockwise(Vector2 vector)
        {
            return new Vector2(-vector.y, vector.x);
        }

        public static bool AreScalarsEqual(float a, float b)
        {
            return Mathf.Approximately(a, b);
        }

        public static bool AreComponentsEqual(Vector2 a, Vector2 b)
        {
            return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y);
        }

        public static float PercentToRatio(float percent)
        {
            return Mathf.Approximately(percent, 0.00f) ? 0.00f : percent / 100.00f;
        }

        public static bool IsWithinTolerance(float a, float b, float tolerance)
        {
            return Mathf.Abs(b - a) <= tolerance;
        }

        public static bool IsWithinTolerance(Vector2 a, Vector2 b, float tolerance)
        {
            return Mathf.Abs(b.x - a.x) <= tolerance && Mathf.Abs(b.y - a.y) <= tolerance;
        }

        public static bool IsWithinRange(float val, float min, float max)
        {
            return (min < max) && (min <= val && val <= max);
        }
    }
}
