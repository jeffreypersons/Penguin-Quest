using UnityEngine;


/*
Convenience methods for math related functionality such as conversions, x-y plane vector computations, etc

Notes
- Rotational functionality is for the x-y plane _unless_ otherwise stated, so that we can use lightweight trig rather
  than using the less intuitive (especially for 2d), and heavier/more-generalized 3d quaternion methods in unity
*/
public static class MathUtils
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


    /*
    Compute point rotated degrees clockwise about given local origin.

    Determines point relative to origin, rotates, and translates back to get our newly rotated position.
    */
    public static Vector2 RotateClockwise(Vector2 point, float degrees, Vector2? origin = null)
    {
        Vector2 pivot = origin.GetValueOrDefault(Vector2.zero);
        Vector2 pointLocalToPivot = point - pivot;
        float radians = degrees * Mathf.Deg2Rad;
        float cosTheta = Mathf.Cos(radians);
        float sinTheta = Mathf.Sin(radians);
        return new Vector2( (pointLocalToPivot.x * cosTheta) + (pointLocalToPivot.y * sinTheta) + pivot.x,
                           -(pointLocalToPivot.x * sinTheta) + (pointLocalToPivot.y * cosTheta) + pivot.y);
    }

    /*
    Compute point rotated degrees counter-clockwise about given local origin.
     
    Determines point relative to origin, rotates, and translates back to get our newly rotated position).
    */
    public static Vector2 RotateCounterClockwise(Vector2 point, float degrees, Vector2? origin = null)
    {
        Vector2 pivot = origin.GetValueOrDefault(Vector2.zero);
        Vector2 pointLocalToPivot = point - pivot;
        float radians  = degrees * Mathf.Deg2Rad;
        float cosTheta = Mathf.Cos(radians);
        float sinTheta = Mathf.Sin(radians);
        return new Vector2((pointLocalToPivot.x * cosTheta) - (pointLocalToPivot.y * sinTheta) + pivot.x,
                           (pointLocalToPivot.x * sinTheta) + (pointLocalToPivot.y * cosTheta) + pivot.y);
    }
    public static Vector2 RotateClockwise3D(Vector3 vector, float degrees)
    {
        return Quaternion.AngleAxis(-degrees, vector) * vector;
    }
    public static Vector2 RotateCounterClockwise3D(Vector3 vector, float degrees)
    {
        return Quaternion.AngleAxis(degrees, vector) * vector;
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
}
