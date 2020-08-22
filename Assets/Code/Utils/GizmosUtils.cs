using UnityEngine;
using UnityEditor;


// note that if using gizmos directly after using any of the below utility functions,
// the color will likely have been changed via Gizmos.color = color
public static class GizmosUtils
{
    private const float DEFAULT_ARROW_HEAD_LENGTH = 20.00f;
    private const float DEFAULT_ARROW_HEAD_ANGLE  = 0.25f;
    private static readonly float DEFAULT_ARROW_HEAD_HALF_ANGLE = Mathf.Tan(DEFAULT_ARROW_HEAD_HALF_ANGLE);
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
    public static void DrawArrow(Vector2 from, Vector2 to, Color? color = null,
        float arrowHeadLength = DEFAULT_ARROW_HEAD_LENGTH, float arrowHeadAngle = DEFAULT_ARROW_HEAD_ANGLE)
    {
        Gizmos.color = color.GetValueOrDefault(DEFAULT_COLOR);
        Vector2 vector = from - to;
        Vector3 right = Quaternion.LookRotation(to) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left  = Quaternion.LookRotation(to) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);

        Gizmos.DrawRay(from, to);
        Gizmos.DrawRay(from + to, right * arrowHeadLength);
        Gizmos.DrawRay(from + to, left * arrowHeadLength);
    }
}
