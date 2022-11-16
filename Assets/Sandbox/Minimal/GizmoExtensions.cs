using UnityEngine;
using UnityEditor;


namespace PQ.TestScenes.Minimal.Physics
{
    /*
    Various utilities for drawing and manipulating gizmos.

    Includes resetting of color to what it was previously before the call to minimize side effects.
    */
    public static class GizmoExtensions
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
        public static void DrawLine((Vector2, Vector2) endpoints, Color? color = null)
        {
            Color previousColor = Gizmos.color;
            Gizmos.color = color.GetValueOrDefault(DEFAULT_COLOR);

            Gizmos.DrawLine(endpoints.Item1, endpoints.Item2);

            Gizmos.color = previousColor;
        }

        /* Draw sides of rectangle corresponding to the area represented by given relative origin and axes. */
        public static void DrawRect(Vector2 origin, Vector2 xAxis, Vector2 yAxis, Color? color = null)
        {
            Color previousColor = Gizmos.color;
            Gizmos.color = color.GetValueOrDefault(DEFAULT_COLOR);

            Vector2 min = origin - xAxis - yAxis;
            Vector2 max = origin + xAxis + yAxis;
            Vector2 leftBottom  = new(min.x, min.y);
            Vector2 leftTop     = new(min.x, max.y);
            Vector2 rightBottom = new(max.x, min.y);
            Vector2 rightTop    = new(max.x, max.y);

            Gizmos.DrawLine(leftTop,     rightTop);
            Gizmos.DrawLine(leftBottom,  rightBottom);
            Gizmos.DrawLine(leftBottom,  leftTop);
            Gizmos.DrawLine(rightBottom, rightTop);

            Gizmos.color = previousColor;
        }
        
        /* Draw lines between given points. */
        public static void DrawWaypoints(Vector2[] points, Color? color = null)
        {
            if (points == null || points.Length < 2)
            {
                return;
            }

            Color previousColor = Gizmos.color;
            Gizmos.color = color.GetValueOrDefault(DEFAULT_COLOR);

            for (int i = 1; i < points.Length; i++)
            {
                Gizmos.DrawLine(points[i-1], points[i]);
            }

            Gizmos.color = previousColor;
        }

        /* Draw a 3d sphere at given world position.*/
        public static void DrawSphere(Vector2 origin, float radius, Color? color = null)
        {
            Color previousColor = Gizmos.color;
            Gizmos.color = color.GetValueOrDefault(DEFAULT_COLOR);

            Gizmos.DrawSphere(origin, radius);

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
            Gizmos.color = color.GetValueOrDefault(DEFAULT_COLOR);

            Vector2 vector    = to - from;
            Vector2 dir       = vector.normalized;
            float length      = vector.magnitude;
            float arrowLength = arrowHeadLengthRatio * length;
            Vector2 arrowHeadBottom      = from + ((length - arrowLength) * dir);
            Vector2 arrowHeadBaseExtents = new Vector2(-dir.y, dir.x) * arrowLength * 0.50f;

            Gizmos.DrawLine(from, to);
            Gizmos.DrawLine(arrowHeadBottom + arrowHeadBaseExtents, to);
            Gizmos.DrawLine(arrowHeadBottom - arrowHeadBaseExtents, to);

            Gizmos.color = previousColor;
        }
    }
}
