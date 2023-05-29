using System;
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
        public static void DrawRect(Vector2 origin, Vector2 extents, Color? color=null, float duration=0f)
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
        public static void DrawWaypoints(Vector2[] points, Color? color=null, float duration=0f, bool connectEnds=false)
        {
            if (points == null || points.Length <= 1)
            {
                return;
            }

            Color drawColor = color.GetValueOrDefault(DefaultColor);
            for (int i = 1; i < points.Length; i++)
            {
                Debug.DrawLine(points[i - 1], points[i], drawColor, duration);
            }
            if (connectEnds)
            {
                Debug.DrawLine(points[0], points[points.Length-1], drawColor, duration);
            }
        }


        /* Draw line for given delta, with hit (if any) highlighted in given color. If shape cast, draws line from center to edge. */
        public static void DrawCastHit(Vector2 delta, RaycastHit2D hit, Color? lineColor=null, Color? hitColor=null, float duration=0f)
        {
            Vector2 centroidPoint = hit.centroid;
            Vector2 edgePoint     = hit.point - (hit.distance * delta.normalized);
            Vector2 hitPoint      = hit.point;
            Vector2 terminalPoint = edgePoint + delta;

            // if not approximately same, it was a shape cast, so draw from center of original shape to the edge where the delta starts
            if (centroidPoint != edgePoint)
            {
                Debug.DrawLine(centroidPoint, edgePoint, DefaultColor, duration);
            }

            if (hit)
            {
                Debug.DrawLine(edgePoint, hitPoint,      lineColor.GetValueOrDefault(Color.green), duration);
                Debug.DrawLine(hitPoint,  terminalPoint, hitColor.GetValueOrDefault(Color.red),    duration);
            }
            else
            {
                Debug.DrawLine(edgePoint, terminalPoint, hitColor.GetValueOrDefault(Color.red), duration);
            }
        }

        /* Draw line from origin to offset from origin. */
        public static void DrawRayCast(Vector2 origin, Vector2 delta, Color? color=null, float duration=0f)
        {
            Color drawColor = color.GetValueOrDefault(DefaultColor);
            Debug.DrawLine(origin, origin + delta, drawColor, duration);
        }

        /* Draw box cast from given origin along delta. */
        public static void DrawBoxCast(Vector2 origin, Vector2 extents, float degrees, Vector2 delta,
            Color? color=null, float duration=0f)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cosTheta = Mathf.Cos(radians);
            float sinTheta = Mathf.Sin(radians);
            Vector2 forward = extents.x * new Vector2( cosTheta, sinTheta);
            Vector2 up      = extents.y * new Vector2(-sinTheta, cosTheta);

            Span<Vector2> original = stackalloc Vector2[]
            {
                origin + forward + up,
                origin + forward - up,
                origin - forward - up,
                origin - forward + up,
            };
            Span<Vector2> shifted = stackalloc Vector2[original.Length];
            for (int i = 0; i < original.Length; i++)
            {
                shifted[i] = original[i] + delta;
            }

            Color drawColor = color.GetValueOrDefault(DefaultColor);
            for (int i = 1; i < original.Length; i++)
            {
                Debug.DrawLine(original[i-1], original[i], drawColor, duration);
                Debug.DrawLine(shifted [i-1], shifted [i], drawColor, duration);
                Debug.DrawLine(original[i],   shifted [i], drawColor, duration);
            }
        }
    }
}
