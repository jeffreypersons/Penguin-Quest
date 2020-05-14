using System.Globalization;
using UnityEngine;


// note: methods, where it makes sense to, logerrors (ie in conversion methods, or other methods
// that wouldn't be used on a frame by frame basis.
// otherwise the methods make appropriate assumptions about input, stated in their corresponding comments
public static class MathUtils
{
    // see: https://answers.unity.com/questions/10093/rigidbody-rotating-around-a-point-instead-on-self.html
    public static void RotateRigidBodyAroundPointBy(Rigidbody2D rb, Vector3 origin, Vector3 axis, float angle)
    {
        Quaternion q = Quaternion.AngleAxis(angle, axis);
        rb.MovePosition(q * (rb.transform.position - origin) + origin);
        rb.MoveRotation(rb.transform.rotation * q);
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
