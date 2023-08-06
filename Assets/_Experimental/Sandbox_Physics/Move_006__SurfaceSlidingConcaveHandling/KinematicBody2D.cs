using System;
using UnityEngine;


namespace PQ._Experimental.Physics.Move_006
{
    internal sealed class KinematicBody2D
    {
        private Transform        _transform;
        private Rigidbody2D      _rigidbody;
        private BoxCollider2D    _boxCollider;
        private ContactFilter2D  _contactFilter;
        private RaycastHit2D[]   _hitBuffer;
        private Collider2D[]     _overlapBuffer;
        private ContactPoint2D[] _contactBuffer;

        private const int DefaultBufferSize = 16;

        public override string ToString() =>
            $"{GetType()}{{" +
                $"Position:{Position}," +
                $"Forward:{Forward}," +
                $"Up:{Up}," +
                $"AABB:bounds(center:{Center},extents:{Extents})," +
                $"SkinWidth:{SkinWidth}," +
                $"LayerMask:{LayerMask}," +
            $"}}";

        public LayerMask LayerMask => _contactFilter.layerMask;

        public Vector2 Position
        {
            get => _rigidbody.position;
            set => _rigidbody.position = value;
        }

        public Vector3 Rotation
        {
            get => _transform.localEulerAngles;
            set => _transform.localEulerAngles = value;
        }

        public Vector2 Center    => _boxCollider.bounds.center;
        public Vector2 Forward   => _transform.right.normalized;
        public Vector2 Up        => _transform.up.normalized;
        public Vector2 Extents   => _boxCollider.bounds.extents + new Vector3(_boxCollider.edgeRadius, _boxCollider.edgeRadius, 0f);
        public float   Depth     => _transform.position.z;
        public float   SkinWidth => _boxCollider.edgeRadius;
        

