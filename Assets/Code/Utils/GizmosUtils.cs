using UnityEngine;
using UnityEditor;


// note that if using gizmos directly after using any of the below utility functions,
// the color will likely have been changed via Gizmos.color = color
public static class GizmosUtils
{
    private const float DEFAULT_ARROW_HEAD_LENGTH = 20.00f;
    private const float DEFAULT_ARROW_HEAD_ANGLE  = 0.25f;
    private static readonly Color DEFAULT_COLOR = Color.white;

    public static void DrawText(Vector2 position, string text, Color? color = null)
    {
        Handles.color = color.GetValueOrDefault(DEFAULT_COLOR);
        Handles.Label(position, text);
    }
    public static void DrawLine(Vector2 from, Vector2 to, Color? color = null)
    {
        Gizmos.color = color.GetValueOrDefault(DEFAULT_COLOR);
        Gizmos.DrawLine(from, to);
    }
    // assumes arrow head length is nonzero and from to are nonequal
    // note: arrow head length and height are set to be equal
    public static void DrawArrow(Vector2 from, Vector2 to, Color? color = null, float arrowHeadLength = DEFAULT_ARROW_HEAD_LENGTH)
    {
        Vector2 vector = from - to;
        Vector2 dir = vector.normalized;
        Vector2 arrowHeadBottom = (vector.magnitude - arrowHeadLength) * dir;
        Vector2 arrowHeadBaseExtents = MathUtils.PerpendicularClockwise(dir) * arrowHeadLength * 0.50f;

        Gizmos.color = color.GetValueOrDefault(DEFAULT_COLOR);
        Gizmos.DrawLine(from, to);
        Gizmos.DrawLine(arrowHeadBottom + arrowHeadBaseExtents, to);
        Gizmos.DrawLine(arrowHeadBottom - arrowHeadBaseExtents, to);
    }
}
