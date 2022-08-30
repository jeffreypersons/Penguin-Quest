using UnityEngine;


namespace PQ.Common.Collisions
{
    /*
    Box aligned with axes extending from center to right and top sides respectively.

    In other words, represents an (worldspace) oriented bounding box, given (local space)
    bounds of an axis-aligned bounding box.
    */
    public class OrientedBounds
    {
        private Bounds      _bounds;
        private Collider2D  _collider;
        private Rigidbody2D _rigidBody;
        private Transform   _transform;

        /* World position of bounding box's origin point. */
        public Vector2 Center      { get; private set; }

        /* Relative x axis - in relative forward direction with length of extents.x. */
        public Vector2 AxisX       { get; private set; }

        /* Relative y axis - in relative up direction with length of extents.y. */
        public Vector2 AxisY       { get; private set; }

        public Vector2 Size        { get; private set; }
        public Vector2 Extents     { get; private set; }
        public float   Rotation    { get; private set; }
        public float   Depth       { get; private set; }

        public Vector2 Forward     { get; private set; }
        public Vector2 Up          { get; private set; }
        public Vector2 Back        { get; private set; }
        public Vector2 Down        { get; private set; }

        public Vector2 RearBottom  { get; private set; }
        public Vector2 RearTop     { get; private set; }
        public Vector2 FrontBottom { get; private set; }
        public Vector2 FrontTop    { get; private set; }

        
        public override string ToString() =>
            $"OrientedBounds{{"         +
                $"Center:{Center},"     +
                $"Size:{Size},"         +
                $"Rotation:{Rotation}," +
                $"Depth:{Depth}";


        public OrientedBounds(Collider2D collider)
        {
            _collider  = collider;
            _rigidBody = collider.attachedRigidbody;
            _transform = collider.transform;
            Update();
        }

        public void Update()
        {
            Bounds bounds   = _collider.bounds;
            float  rotation = _collider.transform.eulerAngles.z;
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
            AxisX       = forwardAxis;
            AxisY       = upAxis;
            Size        = 2f * extents;
            Extents     = extents;
            Rotation    = rotation;
            Depth       = depth;
            Forward     = rightDir;
            Up          = upDir;
            Back        = -1 * rightDir;
            Down        = -1 * upDir;
            RearBottom  = new(min.x, min.y);
            RearTop     = new(min.x, max.y);
            FrontBottom = new(max.x, min.y);
            FrontTop    = new(max.x, max.y);
        }
    }
}
