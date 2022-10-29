﻿using System;
using System.Diagnostics.Contracts;
using UnityEngine;


namespace PQ.Common.Physics
{
    /*
    Represents an orientation aligned bounding box, centered at position, oriented and sized according to forward/up axes.
    */
    public sealed class OrientedBounds2D : IEquatable<OrientedBounds2D>
    {
        public struct Side
        {
            public readonly Vector2 start;
            public readonly Vector2 end;
            public readonly Vector2 normal;
            public override string ToString() => $"{GetType().Name}(start:{start}, end:{end}, normal:{normal})";

            public Side(Vector2 start, Vector2 end, Vector2 normal)
            {
                this.start  = start;
                this.end    = end;
                this.normal = normal;
            }

            [Pure]
            public Vector2 PointAt(float t)
            {
                return Vector2.Lerp(start, end, t);
            }
        }

        public Vector2 Center   { get; private set; }
        public Vector2 Size     { get; private set; }
        public float   Rotation { get; private set; }
        public Vector2 XAxis    { get; private set; }
        public Vector2 YAxis    { get; private set; }
        public Side    Back     { get; private set; }
        public Side    Front    { get; private set; }
        public Side    Bottom   { get; private set; }
        public Side    Top      { get; private set; }

        public override string ToString() =>
            $"{GetType().Name}(" +
                $"center:{Center}," +
                $"xAxis:{XAxis}," +
                $"yAxis:{YAxis})";
        
        public OrientedBounds2D() { }


        /* Given position and axes, adjust such that it's aligned and scaled with given forward and up vectors. */
        public bool Update(Vector2 center, Vector2 xAxis, Vector2 yAxis)
        {
            if (xAxis == Vector2.zero || yAxis == Vector2.zero)
            {
                throw new ArgumentException($"Axes must be nonzero - received {xAxis} and up {yAxis} axes");
            }
            if (Vector2.Dot(xAxis, yAxis) != 0f)
            {
                throw new ArgumentException($"Axes must be orthorgonal - received forward {xAxis} and up {yAxis} axes");
            }
            if (Center == center && XAxis == xAxis && YAxis == yAxis)
            {
                return false;
            }
            
            Vector2 min = center - xAxis - yAxis;
            Vector2 max = center + xAxis + yAxis;
            Vector2 size = new(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y));
            Vector2 rearBottom  = new(min.x, min.y);
            Vector2 rearTop     = new(min.x, max.y);
            Vector2 frontBottom = new(max.x, min.y);
            Vector2 frontTop    = new(max.x, max.y);

            Center = center;
            Size   = size;
            XAxis  = xAxis;
            YAxis  = yAxis;
            Rotation = Vector2.Angle(Vector2.right, xAxis);
            Back   = new(start: rearBottom,  end: rearTop,     normal: (-xAxis).normalized);
            Front  = new(start: frontBottom, end: frontTop,    normal: xAxis.normalized);
            Bottom = new(start: rearBottom,  end: frontBottom, normal: (-yAxis).normalized);
            Top    = new(start: rearTop,     end: frontTop,    normal: yAxis.normalized);
            return true;
        }


        bool IEquatable<OrientedBounds2D>.Equals(OrientedBounds2D other) =>
            Center == other.Center && XAxis == other.XAxis && YAxis == other.YAxis;
        public override bool Equals(object obj) => ((IEquatable<OrientedBounds2D>)this).Equals(obj as OrientedBounds2D);
        public override int GetHashCode() => HashCode.Combine(GetType(), Center, XAxis, YAxis);


        // computed the unsigned degrees (between [0, 90]) between given vec and y axis
        private static float AngleFromYAxis(Vector2 vector)
        {
            if (Mathf.Approximately(vector.x, 0.00f))
            {
                return 0.00f;
            }
            if (Mathf.Approximately(vector.y, 0.00f))
            {
                return 90.00f;
            }
            return vector.y < 0.00f ? Vector2.Angle(Vector2.down, vector) : Vector2.Angle(Vector2.up, vector);
        }
    }
}
