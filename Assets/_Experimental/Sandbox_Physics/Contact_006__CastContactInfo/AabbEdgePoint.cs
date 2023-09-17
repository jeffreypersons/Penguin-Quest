using System;
using System.Diagnostics.Contracts;
using UnityEngine;


namespace PQ._Experimental.Physics.Contact_006
{
    public struct AabbEdgePoint
    {
        public readonly float Angle;

        private readonly Vector2 _localPoint;

        public Vector2 Point(Vector2 center, Vector2 extents) => center + Vector2.Scale(_localPoint, 2f * extents);


        public AabbEdgePoint(float angle)
        {
            #if UNITY_EDITOR
            if (angle is < 0 or > 360)
            {
                throw new ArgumentException($"Angle must be between 0 and 360, received index={angle}");
            }
            #endif
            Angle = angle;
            _localPoint = FindPointOnEdgeOfUnitSquare(Mathf.Deg2Rad * angle);
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
    }
}
