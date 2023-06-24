using System;
using UnityEngine;


namespace PQ._Experimental.Physics.LinearStep_002
{
    public static class DebugExtensions
    {
        public static Color LineColor          { get; set; } = Color.white;
        public static Color CastMissColor      { get; set; } = Color.red;
        public static Color CastHitColor       { get; set; } = Color.green;
        public static float ArrowheadSizeRatio { get; set; } = 0.10f;


        /* Draw line between given world positions. */
        public static void DrawLine(Vector2 from, Vector2 to, Color? color=null, float duration=0f)
        {
            Debug.DrawLine(from, to, color.GetValueOrDefault(LineColor), duration);
        }

        /*
        Assumes from != to and arrow head length is > 0.
        
        Note that arrow head length and height are configured to be the same length for simplicity,
        and sized relative to the length of the line.
        */
        public static void DrawArrow(Vector2 from, Vector2 to, Color? color=null, float duration=0f)
        {
            Color drawColor = color.GetValueOrDefault(LineColor);

            Vector2 vector = to - from;
            Vector2 arrowheadBottom = to - ArrowheadSizeRatio * vector;
            Vector2 arrowheadOffset = 0.50f * ArrowheadSizeRatio * new Vector2(-vector.y, vector.x);

            Debug.DrawLine(from, to, drawColor, duration);
            Debug.DrawLine(arrowheadBottom - arrowheadOffset, to, drawColor, duration);
            Debug.DrawLine(arrowheadBottom + arrowheadOffset, to, drawColor, duration);
        }

        /* Draw plus + as two perpendicular lines at given point/rotation. */
        public static void DrawPlus(Vector2 origin, Vector2 extents, float degrees, Color? color=null, float duration=0f)
        {
            float cosTheta = Mathf.Cos(Mathf.Deg2Rad * degrees);
            float sinTheta = Mathf.Sin(Mathf.Deg2Rad * degrees);
            Vector2 xAxis = extents.x * new Vector2( cosTheta, sinTheta);
            Vector2 yAxis = extents.y * new Vector2(-sinTheta, cosTheta);

            Color drawColor = color.GetValueOrDefault(LineColor);
            Debug.DrawLine(origin - xAxis, origin + xAxis, drawColor, duration);
            Debug.DrawLine(origin - yAxis, origin + yAxis, drawColor, duration);
        }


        /* Draw lines between given points. */
        public static void DrawWaypoints(ReadOnlySpan<Vector2> points, Color? color=null, float duration=0f, bool connectEnds=false)
        {
            if (points == null || points.Length <= 1)
            {
                return;
            }

            Color drawColor = color.GetValueOrDefault(LineColor);
            for (int i = 1; i < points.Length; i++)
            {
                Debug.DrawLine(points[i - 1], points[i], drawColor, duration);
            }
            if (connectEnds)
            {
                Debug.DrawLine(points[0], points[points.Length-1], drawColor, duration);
            }
        }

        /* Visualize the 'path' formed by dragging a point from start along given delta. */
        public static void DrawRayCast(Vector2 origin, Vector2 direction, float distance, RaycastHit2D hit, float duration=0f)
        {
            Debug.DrawLine(origin, origin + distance * direction, CastMissColor, duration);
            if (hit)
            {
                Debug.DrawLine(origin, hit.point, CastHitColor, duration);
            }
        }
    }
}
