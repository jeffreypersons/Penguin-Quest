using UnityEngine;
using UnityEditor;


namespace PenguinQuest.Utils
{
    /*
    Various utilities for drawing and manipulating gizmos.

    Includes resetting of color to what it was previously before the call to minimize side effects.
    */
    public static class GizmosUtils
    {
        private const float DEFAULT_ARROWHEAD_LENGTH_RATIO = 0.10f;
        private static readonly Color DEFAULT_COLOR = Color.white;

        /* Draw text at given world position. */
        public static void DrawText(Vector2 position, string text, Color? color = null)
        {
            // since handles are only available in editor, this becomes a no op when running elsewhere
            #if UNITY_EDITOR
            Color previousColor = Handles.color;

            Handles.color = color.GetValueOrDefault(DEFAULT_COLOR);
            Handles.Label(position, text);

            Handles.color = previousColor;
            #endif
        }

        /* Draw line between given world positions. */
        public static void DrawLine(Vector2 from, Vector2 to, Color? color = null, float lineWidth=0.30f)
        {
            Color previousColor = Gizmos.color;

            Gizmos.color = color.GetValueOrDefault(DEFAULT_COLOR);
            Gizmos.DrawLine(from, to);

            Gizmos.color = previousColor;
        }

        /*
        Assumes arrow head length is nonzero and from,to are nonequal.

        Note that arrow head length and height are configured to be the same length for simplicity.
        */
        public static void DrawArrow(Vector2 from, Vector2 to, Color? color = null,
            float arrowHeadLengthRatio = DEFAULT_ARROWHEAD_LENGTH_RATIO)
        {
            Color previousColor = Gizmos.color;

            Vector2 vector    = to - from;
            Vector2 dir       = vector.normalized;
            float length      = vector.magnitude;
            float arrowLength = arrowHeadLengthRatio * length;
            Vector2 arrowHeadBottom      = from + ((length - arrowLength) * dir);
            Vector2 arrowHeadBaseExtents = MathUtils.PerpendicularCounterClockwise(dir) * arrowLength * 0.50f;

            Gizmos.color = color.GetValueOrDefault(DEFAULT_COLOR);
            Gizmos.DrawLine(from, to);
            Gizmos.DrawLine(arrowHeadBottom + arrowHeadBaseExtents, to);
            Gizmos.DrawLine(arrowHeadBottom - arrowHeadBaseExtents, to);

            Gizmos.color = previousColor;
        }
    }
}
