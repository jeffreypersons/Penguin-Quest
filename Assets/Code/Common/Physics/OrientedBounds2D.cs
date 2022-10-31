using System;
using System.Diagnostics.Contracts;
using UnityEngine;


namespace PQ.Common.Physics
{
    /*
    Represents an orientation aligned bounding box, centered at position, oriented and sized according to forward/up axes.

    This construct is useful, as it can be used for extrapolation and other computations without having to work off of a
    transform, rigidbody, or collider.
    
    Additionally, the orientation alignment means we can take rotation explicitly into account, unlike axis-aligned bounds.
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

            [Pure] public Vector2 PointAt(float t) => Vector2.Lerp(start, end, t);
        }

        private Bounds _aab;
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

        [Pure] public static Vector2 ComputePositionDelta(OrientedBounds2D from, OrientedBounds2D to) =>
            to.Center - from.Center;

        [Pure] public static float   ComputeRotationDelta(OrientedBounds2D from, OrientedBounds2D to) =>
            Vector2.SignedAngle(from.XAxis, to.XAxis);


        public bool RotateBy(float degrees)
        {
            return SetFromOriginAndAxes(Center, Rotate(XAxis, degrees), Rotate(YAxis, degrees));
        }

        public bool MoveBy(Vector2 amount)
        {
            return SetFromOriginAndAxes(Center + amount, XAxis, YAxis);
        }



        public bool SetFrom(OrientedBounds2D other)
        {
            return SetFromOriginAndAxes(other.Center, other.XAxis, other.YAxis);
        }

        public bool SetFromCollider(Collider2D collider)
        {
            var bounds    = collider.bounds;
            var transform = collider.transform;
            return SetFromOriginAndAxes(
                center: bounds.center,
                xAxis:  bounds.extents.x * transform.right.normalized,
                yAxis:  bounds.extents.y * transform.up.normalized
            );
        }

        public bool SetFromOriginAndAxes(Vector2 center, Vector2 xAxis, Vector2 yAxis)
        {
            if (xAxis == Vector2.zero || yAxis == Vector2.zero)
            {
                throw new ArgumentException($"Axes must be nonzero - received {xAxis} and up {yAxis} axes");
            }
            if (Vector2.Dot(xAxis, yAxis) != 0f)
            {
                throw new ArgumentException($"Axes must be orthorgonal - received forward {xAxis} and up {yAxis} axes");
            }
            if (AreBoundsApproximatelyEqual(center, xAxis, yAxis))
            {
                return false;
            }

            Vector2 min         = center - xAxis - yAxis;
            Vector2 max         = center + xAxis + yAxis;
            Vector2 size        = new(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y));
            float   rotation    = DegreesBetween(Vector2.right, xAxis);
            Vector2 rearBottom  = new(min.x, min.y);
            Vector2 rearTop     = new(min.x, max.y);
            Vector2 frontBottom = new(max.x, min.y);
            Vector2 frontTop    = new(max.x, max.y);

            _aab     = new Bounds(center, size);
            Center   = center;
            Size     = size;
            XAxis    = xAxis;
            YAxis    = yAxis;
            Rotation = rotation;
            Back     = new(start: rearBottom,  end: rearTop,     normal: (-xAxis).normalized);
            Front    = new(start: frontBottom, end: frontTop,    normal: xAxis.normalized);
            Bottom   = new(start: rearBottom,  end: frontBottom, normal: (-yAxis).normalized);
            Top      = new(start: rearTop,     end: frontTop,    normal: yAxis.normalized);
            return true;
        }
        

        public override int GetHashCode() => HashCode.Combine(GetType(), Center, XAxis, YAxis);
        public override bool Equals(object obj) => ((IEquatable<OrientedBounds2D>)this).Equals(obj as OrientedBounds2D);
        bool IEquatable<OrientedBounds2D>.Equals(OrientedBounds2D other) => Equal(this, other);

        public static bool operator ==(OrientedBounds2D left, OrientedBounds2D right) =>  Equal(left, right);
        public static bool operator !=(OrientedBounds2D left, OrientedBounds2D right) => !Equal(left, right);


        [Pure] private static bool Equal(OrientedBounds2D left, OrientedBounds2D right) =>
            ReferenceEquals(left, right) ||
            (left is not null && right is not null && left.AreBoundsApproximatelyEqual(right.Center, right.XAxis, right.YAxis));
        
        [Pure] private bool AreBoundsApproximatelyEqual(Vector2 center, Vector2 xAxis, Vector2 yAxis) =>
            Mathf.Approximately(Center.x, center.x) && Mathf.Approximately(Center.y, center.y) &&
            Mathf.Approximately(XAxis.x,  xAxis.x)  && Mathf.Approximately(XAxis.y,  xAxis.y)  &&
            Mathf.Approximately(YAxis.x,  yAxis.x)  && Mathf.Approximately(YAxis.y,  yAxis.y);


        // todo: lots of room for optimization here, since if we assume normalized vectors in a 2d plane,
        //       we can avoid quaternions and square roots and more
        [Pure] private static float DegreesBetween(Vector2 from, Vector2 to) =>
            Vector2.SignedAngle(from, to);
        [Pure] private static Vector2 Rotate(Vector2 vector, float degrees) =>
            Quaternion.AngleAxis(degrees, vector) * vector;
    }
}
