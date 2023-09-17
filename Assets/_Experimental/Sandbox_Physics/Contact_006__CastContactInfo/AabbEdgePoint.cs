using System;
using System.Diagnostics.Contracts;
using UnityEngine;


namespace PQ._Experimental.Physics.Contact_006
{
    public struct AabbEdgePoint
    {
        public readonly float   Angle;
        public readonly Vector2 Point;
        public readonly Vector2 Direction;
        public readonly float   Distance;

        public AabbEdgePoint(Vector2 center, Vector2 extents, float angle)
        {
            #if UNITY_EDITOR
            if (angle is < 0 or > 360)
            {
                throw new ArgumentException($"Angle must be between 0 and 360, received index={angle}");
            }
            #endif

            Vector2 unitSquarePoint = FindPointOnEdgeOfUnitSquare(Mathf.Deg2Rad * angle);
            Vector2 scaleOfExtents = FindScaleRelativeToUnitSquare(extents);

            Vector2 offset = Vector2.Scale(unitSquarePoint, scaleOfExtents);
            Angle     = angle;
            Point     = center + offset;
            Direction = unitSquarePoint;
            Distance  = offset.magnitude;
        }


        /*
        Find distance and direction from center to edge of a 1x1 rectangle.
        
        Note that radians are counter-clockwise from x-axis and point is relative to zero origin.
        */
        [Pure]
        private static Vector2 FindPointOnEdgeOfUnitSquare(float radians)
        {
            // scale point on edge of unit circle by distance to edge of square
            Vector2 radialVector = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            float distanceToEdge = 1f / (2f * Mathf.Max(Mathf.Abs(radialVector.x), Mathf.Abs(radialVector.y)));
            return distanceToEdge * radialVector;
        }

        /*
        Find ratio of dimensions between extents and a 1x1 rectangle.

        For example, for xy extents (1/4,1) the scale is (0.50,2).
        */
        [Pure]
        private static Vector2 FindScaleRelativeToUnitSquare(Vector2 extents)
        {
            // note scale relative to unit square is computed by (x,y) / (0.5,0.5)
            // which simplifies to (1/(1/2)) * (x,y) and thus 2f * extents, avoiding more costly division
            return 2f * extents;
        }
    }
}
