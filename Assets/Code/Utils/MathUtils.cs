using System.Globalization;
using UnityEngine;


// note: methods, where it makes sense to, logerrors (ie in conversion methods, or other methods
// that wouldn't be used on a frame by frame basis.
// otherwise the methods make appropriate assumptions about input, stated in their corresponding comments
public static class MathUtils
{
    // computed the unsigned degrees (between [0, 90]) between given vec and x axis
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
    // computed the unsigned degrees (between [0, 90]) between given vec and y axis
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

    // perform a quick check for normalization without having to do the expensive magnitude calculation
    public static bool IsNormalized(Vector2 vector)
    {
        return Mathf.Approximately(vector.sqrMagnitude, 1.00f);
    }

    public static Vector2 PerpendicularClockwise(Vector2 vector)
    {
        return new Vector2(vector.y, -vector.x);
    }
    public static Vector2 PerpendicularCounterClockwise(Vector2 vector)
    {
        return new Vector2(-vector.y, vector.x);
    }

    // rotate given vector by degrees counter-clockwise (or clockwise if negative)
    public static Vector2 RotateBy(Vector2 vector, float degrees)
    {
        return Quaternion.AngleAxis(degrees, vector) * vector;
    }

    // return true if floating point approximations of vectors a and b are the same
    public static bool AreComponentsEqual(Vector2 a, Vector2 b)
    {
        return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y);
    }
    // return true if floating point approximations of magnitudes a and b are the same
    public static bool AreMagnitudesEqual(Vector2 a, Vector2 b)
    {
        return Mathf.Approximately(a.magnitude, b.magnitude);
    }
    // return true if both vectors are pointed in the same direction
    //
    // Note
    // * keep in mind that normalized vectors return vector2.zero if components are too small
    //  `https://docs.unity3d.com/ScriptReference/Vector2-normalized.html`,
    //  so comparing the directions of two tiny vectors with nonequal components may actually return true
    public static bool AreDirectionsEqual(Vector2 a, Vector2 b)
    {
        return AreComponentsEqual(a, b) || AreComponentsEqual(a.normalized, b.normalized);
    }

    public static Vector2 SwapCoords(Vector2 vector)
    {
        return new Vector2(vector.y, vector.x);
    }
    // return unit vector parallel to the line[from, to]
    public static Vector2 ComputeDirection(Vector2 from, Vector2 to)
    {
        return (from - to).normalized;
    }
    // return unit vector perpendicular to the line[from, to]
    public static Vector2 ComputeDirectionPerpendicular(Vector2 from, Vector2 to)
    {
        Vector2 vector = (from - to);
        return new Vector2(vector.y, -vector.x).normalized;
    }

    public static void RotateRigidBodyAroundPointBy(Rigidbody2D rigidBody, Vector3 axis, float angle)
    {
        Quaternion q = Quaternion.AngleAxis(angle, axis);
        rigidBody.MovePosition(q * rigidBody.transform.position);
        rigidBody.MoveRotation(rigidBody.transform.rotation * q);
    }
    // see: https://answers.unity.com/questions/10093/rigidbody-rotating-around-a-point-instead-on-self.html
    public static void RotateRigidBodyAroundPointBy(Rigidbody2D rigidBody, Vector3 origin, Vector3 axis, float angle)
    {
        Quaternion q = Quaternion.AngleAxis(angle, axis);
        rigidBody.MovePosition(q * (rigidBody.transform.position - origin) + origin);
        rigidBody.MoveRotation(rigidBody.transform.rotation * q);
    }
    // return 2d world space coords corresponding to given ratio from bottom left corner of bounding box
    // note: does not currently support rotated bounds, and assumes offset is between 0 and 1
    public static Vector3 GetPointInsideBounds(Bounds bounds, Vector2 ratioOffsetFromMin)
    {
        return new Vector2(bounds.min.x + (bounds.size.x * ratioOffsetFromMin.x),
                           bounds.min.y + (bounds.size.y * ratioOffsetFromMin.y));
    }

    // assuming given value is between 0, 100, convert to a ratio between 0.00 and 1.00
    public static float PercentToRatio(float percent)
    {
        return Mathf.Approximately(percent, 0.00f)? 0.00f : percent / 100.00f;
    }
    // assuming given value is between 0, 100, convert to a ratio between 0.00 and 1.00
    public static float RatioToPercent(float ratio)
    {
        return ratio * 100.00f;
    }

    public static float RandomSign()
    {
        return Random.Range(0, 2) == 0? 1.0f : -1.0f;
    }

    public static bool IsWithinRange(float val, float min, float max)
    {
        return (min < max) && (min <= val && val <= max);
    }
    // assuming startA <= endA and startB <= endB, two ranges overlap in the case that
    // there exists a value C such that: startA <= C <= endA and startB <= C <= endB
    // see: https://stackoverflow.com/questions/3269434/whats-the-most-efficient-way-to-test-two-integer-ranges-for-overlap
    public static bool IsOverlappingRange(float startA, float endA, float startB, float endB)
    {
        return startA <= endB && startB <= endA;
    }

    public static bool IsInteger(float value)
    {
        return Mathf.Approximately(value - Mathf.Round(value), 0);
    }
    public static bool IsAllInteger(params string[] values)
    {
        foreach (string value in values)
        {
            if (!int.TryParse(value, out _))
            {
                return false;
            }
        }
        return true;
    }
    public static bool IsAllFloat(params string[] values)
    {
        foreach (string value in values)
        {
            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            {
                return false;
            }
        }
        return true;
    }
}
