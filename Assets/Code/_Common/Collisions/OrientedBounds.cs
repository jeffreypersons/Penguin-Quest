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
        private Bounds      _bounds;
        private Collider2D  _collider;
        private Rigidbody2D _rigidBody;
        private Transform   _transform;

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
            _collider  = collider;
            _rigidBody = collider.attachedRigidbody;
            _transform = collider.transform;
            Update();
        }

        public void Update()
        {
            Bounds bounds   = _collider.bounds;
            float  rotation = _collider.transform.localEulerAngles.z;
            if (bounds != _bounds || !Mathf.Approximately(rotation, Rotation))
            {
                _bounds = bounds;
                Set(bounds.center,
                    bounds.extents,
                    _transform.right.normalized,
                    _transform.up.normalized,
                    rotation, bounds.center.z);
            }
        }


        private void Set(Vector2 center, Vector2 extents, Vector2 rightDir, Vector2 upDir, float rotation, float depth)
        {
            Vector2 forwardAxis = extents.x * rightDir;
            Vector2 upAxis      = extents.y * upDir;
            Vector2 min         = center - forwardAxis - upAxis;
            Vector2 max         = center + forwardAxis + upAxis;

            Center      = center;
            ForwardAxis = forwardAxis;
            UpAxis      = upAxis;
            Size        = 2f * extents;
            Extents     = extents;
            Rotation    = rotation;
            Depth       = depth;
            ForwardDir  = rightDir;
            UpDir       = upDir;
            BehindDir   = -1 * rightDir;
            DownDir     = -1 * upDir;
            LeftBottom  = new Vector2(min.x, min.y);
            LeftTop     = new Vector2(min.x, max.y);
            RightBottom = new Vector2(max.x, min.y);
            RightTop    = new Vector2(max.x, max.y);
        }
    }
}
