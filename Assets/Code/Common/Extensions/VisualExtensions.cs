using System;
using System.Diagnostics.Contracts;
using UnityEditor;
using UnityEngine;


namespace PQ.Common.Extensions
{
    /*
    Various utilities for drawing and manipulating gizmos or debug shapes in editor.

    Includes resetting of color to what it was previously before the call to minimize side effects.
    */
    public sealed class VisualExtensions
    {
        // todo: extend this to use line renderer so it can be used for game-view/builds (eg via a debug menu)
        public enum DrawMode
        {
            Debug,
            Gizmos,
        }
        // only applies for debug
        public enum DurationMode
        {
            DeltaTime,
        }

        public DrawMode Mode         { get; set; }
        public float    Duration     { get; set; }
        public Color    DefaultColor { get; set; }

        public VisualExtensions(DrawMode mode)
        {
            Mode         = mode;
            Duration     = 0f;
            DefaultColor = Color.white;
        }

        
        public void DrawText(Vector2 position, string text, Color? color = null)
        {
            // since handles are only available in editor, this becomes a no op when running elsewhere
            #if UNITY_EDITOR
            Color previousColor = Handles.color;
            Handles.color = color.GetValueOrDefault(DefaultColor);

            Handles.Label(position, text);

            Handles.color = previousColor;
            #endif
        }
        
        public void DrawLine(Vector2 pointA, Vector2 pointB, Color? color = null)
        {
            Span<Vector2> endPoints = stackalloc Vector2[] { pointA, pointB };

            DrawLinesBetweenPoints(endPoints, Mode, Duration, color.GetValueOrDefault(DefaultColor), connectEnds: false);
        }
        
        /*
        Assumes from != to and arrow head length is > 0.
        
        Note that arrow head length and height are configured to be the same length for simplicity,
        and sized relative to the length of the line.
        */
        public void DrawArrow(Vector2 from, Vector2 to, Color? color = null, float arrowheadSizeRatio = 0.10f)
        {
            Vector2 vector = to - from;
            Vector2 arrowheadBottom = to - arrowheadSizeRatio * vector;
            Vector2 arrowheadOffset = 0.50f * arrowheadSizeRatio * new Vector2(-vector.y, vector.x);
            
            DrawLine(from, to, color);
            DrawLine(arrowheadBottom - arrowheadOffset, to, color);
            DrawLine(arrowheadBottom + arrowheadOffset, to, color);
        }

        public void DrawCircle(Vector2 center, float radius, Color? color=null)
        {
            DrawEllipse(center, new Vector2(radius, radius), 0f, 16, color);
        }

        public void DrawEllipse(Vector2 center, Vector2 extents, float degrees = 0f, int segments=16, Color? color=null)
        {
            // assumes >= 5 segments
            Span<Vector2> points = stackalloc Vector2[segments+1];

            float deltaRadians = (Mathf.Deg2Rad * 360f) / segments;
            for (int i = 0; i < segments; i++)
            {
                float epsiloidalX = extents.x * Mathf.Sin(i * deltaRadians);
                float epsiloidalY = extents.y * Mathf.Cos(i * deltaRadians);

                points[i] = AsWorldPoint(center, epsiloidalX, epsiloidalY, degrees);
            }

            DrawLinesBetweenPoints(points, Mode, Duration, color.GetValueOrDefault(DefaultColor), connectEnds: true);
        }

        public void DrawBox(Vector2 center, Vector2 extents, float degrees=0f, Color? color = null)
        {
            Span<Vector2> corners = stackalloc Vector2[]
            {
                AsWorldPoint(center, -extents.x, -extents.y, degrees),
                AsWorldPoint(center, -extents.x,  extents.y, degrees),
                AsWorldPoint(center,  extents.x,  extents.y, degrees),
                AsWorldPoint(center,  extents.x, -extents.y, degrees),
            };
            DrawLinesBetweenPoints(corners, Mode, Duration, color.GetValueOrDefault(DefaultColor), connectEnds: true);
        }


        private static void DrawLinesBetweenPoints(ReadOnlySpan<Vector2> points, DrawMode mode, float duration, Color color, bool connectEnds)
        {
            switch (mode)
            {
                case DrawMode.Debug:
                    for (int i = 1; i < points.Length; i++)
                    {
                        Debug.DrawLine(points[i - 1], points[i], color, duration);
                    }
                    if (connectEnds)
                    {
                        Debug.DrawLine(points[points.Length-1], points[0], color, duration);
                    }
                    break;

                case DrawMode.Gizmos:
                    Color previousColor = Gizmos.color;
                    Gizmos.color = color;
                    for (int i = 1; i < points.Length; i++)
                    {
                        Gizmos.DrawLine(points[i - 1], points[i]);
                    }
                    if (connectEnds)
                    {
                        Debug.DrawLine(points[points.Length - 1], points[0], color, duration);
                    }
                    Gizmos.color = previousColor;
                    break;

                default:
                    break;
            }
        }
        
        [Pure]
        private static Vector2 AsWorldPoint(Vector2 origin, float xOffset, float yOffset, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cosTheta = Mathf.Cos(radians);
            float sinTheta = Mathf.Sin(radians);
            return new Vector2(
                x: origin.x + (xOffset * cosTheta) + (yOffset * sinTheta),
                y: origin.y - (xOffset * sinTheta) + (yOffset * cosTheta));
        }
    }
}
