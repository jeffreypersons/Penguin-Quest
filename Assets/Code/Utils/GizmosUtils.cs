using UnityEngine;
using UnityEditor;


// note that if using gizmos directly after using any of the below utility functions,
// the color will likely have been changed via Gizmos.color = color
public static class GizmosUtils
{
    private const float DEFAULT_ARROWHEAD_LENGTH_RATIO = 0.10f;
    private static readonly Color DEFAULT_COLOR = Color.white;

    public static void DrawText(Vector2 position, string text, Color? color = null)
    {
        #if UNITY_EDITOR
        Handles.color = color.GetValueOrDefault(DEFAULT_COLOR);
        Handles.Label(position, text);
        #endif
    }
    public static void DrawLine(Vector2 from, Vector2 to, Color? color = null)
    {
        Gizmos.color = color.GetValueOrDefault(DEFAULT_COLOR);
        Gizmos.DrawLine(from, to);
    }
    // assumes arrow head length is nonzero and from to are nonequal
    // note: arrow head length and height are set to be equal
    public static void DrawArrow(Vector2 from, Vector2 to, Color? color = null,
        float arrowHeadLengthRatio = DEFAULT_ARROWHEAD_LENGTH_RATIO)
    {
        Vector2 vector = to - from;
        Vector2 dir  = vector.normalized;
        float length = vector.magnitude;
        float arrowLength = arrowHeadLengthRatio * length;
        Vector2 arrowHeadBottom = from + ((length - arrowLength) * dir);
        Vector2 arrowHeadBaseExtents = MathUtils.PerpendicularCounterClockwise(dir) * arrowLength * 0.50f;

        Gizmos.color = color.GetValueOrDefault(DEFAULT_COLOR);
        Gizmos.DrawLine(from, to);
        Gizmos.DrawLine(arrowHeadBottom + arrowHeadBaseExtents, to);
        Gizmos.DrawLine(arrowHeadBottom - arrowHeadBaseExtents, to);
    }
}
