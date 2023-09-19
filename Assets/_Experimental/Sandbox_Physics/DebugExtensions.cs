using System;
using UnityEngine;


namespace PQ._Experimental.Physics
{
    public static class DebugExtensions
    {
        private struct OrientedRect
        {
            public readonly Vector2 Center;
            public readonly Vector2 P0;
            public readonly Vector2 P1;
            public readonly Vector2 P2;
            public readonly Vector2 P3;
            
            public OrientedRect(Vector2 center, Vector2 extents, float degrees)
            {
                float cosTheta = Mathf.Cos(Mathf.Deg2Rad * degrees);
                float sinTheta = Mathf.Sin(Mathf.Deg2Rad * degrees);
                
                Vector2 xAxis = extents.x * new Vector2( cosTheta, sinTheta);
                Vector2 yAxis = extents.y * new Vector2(-sinTheta, cosTheta);

                Center = center;
                P0 = center - xAxis - yAxis;
                P1 = center - xAxis + yAxis;
                P2 = center + xAxis + yAxis;
                P3 = center + xAxis - yAxis;
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
                Debug.DrawLine(origin, origin + hit.distance * direction, CastHitColor, duration);
            }
        }

        
        /* Visualize the 'path' formed by dragging a box from start along given delta. */
        public static void DrawBoxCast(Vector2 origin, Vector2 extents, float degrees, Vector2 direction, float distance, ReadOnlySpan<RaycastHit2D> hits, float duration=0f)
        {
            OrientedRect original = new(origin, extents, degrees);
            OrientedRect shifted  = new(origin + distance * direction, extents, degrees);

            // render the box center at the start and end of the casts, with lines connecting the corners
            original.Draw(LineColor, duration);
            shifted .Draw(LineColor, duration);
            Debug.DrawLine(original.P0, shifted.P0, LineColor, duration);
            Debug.DrawLine(original.P1, shifted.P1, LineColor, duration);
            Debug.DrawLine(original.P2, shifted.P2, LineColor, duration);
            Debug.DrawLine(original.P3, shifted.P3, LineColor, duration);

            // render an 'x' at the cast origin with an arrow extending to the cast terminal
            Vector2 plusSignExtents = ArrowheadSizeRatio * Mathf.LerpUnclamped(extents.x, extents.y, 0.50f) * Vector2.one;
            DrawPlus(origin, plusSignExtents, 45f, LineColor, duration);
            DrawArrow(original.Center, shifted.Center, LineColor, duration);

            // render any 'hits' by coloring a line segment showing the hit point and distance
            for (int i = 0; i < hits.Length; i++)
            {
                Debug.DrawLine(hits[i].point, hits[i].point + (hits[i].distance * -direction), CastHitColor, duration);
            }
        }
    }
}
