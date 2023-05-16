using UnityEngine;
using UnityEditor;


namespace PQ.Common.Extensions
{
    /*
    Various utilities for drawing and manipulating gizmos.

    Includes resetting of color to what it was previously before the call to minimize side effects.
    */
    public static class GizmoExtensions
    {
        private static readonly Color DefaultColor = Color.white;

        /* Draw text at given world position. */
        public static void DrawText(Vector2 position, string text, Color? color = null)
        {
            // since handles are only available in editor, this becomes a no op when running elsewhere
            #if UNITY_EDITOR
            Color previousColor = Handles.color;
            Handles.color = color.GetValueOrDefault(DefaultColor);

            Handles.Label(position, text);

            Handles.color = previousColor;
            #endif
        }

        /* Draw line between given world positions. */
        public static void DrawLine(Vector2 from, Vector2 to, Color? color = null)
        {
            Color previousColor = Gizmos.color;
            Gizmos.color = color.GetValueOrDefault(DefaultColor);

            Gizmos.DrawLine(from, to);

            Gizmos.color = previousColor;
        }

        /* Draw sides of rectangle corresponding to the area represented by given relative origin and extents (AABB). */
        public static void DrawRect(Vector2 origin, Vector2 extents, Color? color = null)
        {
            Color previousColor = Gizmos.color;
            Gizmos.color = color.GetValueOrDefault(DefaultColor);

            Vector2 min = origin - extents;
            Vector2 max = origin + extents;
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

        /* Draw sides of rectangle corresponding to the area represented by given relative origin and axes. */
        public static void DrawRect(Vector2 origin, Vector2 xAxis, Vector2 yAxis, Color? color = null)
        {
            Color previousColor = Gizmos.color;
            Gizmos.color = color.GetValueOrDefault(DefaultColor);

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
            Gizmos.color = color.GetValueOrDefault(DefaultColor);

            for (int i = 1; i < points.Length; i++)
            {
                Gizmos.DrawLine(points[i-1], points[i]);
            }

            Gizmos.color = previousColor;
        }

        /* Draw a 3d sphere at given world position. */
        public static void DrawSphere(Vector2 origin, float radius, Color? color = null)
        {
            Color previousColor = Gizmos.color;
            Gizmos.color = color.GetValueOrDefault(DefaultColor);

            Gizmos.DrawSphere(origin, radius);

            Gizmos.color = previousColor;
        }

        /*
        Assumes from != to and arrow head length is > 0.
        
        Note that arrow head length and height are configured to be the same length for simplicity,
        and sized relative to the length of the line.
        */
        public static void DrawArrow(Vector2 from, Vector2 to, Color? color = null, float arrowheadSizeRatio = 0.10f)
        {
            Color previousColor = Gizmos.color;
            Gizmos.color = color.GetValueOrDefault(DefaultColor);

            Vector2 vector = to - from;
            Vector2 arrowheadBottom = to - arrowheadSizeRatio * vector;
            Vector2 arrowheadOffset = 0.50f * arrowheadSizeRatio * new Vector2(-vector.y, vector.x);

            Gizmos.DrawLine(from, to);
            Gizmos.DrawLine(arrowheadBottom - arrowheadOffset, to);
            Gizmos.DrawLine(arrowheadBottom + arrowheadOffset, to);

            Gizmos.color = previousColor;
        }
    }
}
