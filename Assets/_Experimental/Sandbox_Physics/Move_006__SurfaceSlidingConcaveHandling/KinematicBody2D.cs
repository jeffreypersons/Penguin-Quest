using System;
using UnityEngine;


namespace PQ._Experimental.Physics.Move_006
{
    [Flags]
    public enum ContactFlags2D
    {
        None              = 0,
        LeftSide          = 1 << 1,
        BottomLeftCorner  = 1 << 2,
        BottomSide        = 1 << 3,
        BottomRightCorner = 1 << 4,
        RightSide         = 1 << 5,
        TopRightCorner    = 1 << 6,
        TopSide           = 1 << 7,
        TopLeftCorner     = 1 << 8,
        All               = ~0,
    }
    internal sealed class KinematicBody2D
    {
        private Transform        _transform;
        private Rigidbody2D      _rigidbody;
        private BoxCollider2D    _boxCollider;
        private ContactFilter2D  _contactFilter;

        private RaycastHit2D[]   _hitBuffer;
        private RaycastHit2D[]   _hitBufferSecondary;
        private Collider2D[]     _overlapBuffer;
        private ContactPoint2D[] _contactBuffer;

        private LayerMask _previousLayerMask;

        private const float DefaultEpsilon = 0.005f;
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

            _transform          = rigidbody2D.transform;
            _rigidbody          = rigidbody2D;
            _boxCollider        = boxCollider2D;
            _contactFilter      = new ContactFilter2D();
            _hitBuffer          = new RaycastHit2D  [DefaultBufferSize];
            _hitBufferSecondary = new RaycastHit2D  [DefaultBufferSize];
            _overlapBuffer      = new Collider2D    [DefaultBufferSize];
            _contactBuffer      = new ContactPoint2D[DefaultBufferSize];

            _contactFilter.useTriggers    = false;
            _contactFilter.useNormalAngle = false;
            _contactFilter.SetLayerMask(LayerMask.GetMask("Solids"));

            _rigidbody.simulated   = true;
            _rigidbody.isKinematic = true;
            _rigidbody.useFullKinematicContacts = true;
            _rigidbody.constraints = RigidbodyConstraints2D.None;
        }

        private void EnableCollisionsWithAABB()
        {
            _previousLayerMask = _transform.gameObject.layer;
            _transform.gameObject.layer = Physics2D.IgnoreRaycastLayer;
        }

        private void DisableCollisionsWithAABB()
        {
            _transform.gameObject.layer = _previousLayerMask;
        }

        /* Check if body is filtering out collisions with given object or not. */
        public bool IsFilteringLayerMask(GameObject other)
        {
            return _contactFilter.IsFilteringLayerMask(other);
        }

