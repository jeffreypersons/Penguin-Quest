using PQ.Common.Extensions;
using UnityEngine;


namespace PQ.Common.Collisions
{
    /*
    Box aligned with axes extending from center to right and top sides respectively.

    In other words, represents an (worldspace) oriented bounding box, given (local space)
    bounds of an axis-aligned bounding box.
    */
    public class OrientedBoundingBox
    {
        private Collider2D _collider;
        private Transform _transform;
        private Rigidbody2D _rigidBody;


        public Vector2 Center      { get; private set; }
        public Vector2 ForwardAxis { get; private set; }
        public Vector2 UpAxis      { get; private set; }

        public Vector2 Size        { get; private set; }
        public Vector2 Extents     { get; private set; }
        public float   Rotation    { get; private set; }
        public float   Depth       { get; private set; }

        public Vector2 ForwardDir  { get; private set; }
        public Vector2 UpDir       { get; private set; }
        public Vector2 BehindDir   { get; private set; }
        public Vector2 DownDir     { get; private set; }

        public Vector2 LeftBottom  { get; private set; }
        public Vector2 LeftTop     { get; private set; }
        public Vector2 RightBottom { get; private set; }
        public Vector2 RightTop    { get; private set; }

        
        public override string ToString() =>
            $"OrientedBounds{{"         +
                $"Center:{Center},"     +
                $"Size:{Size},"         +
                $"Rotation:{Rotation}," +
                $"Depth:{Depth}";


        public OrientedBoundingBox(Collider2D collider)
        {
            _collider = collider;
            _transform = _collider.transform;
            _rigidBody = _collider.attachedRigidbody;
            Update();
        }

        public void Update()
        {
            Vector2 worldPosition = _collider.bounds.center;
            Vector2 worldExtents  = _collider.bounds.extents;
            float   worldRotation = _rigidBody.rotation;

            if (!MathExtensions.AreComponentsEqual(worldPosition, Center) ||
                !MathExtensions.AreComponentsEqual(worldExtents, Extents) ||
                !MathExtensions.AreScalarsEqual(worldRotation, Rotation))
            {
                Set(center:   worldPosition,
                    extents:  worldExtents,
                    rightDir: _transform.right.normalized,
                    upDir:    _transform.up.normalized,
                    depth:    _transform.position.z);
            }
        }

        private void Set(Vector2 center, Vector2 extents, Vector2 rightDir, Vector2 upDir, float depth)
        {
            Vector2 forwardAxis = extents.x * rightDir;
            Vector2 upAxis      = extents.y * upDir;
            Vector2 min = center - forwardAxis - upAxis;
            Vector2 max = center + forwardAxis + upAxis;

            Center      = center;
            ForwardAxis = forwardAxis;
            UpAxis      = upAxis;
            Size        = 2f * extents;
            Extents     = extents;
            Rotation    = Vector2.Angle(from: upDir, to: upDir);
            Depth       = depth;
            ForwardDir  = rightDir;
            UpDir       = upDir;
            BehindDir   = -1f * rightDir;
            DownDir     = -1f * upDir;
            LeftBottom  = new Vector2(min.x, min.y);
            LeftTop     = new Vector2(min.x, max.y);
            RightBottom = new Vector2(max.x, min.y);
            RightTop    = new Vector2(max.x, max.y);
        }
    }
}