        public KinematicBody2D(Transform transform)
        {
            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform));
            }
            if (!transform.TryGetComponent(out Rigidbody2D rigidbody2D))
            {
                throw new MissingComponentException($"Expected attached {nameof(Rigidbody2D)} - not found on {nameof(transform)}");
            }
            if (!transform.TryGetComponent(out BoxCollider2D boxCollider2D))
            {
                throw new MissingComponentException($"Expected attached {nameof(BoxCollider2D)} - not found on {nameof(transform)}");
            }
            if (!ReferenceEquals(boxCollider2D.attachedRigidbody, rigidbody2D))
            {
                throw new MissingComponentException($"Expected attached {nameof(Rigidbody2D)} - not found on {nameof(boxCollider2D)}");
            }

            _transform     = rigidbody2D.transform;
            _rigidbody     = rigidbody2D;
            _boxCollider   = boxCollider2D;
            _contactFilter = new ContactFilter2D();
            _hitBuffer     = new RaycastHit2D[DefaultBufferSize];
            _overlapBuffer = new Collider2D[DefaultBufferSize];
            _contactBuffer = new ContactPoint2D[DefaultBufferSize];

            _contactFilter.useTriggers    = false;
            _contactFilter.useNormalAngle = false;
            _contactFilter.SetLayerMask(LayerMask.GetMask("Solids"));

            _rigidbody.simulated   = true;
            _rigidbody.isKinematic = true;
            _rigidbody.useFullKinematicContacts = true;
            _rigidbody.constraints = RigidbodyConstraints2D.None;
        }

        /* Check if body is filtering out collisions with given object or not. */
        public bool IsFilteringLayerMask(GameObject other)
        {
            return _contactFilter.IsFilteringLayerMask(other);
        }

        /*
        Use separating axis theorem to determine distance needed for no overlap.
        
        May require multiple calls for complex polygons.
        */
        public ColliderDistance2D ComputeMinimumSeparation(Collider2D collider)
        {
            if (collider == null)
            {
                throw new InvalidOperationException($"Invalid minimum separation distance between body={_boxCollider.name} and collider=null");
            }

            // ensure it's possible to get a valid minimum separation (ie both non-null and enabled)
            ColliderDistance2D minimumSeparation = _boxCollider.Distance(collider);
            if (!minimumSeparation.isValid)
            {
                throw new InvalidOperationException($"Invalid minimum separation distance between body={_boxCollider.name} and collider={collider.name}");
            }
            return minimumSeparation;
        }

        /*
        Reset to position at start of frame and apply MovePosition. This preserves interpolation despite any changes to position.
        
        This works around the fact that modifying rigidbody.position prevents unity from applying smoothing based on previous
        frame positions. By 'clearing' any manual changes to position and invoking movePosition, we can maintain interpolation.
        */
        public void MovePositionWithoutBreakingInterpolation(Vector2 startPositionThisFrame, Vector2 targetPositionThisFrame)
        {
            _rigidbody.position = startPositionThisFrame;
            _rigidbody.MovePosition(targetPositionThisFrame);
        }
        
        
        /*
        Check if body center is fully surrounded by the same edge collider.
        
        Considered to be 'inside' if there is an edge collider above center of our AABB, and the same edge collider below.
        Assumes there aren't any edge collider inside another.
        */
        public bool IsCenterBoundedByAnEdgeCollider(out EdgeCollider2D collider)
        {
            Vector2 origin = _boxCollider.bounds.center;

            if (!CastRay(origin, Vector2.up, Mathf.Infinity, out var aboveHit) ||
                !aboveHit.collider.transform.TryGetComponent<EdgeCollider2D>(out var edge))
            {
                collider = default;
                return false;
            }

            float maxHorizontal = 2f * edge.bounds.extents.x;
            float maxVertical   = 2f * edge.bounds.extents.y;
            if (!CastRayAt(edge, origin, Vector2.down,  maxVertical,   out var _) ||
                !CastRayAt(edge, origin, Vector2.left,  maxHorizontal, out var _) ||
                !CastRayAt(edge, origin, Vector2.right, maxHorizontal, out var _))
            {
                collider = default;
                return false;
            }

            collider = edge;
            return true;
        }

        /*
        Project AABB along given delta from AABB center, and outputs ALL hits (if any).

        Note that casts ignore body's bounds, and all Physics2D cast results are sorted by ascending distance.
        */
        public bool CastAABB(Vector2 direction, float distance, out RaycastHit2D hit)
        {
            if (_boxCollider.Cast(direction, _contactFilter, _hitBuffer, distance) > 0)
            {
                hit = _hitBuffer[0];
            }
            else
            {
                hit = default;
            }
            return hit;
        }


        /*
        Project point along given delta from given origin, and outputs ALL hits (if any).

        Note that casts ignore body's bounds, and all Physics2D cast results are sorted by ascending distance.
        */
        public bool CastRay(Vector2 origin, Vector2 direction, float distance, out RaycastHit2D hit)
        {
            int layer = _transform.gameObject.layer;
            _transform.gameObject.layer = Physics2D.IgnoreRaycastLayer;

            if (Physics2D.Raycast(origin, direction, _contactFilter, _hitBuffer, distance) > 0)
            {
                hit = _hitBuffer[0];
            }
            else
            {
                hit = default;
            }

            _transform.gameObject.layer = layer;

            Debug.DrawLine(origin, origin + distance * direction, Color.red, 1f);
            if (hit)
            {
                Debug.DrawLine(origin, hit.point, Color.green, 1f);
            }
            return hit;
        }

        /*
        Project center point along given direction, outputting first hit to given collider (if any).
        */
        public bool CastRayAt(Collider2D collider, Vector2 origin, Vector2 direction, float distance, out RaycastHit2D hit)
        {
            int layer = _transform.gameObject.layer;
            _transform.gameObject.layer = Physics2D.IgnoreRaycastLayer;

            hit = default;
            int hitCount = Physics2D.Raycast(origin, direction, _contactFilter, _hitBuffer, distance);
            for (int i = 0; i < hitCount; i++)
            {
                if (_hitBuffer[i].collider == collider)
                {
                    hit = _hitBuffer[i];
                    break;
                }
            }

            _transform.gameObject.layer = layer;

            Debug.DrawLine(origin, origin + distance * direction, Color.red, 1f);
            if (hit)
            {
                Debug.DrawLine(origin, hit.point, Color.green, 1f);
            }
            return hit;
        }

        /*
        Compute distance from center to edge of our bounding box in given direction.
        */
        public float ComputeDistanceToEdge(Vector2 direction)
        {
            Bounds bounds = _boxCollider.bounds;
            bounds.IntersectRay(new Ray(bounds.center, direction), out float distanceFromCenterToEdge);

            // discard sign since distance is negative if starts within bounds (contrary to other ray methods)
            return Mathf.Abs(distanceFromCenterToEdge);
        }
    }
}
