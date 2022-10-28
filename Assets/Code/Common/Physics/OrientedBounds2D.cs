using System;
using UnityEngine;


namespace PQ.Common.Physics
{
    /*
    Represents an orientation aligned bounding box, centered at position, oriented and sized according to forward/up axes.
    */
    public sealed class OrientedBounds2D
    {
        public struct Side
        {
            public readonly Vector2 start;
            public readonly Vector2 end;
            public readonly Vector2 normal;

            public override string ToString() =>
                $"{GetType().Name}(" +
                    $"center:{start}," +
                    $"xAxis:{end}," +
                    $"yAxis:{normal})";

            public Side(in Vector2 start, in Vector2 end, in Vector2 normal)
            {
                this.start  = start;
                this.end    = end;
                this.normal = normal;
            }
        }

        public Vector2 Center { get; private set; }
        public Vector2 XAxis  { get; private set; }
        public Vector2 YAxis  { get; private set; }
        public Side    Back   { get; private set; }
        public Side    Front  { get; private set; }
        public Side    Bottom { get; private set; }
        public Side    Top    { get; private set; }

        public override string ToString() =>
            $"{GetType().Name}(" +
                $"center:{Center}," +
                $"xAxis:{XAxis}," +
                $"yAxis:{YAxis})";
        

        public OrientedBounds2D() { }

        /* Given position and axes, adjust such that it's aligned and scaled with given forward and up vectors. */
        public void Update(Vector2 center, Vector2 xAxis, Vector2 yAxis)
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
                return;
            }
            
            Vector2 min = center - xAxis - yAxis;
            Vector2 max = center + xAxis + yAxis;
            Vector2 rearBottom  = new(min.x, min.y);
            Vector2 rearTop     = new(min.x, max.y);
            Vector2 frontBottom = new(max.x, min.y);
            Vector2 frontTop    = new(max.x, max.y);

            Back   = new(start: rearBottom,  end: rearTop,     normal: (-xAxis).normalized);
            Front  = new(start: frontBottom, end: frontTop,    normal: xAxis.normalized);
            Bottom = new(start: rearBottom,  end: frontBottom, normal: (-yAxis).normalized);
            Top    = new(start: rearTop,     end: frontTop,    normal: yAxis.normalized);
        }
    }
}
