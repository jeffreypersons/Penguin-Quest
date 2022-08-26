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
        /* Compute the unsigned degrees (between [0, 90]) between given vec and x axis. */
        public static float AngleFromXAxis(Vector2 vector)
        {
            if (Mathf.Approximately(vector.y, 0.00f))
            {
                return 0.00f;
            }
            if (Mathf.Approximately(vector.x, 0.00f))
            {
                return 90.00f;
            }
            return vector.x < 0.00f ? Vector2.Angle(Vector2.left, vector) : Vector2.Angle(Vector2.right, vector);
        }

        /* Compute the unsigned degrees (between [0, 90]) between given vec and y axis. */
        public static float AngleFromYAxis(Vector2 vector)
        {
            if (Mathf.Approximately(vector.x, 0.00f))
            {
                return 0.00f;
            }
            if (Mathf.Approximately(vector.y, 0.00f))
            {
                return 90.00f;
            }
            return vector.y < 0.00f ? Vector2.Angle(Vector2.down, vector) : Vector2.Angle(Vector2.up, vector);
        }

        public static Vector2 PerpendicularClockwise(Vector2 vector)
        {
            return new Vector2(vector.y, -vector.x);
        }

        public static Vector2 PerpendicularCounterClockwise(Vector2 vector)
        {
            return new Vector2(-vector.y, vector.x);
        }
        
        public static Vector2 RotateVector(Vector2 vector, float degrees)
        {
            float radians  = degrees * Mathf.Deg2Rad;
            float cosTheta = Mathf.Cos(radians);
            float sinTheta = Mathf.Sin(radians);
            return new Vector2(
                x: (cosTheta * vector.x) - (sinTheta * vector.y),
                y: (sinTheta * vector.x) + (cosTheta * vector.y));
        }
        
        /*
        Compute point rotated degrees clockwise about given local origin.

        Determines point relative to origin, rotates, and translates back to get our newly rotated position.
        */
        public static Vector2 RotatePointAroundPivot(Vector2 point, Vector2 pivot, float degrees)
        {
            Vector2 pointLocalToPivot = point - pivot;
            float radians = degrees * Mathf.Deg2Rad;
            float cosTheta = Mathf.Cos(radians);
            float sinTheta = Mathf.Sin(radians);
            return new Vector2(
                x: pivot.x + (pointLocalToPivot.x * cosTheta) + (pointLocalToPivot.y * sinTheta),
                y: pivot.y - (pointLocalToPivot.x * sinTheta) + (pointLocalToPivot.y * cosTheta));
        }

        public static bool AreScalarsEqual(float a, float b)
        {
            return Mathf.Approximately(a, b);
        }

        /* Perform a quick check for normalization without having to do the expensive magnitude calculation. */
        public static bool IsNormalized(Vector2 vector)
        {
            return Mathf.Approximately(vector.sqrMagnitude, 1.00f);
        }

        /* Return true if floating point approximations of vectors a and b are the same. */
        public static bool AreComponentsEqual(Vector2 a, Vector2 b)
        {
            return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y);
        }

        /* Return true if floating point approximations of magnitudes a and b are the same. */
        public static bool AreMagnitudesEqual(Vector2 a, Vector2 b)
        {
            return (IsNormalized(a) && IsNormalized(b)) || Mathf.Approximately(a.magnitude, b.magnitude);
        }

        /*
        Return true if both vectors are pointed in the same direction.
    
        Keep in mind that hypothetically, this may give false positives for sufficiently tiny vectors,
        as the check is done by comparing the normalized vector components, of which Unity returns vector2.zero
        if components are too small. But then again, for tiny vectors many Unity math functionality will break,
        so in practice this is more of a non issue.
        */
        public static bool AreDirectionsEqual(Vector2 a, Vector2 b)
        {
            if (AreComponentsEqual(a, b))
            {
                return true;
            }

            Vector2 directionA = IsNormalized(a)? a : a.normalized;
            Vector2 directionB = IsNormalized(a)? b : b.normalized;
            return AreComponentsEqual(directionA, directionB);
        }

        public static bool AreDirectionsEqual_Fast(Vector2 a, Vector2 b)
        {
            return Vector2.Dot(a, b) == 1f;
        }

        /* Assuming given value is between 0, 100, convert to a ratio between 0.00 and 1.00. */
        public static float PercentToRatio(float percent)
        {
            return Mathf.Approximately(percent, 0.00f)? 0.00f : percent / 100.00f;
        }

        /* Assuming given value is between 0.00 and 1.00, convert to a ratio between 0 and 100. */
        public static float RatioToPercent(float ratio)
        {
            return ratio * 100.00f;
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


        /*
        Given segments A and B are said to overlap if there exists a value in both ranges.

        To test if A overlaps B we simply check if A starts before the end of B, and A ends after the start of B.
        To derive the above, note that overlap occurs if A is neither completely after or before B, thus:
        ---> A and B Overlap
        ---> not(startA >  endB) and not(endA <  startB)
        --->    (startA <= endB) and    (endA >= startB)
        */
        public static bool IsOverlappingRange(float startA, float endA, float startB, float endB)
        {
            return startA <= endB && startB <= endA;
        }
        

        /*
        Fills given result list with points interpolated between given start and end positions.

        Given endpoints and number of points n, we divide the line into n + 1 segments and compute
        the point between each segment. Note that this includes endpoints.
        
        For example, given 3 points (0, 0) and (10, 10), we cut the line into 4 pieces
        as percents along the line: [0.00, 0.25], [0.25, 0.50], [0.50, 0.75], [0.75, 1.00],
        which gives us the 3 points in between of (2.5, 2.5), (5, 5), (7.5, 7.5),
        plus their endpoints.
        */
        public static List<Vector2> InterpolatePoints(Vector2 from, Vector2 to, int numPointsInBetween)
        {
            int   numPoints   = numPointsInBetween + 2;
            int   numSegments = numPointsInBetween + 1;
            float stepSize    = 1f / numSegments;

            float currentStep = 0f;
            List<Vector2> result = new List<Vector2>(numPoints);
            for (int i = 0; i < numPoints; i++)
            {
                result.Add(Vector2.Lerp(from, to, currentStep));
                currentStep += stepSize;
            }
            return result;
        }

        /*
        How many times does delta fit into length?
        */
        public static int ComputeDivisions(float length, float delta)
        {
            float clampedDelta = Mathf.Clamp01(delta);
            return Mathf.Approximately(clampedDelta, 0f)?
                0 :
                Mathf.RoundToInt(length / clampedDelta);
        }

        public static Vector2 MidPoint(Vector2 from, Vector2 to)
        {
            return Vector2.Lerp(from, to, 0.5f);
        }
    }
}