        public bool IsTouching()
        {
            return _boxCollider.IsTouching(_contactFilter);
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

        public ContactFlags2D CheckSides()
        {
            _contactFilter.useNormalAngle = true;

            bool isDiagonal = false;
            int degrees = 0;
            ContactFlags2D flags = ContactFlags2D.None;
            for (int i = 0; i < 7; i++)
            {
                if (isDiagonal)
                {
                    _contactFilter.SetNormalAngle(degrees - 45 + DefaultEpsilon, degrees + 45 - DefaultEpsilon);
                }
                else
                {
                    _contactFilter.SetNormalAngle(degrees - DefaultEpsilon, degrees + DefaultEpsilon);
                }
                
                if (_boxCollider.IsTouching(_contactFilter))
                {
                    flags |= (ContactFlags2D)(1 << (i+1));
                }

                degrees += 45;
                isDiagonal = !isDiagonal;
            }
            _contactFilter.useNormalAngle = false;
            return flags;
        }

        /*
        Check for overlapping colliders within our bounding box.
        */
        public bool CheckForOverlappingColliders(Vector2 extents, out ReadOnlySpan<Collider2D> colliders)
        {
            int layer = _transform.gameObject.layer;
            _transform.gameObject.layer = Physics2D.IgnoreRaycastLayer;

            int colliderCount = Physics2D.OverlapBox(_boxCollider.bounds.center, 2f * extents, 0f, _contactFilter, _overlapBuffer);
            colliders = _overlapBuffer.AsSpan(0, colliderCount);

            _transform.gameObject.layer = layer;
            return !colliders.IsEmpty;
        }

        /*
        Project AABB along given delta from AABB center, and outputs ALL hits (if any).

        Note that casts ignore body's bounds, and all Physics2D cast results are sorted by ascending distance.
        */
        public bool CastAABB(Vector2 direction, float distance, out RaycastHit2D hit)
        {
            // note that there is no need to disable colliders as that is accounted for by collider instance
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
        Project a box along given delta.
        
        Note that casts ignore body's bounds, and all Physics2D cast results are sorted by ascending distance.
        */
        public bool CastBox(Vector2 origin, float angle, Vector2 extents, Vector2 direction, float distance, out RaycastHit2D hit)
        {
            DisableCollisionsWithAABB();
            if (Physics2D.BoxCast(origin, 2f * extents, angle, direction, _contactFilter, _hitBuffer, distance) > 0)
            {
                hit = _hitBuffer[0];
            }
            else
            {
                hit = default;
            }
            EnableCollisionsWithAABB();

            #if UNITY_EDITOR
            Debug.DrawLine(origin, origin + distance * direction, Color.red, 1f);
            if (hit)
            {
                Debug.DrawLine(origin, hit.point, Color.green, 1f);
            }
            #endif
            return hit;
        }

        /*
        Project a circle along given delta.
        
        Note that casts ignore body's bounds, and all Physics2D cast results are sorted by ascending distance.
        */
        public bool CastCircle(Vector2 origin, float radius, Vector2 direction, float distance, out RaycastHit2D hit)
        {
            DisableCollisionsWithAABB();
            hit = default;
            if (Physics2D.CircleCast(origin, radius, direction, _contactFilter, _hitBuffer, distance) > 0)
            {
                hit = _hitBuffer[0];
            }
            EnableCollisionsWithAABB();
            
            #if UNITY_EDITOR
            Debug.DrawLine(origin, origin + distance * direction, Color.red, 1f);
            if (hit)
            {
                Debug.DrawLine(origin, hit.point, Color.green, 1f);
            }
            #endif
            return hit;
        }

        /*
        Project point along given delta from given origin, and outputs ALL hits (if any).

        Note that casts ignore body's bounds, and all Physics2D cast results are sorted by ascending distance.
        */
        public bool CastRay(Vector2 origin, Vector2 direction, float distance, out RaycastHit2D hit)
        {
            DisableCollisionsWithAABB();
            if (Physics2D.Raycast(origin, direction, _contactFilter, _hitBuffer, distance) > 0)
            {
                hit = _hitBuffer[0];
            }
            else
            {
                hit = default;
            }
            EnableCollisionsWithAABB();

            #if UNITY_EDITOR
            Debug.DrawLine(origin, origin + distance * direction, Color.red, 1f);
            if (hit)
            {
                Debug.DrawLine(origin, hit.point, Color.green, 1f);
            }
            #endif
            return hit;
        }

        /*
        Project center point along given direction, outputting first hit to given collider (if any).
        */
        public bool CastRayAt(Collider2D collider, Vector2 origin, Vector2 direction, float distance, out RaycastHit2D hit)
        {
            DisableCollisionsWithAABB();
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
            EnableCollisionsWithAABB();

            #if UNITY_EDITOR
            Debug.DrawLine(origin, origin + distance * direction, Color.red, 1f);
            if (hit)
            {
                Debug.DrawLine(origin, hit.point, Color.green, 1f);
            }
            #endif
            return hit;
        }

        /*
        Through the side in given direction, project n points outwards, outputting each hit(s).

        spreadExtent is defined as the half length of the segment spanning tangent to the closest corner.
        Angles of (315,45]=>right (45,135]=>top (135,180]=>left (180,315]=>bottom
        
        Note does not account for edge radius curving at the corners - casts origins are along edge of AABB instead.
        Note that results length is always equal to count.
        */
        public bool CastRaysFromCorner(float spreadExtent, Vector2 direction, float distance, int rayCount, out int hitCount, out ReadOnlySpan<RaycastHit2D> results)
        {
            #if UNITY_EDITOR
            if (rayCount is < 3 or > DefaultBufferSize)
            {
                throw new ArgumentException($"Ray count must be in range=[3,{DefaultBufferSize}], received={rayCount}");
            }
            #endif
            
            DisableCollisionsWithAABB();
            int totalHits = 0;
            (Vector2 normal, Vector2 position) = FindClosestCorner(direction);
            Vector2 tangent = Vector2.Perpendicular(normal);
            Vector2 start = position + spreadExtent * tangent;
            Vector2 end = position - spreadExtent * tangent;
            Debug.DrawLine(start, end, Color.blue, 1f);

            Vector2 delta = (end - start) / (rayCount-1);
            for (int rayIndex = 0; rayIndex < rayCount; rayIndex++)
            {
                Vector2 origin = start + (rayIndex * delta);
                if (Physics2D.Raycast(origin, normal, _contactFilter, _hitBuffer, distance) > 0)
                {
                    totalHits++;
                    _hitBufferSecondary[rayIndex] = _hitBuffer[0];
                }
                else
                {
                    _hitBufferSecondary[rayIndex] = default;
                }

                #if UNITY_EDITOR
                Debug.DrawLine(origin, origin + distance * normal, Color.red, 1f);
                if (_hitBufferSecondary[rayIndex])
                {
                    Debug.DrawLine(origin, origin + _hitBufferSecondary[rayIndex].distance * normal, Color.green, 1f);
                }
                #endif
            }
            results = _hitBufferSecondary.AsSpan(0, rayCount);
            hitCount = totalHits;
            EnableCollisionsWithAABB();
            return hitCount > 0;
        }

        /*
        Through the side in given direction, project n points outwards, outputting each hit(s).

        Angles of (315,45]=>right (45,135]=>top (135,180]=>left (180,315]=>bottom
        
        Note does not account for edge radius curving at the corners - casts origins are along edge of AABB instead.
        Note that results length is always equal to count.
        */
        public bool CastRaysFromSide(Vector2 direction, float distance, int rayCount, out int hitCount, out ReadOnlySpan<RaycastHit2D> results)
        {
            #if UNITY_EDITOR
            if (rayCount is < 3 or > DefaultBufferSize)
            {
                throw new ArgumentException($"Ray count must be in range=[3,{DefaultBufferSize}], received={rayCount}");
            }
            #endif
            
            DisableCollisionsWithAABB();
            int totalHits = 0;
            (Vector2 normal, Vector2 start, Vector2 end) = FindIntersectingSide(direction);
            Vector2 delta = (end - start) / (rayCount-1);
            for (int rayIndex = 0; rayIndex < rayCount; rayIndex++)
            {
                Vector2 origin = start + (rayIndex * delta);
                if (Physics2D.Raycast(origin, normal, _contactFilter, _hitBuffer, distance) > 0)
                {
                    totalHits++;
                    _hitBufferSecondary[rayIndex] = _hitBuffer[0];
                }
                else
                {
                    _hitBufferSecondary[rayIndex] = default;
                }

                #if UNITY_EDITOR
                Debug.DrawLine(origin, origin + distance * normal, Color.red, 1f);
                if (_hitBufferSecondary[rayIndex])
                {
                    Debug.DrawLine(origin, origin + _hitBufferSecondary[rayIndex].distance * normal, Color.green, 1f);
                }
                #endif
            }
            results = _hitBufferSecondary.AsSpan(0, rayCount);
            hitCount = totalHits;
            EnableCollisionsWithAABB();
            return hitCount > 0;
        }


        /*
        Check if AABB contains point.
        */
        public bool ContainsPointInBounds(Vector2 point)
        {
            Bounds bounds = _boxCollider.bounds;
            bounds.Expand(_boxCollider.edgeRadius);
            return bounds.Contains(point);
        }
        
        /*
        Project point _against_ body finding distance to intersection (if any).
        
        Works whether starting in or outside bounds.
        Unlike traditional ray methods, no need for maxDistance.
        Note not does not account for any rounded corners due to edge radius, for consistency with Unity Physics2D.
        */
        public bool IntersectAABB(Vector2 origin, Vector2 direction, out float distanceToEdge)
        {
            Bounds bounds = _boxCollider.bounds;
            bounds.Expand(_boxCollider.edgeRadius);

            bool foundIntersection = bounds.IntersectRay(new Ray(origin, direction), out distanceToEdge);

            // discard sign since distance is negative if starts within bounds (contrary to other ray methods)
            distanceToEdge = Mathf.Abs(distanceToEdge);

            #if UNITY_EDITOR
            Debug.DrawRay(origin, direction, Color.cyan, 1f);
            if (foundIntersection)
            {
                Debug.DrawLine(origin, origin + distanceToEdge * direction, Color.cyan, 1f);
            }
            #endif
            return foundIntersection;
        }

        /*
        Map given direction to a corner, returning it's position.
        Relative to right bottom-left-corner, angles map as [270,0)=>right [0,90)=>top [90,180)=>left [180,270]=>bottom
        Note that start is from bottom and left respectively.roundedCornerStart
        */
        private (Vector2 normal, Vector2 position) FindClosestCorner(Vector2 direction)
        {
            // map angle to side's normal and corner coordinates, checking from lower right corner of the box
            float degrees = Vector2.SignedAngle(new Vector2(0, -1), direction);
            if (degrees <= 0)
            {
                degrees = 360f + degrees;
            }
            Vector2 offset = degrees switch
            {
                <= 90f  => new Vector2( 1, -1),
                <= 180f => new Vector2( 1,  1),
                <= 270f => new Vector2(-1,  1),
                _       => new Vector2(-1, -1),
            };
            
            Vector2 center = _boxCollider.bounds.center;
            Vector2 extents = (Vector2)_boxCollider.bounds.extents + new Vector2(_boxCollider.edgeRadius, _boxCollider.edgeRadius);
            return (offset.normalized, center + extents * offset);
        }

        /*
        Map given direction to a side, returning it's normal and start end points.
        Relative to right world-axis, angles map as (315,45]=>right (45,135]=>top (135,180]=>left (180,315]=>bottom
        Note that start is from bottom and left respectively.
        */
        private (Vector2 normal, Vector2 start, Vector2 end) FindIntersectingSide(Vector2 direction)
        {
            // map angle to side's normal and corner coordinates, checking from lower right corner of the box
            float degrees = Vector2.SignedAngle(new Vector2(1, -1), direction);
            if (degrees <= 0)
            {
                degrees = 360f + degrees;
            }
            (Vector2 normal, Vector2 cornerStart, Vector2 cornerEnd) = degrees switch
            {
                <= 90f  => (Vector2.right, new Vector2( 1, -1), new Vector2( 1,  1)),
                <= 180f => (Vector2.up,    new Vector2(-1,  1), new Vector2( 1,  1)),
                <= 270f => (Vector2.left,  new Vector2(-1, -1), new Vector2(-1,  1)),
                _       => (Vector2.down,  new Vector2(-1, -1), new Vector2( 1, -1)),
            };
            
            Vector2 center = _boxCollider.bounds.center;
            Vector2 extents = (Vector2)_boxCollider.bounds.extents + new Vector2(_boxCollider.edgeRadius, _boxCollider.edgeRadius);
            return (normal, center + extents * cornerStart, center + extents * cornerEnd);
        }
    }
}
