using System;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;


namespace PQ.Common.Extensions
{
    /*
    Various utilities for drawing debug visuals in editor.
    */
    public static class DebugExtensions
    {
        private struct OrientedRect
        {
            public readonly Vector2 P0;
            public readonly Vector2 P1;
            public readonly Vector2 P2;
            public readonly Vector2 P3;
            
            public OrientedRect(Vector2 origin, Vector2 extents, float degrees)
            {
                float cosTheta = Mathf.Cos(Mathf.Deg2Rad * degrees);
                float sinTheta = Mathf.Sin(Mathf.Deg2Rad * degrees);
                
                Vector2 xAxis = extents.x * new Vector2( cosTheta, sinTheta);
                Vector2 yAxis = extents.y * new Vector2(-sinTheta, cosTheta);
                P0 = origin - xAxis - yAxis;
                P1 = origin - xAxis + yAxis;
                P2 = origin + xAxis + yAxis;
                P3 = origin + xAxis - yAxis;
            }

            public void Draw(Color color, float duration)
            {
                Debug.DrawLine(P0, P1, color, duration);
                Debug.DrawLine(P1, P2, color, duration);
                Debug.DrawLine(P2, P3, color, duration);
                Debug.DrawLine(P3, P0, color, duration);
            }
        }

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

        /* Draw sides of rectangle corresponding to the area represented by given relative origin and extents (AABB). */
        public static void DrawBox(Vector2 origin, Vector2 extents, float degrees, Color? color=null, float duration=0f)
        {
            Color drawColor = color.GetValueOrDefault(LineColor);
            
            float cosTheta = Mathf.Cos(Mathf.Deg2Rad * degrees);
            float sinTheta = Mathf.Sin(Mathf.Deg2Rad * degrees);
            Vector2 xAxis = extents.x * new Vector2( cosTheta, sinTheta);
            Vector2 yAxis = extents.y * new Vector2(-sinTheta, cosTheta);

            Vector2 p0 = origin - xAxis - yAxis;
            Vector2 p1 = origin - xAxis + yAxis;
            Vector2 p2 = origin + xAxis + yAxis;
            Vector2 p3 = origin + xAxis - yAxis;

            Debug.DrawLine(p0, p1, drawColor, duration);
            Debug.DrawLine(p1, p2, drawColor, duration);
            Debug.DrawLine(p2, p3, drawColor, duration);
            Debug.DrawLine(p3, p0, drawColor, duration);
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


        /* Draw line for given delta, with hit (if any) highlighted in given color. If shape cast, draws line from center to edge. */
        public static void DrawRayCast(Vector2 origin, Vector2 delta, RaycastHit2D hit, float duration=0f)
        {
            Vector2 terminalPoint = origin + delta;
            if (!hit)
            {
                Debug.DrawLine(origin, terminalPoint, CastMissColor, duration);
                return;
            }

            Vector2 centroidPoint = hit.centroid;
            Vector2 edgePoint = hit.point - (hit.distance * delta.normalized);
            Vector2 hitPoint = hit.point;

            // if not approximately same, it was a shape cast, so draw from center of original shape to the edge where the delta starts
            if (centroidPoint != edgePoint)
            {
                Debug.DrawLine(centroidPoint, edgePoint + delta, LineColor, duration);
            }

            Debug.DrawLine(edgePoint, hitPoint,      CastHitColor,   duration);
            Debug.DrawLine(hitPoint,  terminalPoint, CastMissColor, duration);
        }

        /* Draw box cast from given origin along delta. */
        public static void DrawBoxCast(Vector2 origin, Vector2 extents, float degrees, Vector2 delta, ReadOnlySpan<RaycastHit2D> hits, float duration=0f)
        {
            Vector2 terminalPoint = origin + delta;
            OrientedRect original = new(origin, extents, degrees);
            OrientedRect shifted  = new(origin + delta, extents, degrees);

            original.Draw(LineColor, duration);
            shifted .Draw(LineColor, duration);

            Debug.DrawLine(original.P0, shifted.P0, LineColor, duration);
            Debug.DrawLine(original.P1, shifted.P1, LineColor, duration);
            Debug.DrawLine(original.P2, shifted.P2, LineColor, duration);
            Debug.DrawLine(original.P3, shifted.P3, LineColor, duration);

            DrawArrow(origin, terminalPoint, LineColor, duration);
            DrawPlus(origin, ArrowheadSizeRatio * extents, 45f, LineColor, duration);

            for (int i = 0; i < hits.Length; i++)
            {
                Vector2 centroidPoint = hits[i].centroid;
                Vector2 edgePoint = hits[i].point - (hits[i].distance * delta.normalized);
                Vector2 hitPoint = hits[i].point;
                
                Debug.DrawLine(centroidPoint, edgePoint + delta, LineColor, duration);
                Debug.DrawLine(edgePoint, hitPoint, CastHitColor, duration);
                Debug.DrawLine(hitPoint, terminalPoint, CastMissColor, duration);
            }
        }
    }
}
