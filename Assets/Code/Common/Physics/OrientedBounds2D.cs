using System;
using System.Diagnostics.Contracts;
using UnityEngine;


// todo: if it comes up, consider using Physics.Bounds for things like overlap
namespace PQ.Common.Physics
{
    /*
    Represents an orientation aligned bounding box, centered at position, oriented and sized according to forward/up axes.

    This construct is useful, as it can be used for extrapolation and other computations without having to work off of a
    transform, rigidbody, or collider.
    
    Additionally, the orientation alignment means we can take rotation explicitly into account, unlike axis-aligned bounds.
    Furthermore, by just using origin/x-axis/y-axis, we avoid having to worry about Euler angles and scale, greatly simplifying
    assumptions, logic, and computations that have to be made.
    */
    public sealed class OrientedBounds2D : IEquatable<OrientedBounds2D>
    {
        public Vector2 Center  { get; private set; }
        public Vector2 XAxis   { get; private set; }
        public Vector2 YAxis   { get; private set; }
        public float   Width   { get; private set; }
        public float   Height  { get; private set; }
        public Vector2 Back    { get; private set; }
        public Vector2 Forward { get; private set; }
        public Vector2 Below   { get; private set; }
        public Vector2 Above   { get; private set; }

        public override string ToString() =>
            $"{GetType().Name}(" +
                $"center:{Center}," +
                $"xAxis:{XAxis}," +
                $"yAxis:{YAxis})";
        
        public OrientedBounds2D() { }

        /*
        Map (unclamped) ratio along x,y axes to a 2d point.

        For example, (-1,-1), (0,0), (1,1) give our box min, center, max respectively;
        (2.0, 0.5) gives the point at twice along the x-axis (forward), halfway up along the y-axis (up).
        */
        public Vector2 InterpolateAlongAxes(float tXAxis, float tYaxis)
        {
            return Center + (tXAxis * XAxis) + (tYaxis * YAxis);
        }

        public Vector2 WorldToLocalPoint(Vector2 worldPoint)
        {
            return worldPoint - Center;
        }

        public Vector2 LocalToWorldPoint(Vector2 localPoint)
        {
            return Vector2.zero;
        }

        public Vector2 WorldToLocalDirection(Vector2 worldDirection)
        {
            return Vector2.zero;
        }

        public Vector2 LocalToWorldDirection(Vector2 localDirection)
        {
            return Vector2.zero;
        }


        public Vector2 ComputePositionDelta(Vector2 position)
        {
            return position - Center;
        }

        public float ComputeRotationDelta(Vector2 forward)
        {
            return SignedAngleBetweenUnitVectors(Forward, forward);
        }




        public bool MoveBy(Vector2 amount)
        {
            return SetFromOriginAndAxes(Center + amount, XAxis, YAxis);
        }

        public bool RotateBy(float degrees)
        {
            return SetFromOriginAndAxes(
                Center,
                xAxis: 0.5f * Width  * RotateUnitVector(Forward, degrees),
                yAxis: 0.5f * Height * RotateUnitVector(Above,   degrees));
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

            Vector2 min  = center - xAxis - yAxis;
            Vector2 max  = center + xAxis + yAxis;
            Vector2 size = new(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y));
            Center  = center;
            XAxis   = xAxis;
            YAxis   = yAxis;
            Forward = xAxis.normalized;
            Above   = yAxis.normalized;
            Back    = (-xAxis).normalized;
            Below   = (-yAxis).normalized;
            Width   = size.x;
            Height  = size.y;
            return true;
        }
        

        public override int GetHashCode() => HashCode.Combine(GetType(), Center, XAxis, YAxis);
        public override bool Equals(object obj) => ((IEquatable<OrientedBounds2D>)this).Equals(obj as OrientedBounds2D);
        bool IEquatable<OrientedBounds2D>.Equals(OrientedBounds2D other) => Equal(this, other);

        public static bool operator ==(OrientedBounds2D left, OrientedBounds2D right) =>  Equal(left, right);
        public static bool operator !=(OrientedBounds2D left, OrientedBounds2D right) => !Equal(left, right);



        [Pure]
        private static bool Equal(OrientedBounds2D left, OrientedBounds2D right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }
            if (left is null || right is null)
            {
                return false;
            }
            return left.AreBoundsApproximatelyEqual(right.Center, right.XAxis, right.YAxis);
        }


        [Pure]
        private bool AreBoundsApproximatelyEqual(Vector2 center, Vector2 xAxis, Vector2 yAxis)
        {
            return Mathf.Approximately(Center.x, center.x) && Mathf.Approximately(Center.y, center.y) &&
                   Mathf.Approximately(XAxis.x,  xAxis.x)  && Mathf.Approximately(XAxis.y,  xAxis.y)  &&
                   Mathf.Approximately(YAxis.x,  yAxis.x)  && Mathf.Approximately(YAxis.y,  yAxis.y);
        }


        [Pure]
        private static float SignedAngleBetweenUnitVectors(Vector2 from, Vector2 to)
        {
            // note that since we are working with unit vectors, we can skip most the computation Vector2.SignedAngle does
            // note since lengths are always 1, cosine-similarity [(a.b)/sqrt(|a|^2*|b|^2)] simplifies to a.b
            return Mathf.Sign(from.x * to.y - from.y * to.x) *
                   Mathf.Acos(Vector2.Dot(from, to)) *
                   Mathf.Rad2Deg;
        }

        [Pure]
        private static Vector2 RotateUnitVector(Vector2 unitVector, float signedDegrees)
        {
            // note since lengths are always 1, cosine-similarity [(a.b)/sqrt(|a|^2*|b|^2)] simplifies to a.b, where b is
            // the unit circle's x-axis (1,0), thus the dot product [(a.x)*(1) + ((a.x)*(0)] evaluates to a.x (domain [-1,1])
            float rotationCurrent = Mathf.Acos(unitVector.x);
            float rotationTarget  = rotationCurrent + (signedDegrees * Mathf.Deg2Rad);
            return new Vector2(x: Mathf.Cos(rotationTarget), y: Mathf.Sin(rotationTarget));
        }

        [Pure]
        private static Vector2 RotatePointAroundPivot(Vector2 point, Vector2 pivot, float degrees)
        {
            Vector2 pointLocalToPivot = point - pivot;
            float radians = degrees * Mathf.Deg2Rad;
            float cosTheta = Mathf.Cos(radians);
            float sinTheta = Mathf.Sin(radians);
            return new Vector2(
                x: pivot.x + (pointLocalToPivot.x * cosTheta) + (pointLocalToPivot.y * sinTheta),
                y: pivot.y - (pointLocalToPivot.x * sinTheta) + (pointLocalToPivot.y * cosTheta));
        }
    }
}
