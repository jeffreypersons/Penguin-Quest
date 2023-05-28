using UnityEngine;


namespace PQ.Common.Extensions
{
    /*
    Various utilities for drawing debug visuals in editor.
    */
    public static class DebugExtensions
    {
        private static readonly Color DefaultColor = Color.white;

        public static float ArrowheadSizeRatio { get; set; } = 0.10f;


        /* Draw line between given world positions. */
        public static void DrawLine(Vector2 from, Vector2 to, Color? color=null, float duration=0f)
        {
            Color drawColor = color.GetValueOrDefault(DefaultColor);

            Debug.DrawLine(from, to, drawColor, duration);
        }

        /*
        Assumes from != to and arrow head length is > 0.
        
        Note that arrow head length and height are configured to be the same length for simplicity,
        and sized relative to the length of the line.
        */
        public static void DrawArrow(Vector2 from, Vector2 to, Color? color=null, float duration=0f)
        {
            Color drawColor = color.GetValueOrDefault(DefaultColor);

            Vector2 vector = to - from;
            Vector2 arrowheadBottom = to - ArrowheadSizeRatio * vector;
            Vector2 arrowheadOffset = 0.50f * ArrowheadSizeRatio * new Vector2(-vector.y, vector.x);

            Debug.DrawLine(from, to, drawColor, duration);
            Debug.DrawLine(arrowheadBottom - arrowheadOffset, to, drawColor, duration);
            Debug.DrawLine(arrowheadBottom + arrowheadOffset, to, drawColor, duration);
        }

        /* Draw sides of rectangle corresponding to the area represented by given relative origin and extents (AABB). */
        public static void DrawRect(Vector2 origin, Vector2 extents, Color? color = null, float duration=0f)
        {
            Color drawColor = color.GetValueOrDefault(DefaultColor);

            Vector2 min = origin - extents;
            Vector2 max = origin + extents;
            Vector2 leftBottom  = new(min.x, min.y);
            Vector2 leftTop     = new(min.x, max.y);
            Vector2 rightBottom = new(max.x, min.y);
            Vector2 rightTop    = new(max.x, max.y);

            Debug.DrawLine(leftTop,     rightTop,    drawColor, duration);
            Debug.DrawLine(leftBottom,  rightBottom, drawColor, duration);
            Debug.DrawLine(leftBottom,  leftTop,     drawColor, duration);
            Debug.DrawLine(rightBottom, rightTop,    drawColor, duration);
        }
        
        /* Draw lines between given points. */
        public static void DrawWaypoints(Vector2[] points, Color? color=null, float duration=0f)
        {
            if (points == null || points.Length < 2)
            {
                return;
            }
            
            Color drawColor = color.GetValueOrDefault(DefaultColor);
            for (int i = 1; i < points.Length; i++)
            {
                Debug.DrawLine(points[i-1], points[i], drawColor, duration);
            }
        }
    }
}