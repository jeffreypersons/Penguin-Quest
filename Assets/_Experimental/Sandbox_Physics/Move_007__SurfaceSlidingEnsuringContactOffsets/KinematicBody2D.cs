using System;
using UnityEngine;


namespace PQ._Experimental.Physics.Move_007
{
    [Flags]
    public enum ContactSlotId
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

    public struct ContactSlot
    {
        public ContactSlotId Id           { get; init; }
        public Vector2       Normal       { get; init; }
        public Vector2       ScanOrigin   { get; set;  }
        public float         ScanDistance { get; set;  }
        public RaycastHit2D  ScanHit      { get; set;  }

        public override string ToString() => $"{Id}: {(ScanHit ? ScanHit.distance : "-")}";
    }

    internal sealed class KinematicBody2D
    {
        private Transform        _transform;
        private Rigidbody2D      _rigidbody;
        private BoxCollider2D    _boxCollider;
        private ContactFilter2D  _contactFilter;

        private RaycastHit2D[]   _singleHit;
        private RaycastHit2D[]   _hitBuffer;
        private Collider2D[]     _overlapBuffer;
        private ContactPoint2D[] _contactBuffer;
        private ContactSlot[]    _slots;

        private LayerMask _previousLayerMask;

        private const float DefaultEpsilon = 0.005f;
        private const int DefaultBufferSize = 6;
        private static readonly Vector2 NormalizedDiagonal = Vector2.one.normalized;

        private static readonly Vector2[] SlotAnchors = new Vector2[]
        {
            new(1,0), new(1,1), new(0,1), new(-1,1), new(-1,0), new(-1,-1), new(0,-1), new(1,-1),
        };

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

        #if UNITY_EDITOR
        public bool DrawCastsInEditor { get; set; }

        private void DrawCastInEditorIfEnabled(Vector2 origin, Vector2 direction, float distance, float? hitDistance, bool force=false)
        {
            if (DrawCastsInEditor || force)
            {
                Debug.DrawLine(origin, origin + distance * direction, Color.red, 1f);
                if (hitDistance.HasValue)
                {
                    Debug.DrawLine(origin, origin + hitDistance.Value * direction, Color.green, 1f);
                }
            }
        }
        #endif
        
        /* Set attached transform layer to ignore (typically prior to performing static ray casts). */
        private void DisableCollisionsWithAABB()
        {
            _previousLayerMask = _transform.gameObject.layer;
            _transform.gameObject.layer = Physics2D.IgnoreRaycastLayer;
        }

        /* After object layer has been ignored (typically after performing static ray casts) - reassign it back to it's original layer. */
        private void ReEnableCollisionsWithAABB()
        {
            _transform.gameObject.layer = _previousLayerMask;
        }

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
            _singleHit     = new RaycastHit2D  [1];
            _hitBuffer     = new RaycastHit2D  [DefaultBufferSize];
            _overlapBuffer = new Collider2D    [DefaultBufferSize];
            _contactBuffer = new ContactPoint2D[DefaultBufferSize];

            _contactFilter.useTriggers    = false;
            _contactFilter.useNormalAngle = false;
            _contactFilter.SetLayerMask(LayerMask.GetMask("Solids"));

            _rigidbody.simulated   = true;
            _rigidbody.isKinematic = true;
            _rigidbody.useFullKinematicContacts = true;
            _rigidbody.constraints = RigidbodyConstraints2D.None;

            bool isDiagonal = false;
            _slots = new ContactSlot[SlotAnchors.Length];
            for (int index = 0; index < SlotAnchors.Length; index++)
            {
                _slots[index] = new ContactSlot
                {
                    Id           = (ContactSlotId)index,
                    Normal       = isDiagonal ? SlotAnchors[index] * NormalizedDiagonal : SlotAnchors[index],
                    ScanOrigin   = default,
                    ScanDistance = default,
                    ScanHit      = default,
                };
                isDiagonal = !isDiagonal;
            }
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
        
        /* Check if given world point lies on or within our AABB. */
        public bool IsPointInBounds(Vector2 point)
        {
            Bounds bounds = _boxCollider.bounds;
            bounds.Expand(_boxCollider.edgeRadius);
            return bounds.Contains(point);
        }


        /* Expand out bounds from body center, ignoring attached AABB, outputting all overlapping colliders. Note order is not significant. */
        public bool CheckForOverlappingColliders(Vector2 extents, out ReadOnlySpan<Collider2D> colliders)
        {
            DisableCollisionsWithAABB();
            int colliderCount = Physics2D.OverlapBox(_boxCollider.bounds.center, 2f * extents, 0f, _contactFilter, _overlapBuffer);
            colliders = _overlapBuffer.AsSpan(0, colliderCount);
            ReEnableCollisionsWithAABB();
            return !colliders.IsEmpty;
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
            DrawCastInEditorIfEnabled(origin, direction, Mathf.Infinity, foundIntersection ? distanceToEdge : null);
            #endif
            return foundIntersection;
        }

        /* Starting from right going counter-clockwise, sweeptest given distance out from AABB. */
        public void FireAllContactSensors(float contactOffset, out ReadOnlySpan<ContactSlot> slots)
        {
            Vector2 center = _boxCollider.bounds.center;
            Vector2 extents = (Vector2)_boxCollider.bounds.extents;
            for (int index = 0; index < _slots.Length; index++)
            {
                Scan(ref _slots[index], center, extents, contactOffset);
            }
            slots = _slots.AsSpan();
        }

        /* Project a rectangle along delta, ignoring ALL attached colliders, and stopping at first hit (if any). */
        public bool CastAABB(Vector2 direction, float distance, out RaycastHit2D hit)
        {
            // note that there is no need to disable colliders as that is accounted for by collider instance
            if (_boxCollider.Cast(direction, _contactFilter, _singleHit, distance) > 0)
            {
                hit = _singleHit[0];
            }
            else
            {
                hit = default;
            }
            return hit;
        }

        /* Project a rectangle along delta, ignoring attached AABB, and stopping at first hit (if any). */
        public bool CastBox(Vector2 origin, float angle, Vector2 extents, Vector2 direction, float distance, out RaycastHit2D hit)
        {
            DisableCollisionsWithAABB();
            if (Physics2D.BoxCast(origin, 2f * extents, angle, direction, _contactFilter, _singleHit, distance) > 0)
            {
                hit = _singleHit[0];
            }
            else
            {
                hit = default;
            }
            ReEnableCollisionsWithAABB();
            #if UNITY_EDITOR
            DrawCastInEditorIfEnabled(origin, direction, distance, hit? hit.distance : null);
            #endif
            return hit;
        }

        /* Project a circle along delta, ignoring attached AABB, and stopping at first hit (if any). */
        public bool CastCircle(Vector2 origin, float radius, Vector2 direction, float distance, out RaycastHit2D hit)
        {
            DisableCollisionsWithAABB();
            hit = default;
            if (Physics2D.CircleCast(origin, radius, direction, _contactFilter, _singleHit, distance) > 0)
            {
                hit = _singleHit[0];
            }
            ReEnableCollisionsWithAABB();
            #if UNITY_EDITOR
            DrawCastInEditorIfEnabled(origin, direction, distance, hit? hit.distance : null);
            #endif
            return hit;
        }

        /* Project a point along delta, ignoring attached AABB, and stopping at first hit (if any). */
        public bool CastRay(Vector2 origin, Vector2 direction, float distance, out RaycastHit2D hit)
        {
            DisableCollisionsWithAABB();
            if (Physics2D.Raycast(origin, direction, _contactFilter, _singleHit, distance) > 0)
            {
                hit = _singleHit[0];
            }
            else
            {
                hit = default;
            }
            ReEnableCollisionsWithAABB();
            #if UNITY_EDITOR
            DrawCastInEditorIfEnabled(origin, direction, distance, hit? hit.distance : null);
            #endif
            return hit;
        }

        /* Project a point along delta, ignoring attached AABB, and stopping at first hit (if any) to _given_ collider. */
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
            ReEnableCollisionsWithAABB();
            #if UNITY_EDITOR
            DrawCastInEditorIfEnabled(origin, direction, distance, hit? hit.distance : null);
            #endif
            return hit;
        }

        /* Project N points along given direction, evenly spaced between between start end points, ignoring attached AABB, and stopping at first hit (if any) for each. */
        public bool CastRaysAlongSegment(Vector2 start, Vector2 end, Vector2 direction, float distance, int rayCount, out int hitCount, out ReadOnlySpan<RaycastHit2D> results)
        {
            #if UNITY_EDITOR
            int minRayCount = Mathf.Min(3, _hitBuffer.Length);
            int maxRayCount = Mathf.Max(3, _hitBuffer.Length);
            if (rayCount < minRayCount || rayCount > maxRayCount)
            {
                throw new ArgumentException($"Ray count must be in range=[{minRayCount},{maxRayCount}], received={rayCount}");
            }
            #endif

            DisableCollisionsWithAABB();
            int totalHits = 0;
            Vector2 delta = (end - start) / (rayCount-1);
            for (int rayIndex = 0; rayIndex < rayCount; rayIndex++)
            {
                Vector2 origin = start + (rayIndex * delta);
                if (Physics2D.Raycast(origin, direction, _contactFilter, _singleHit, distance) > 0)
                {
                    totalHits++;
                    _hitBuffer[rayIndex] = _singleHit[0];
                }
                else
                {
                    _hitBuffer[rayIndex] = default;
                }
                #if UNITY_EDITOR
                DrawCastInEditorIfEnabled(origin, direction, distance, _hitBuffer[rayIndex] ? _hitBuffer[rayIndex].distance : null);
                #endif
            }
            results = _hitBuffer.AsSpan(0, rayCount);
            hitCount = totalHits;
            ReEnableCollisionsWithAABB();
            return hitCount > 0;
        }

        /*
        Through the corner closest to given direction, project n points outwards, outputting each hit(s).

        SpreadExtent is defined as the half length of the segment spanning tangent to the closest corner.
        Angles of (315,45]=>right (45,135]=>top (135,180]=>left (180,315]=>bottom
        Note that results length is always equal to count.
        */
        public bool CastRaysFromCorner(float spreadExtent, Vector2 direction, float distance, int rayCount, out int hitCount, out ReadOnlySpan<RaycastHit2D> results)
        {
            (Vector2 normal, Vector2 position) = FindClosestCorner(direction);
            Vector2 tangent = Vector2.Perpendicular(normal);
            Vector2 start   = position + spreadExtent * tangent;
            Vector2 end     = position - spreadExtent * tangent;
            return CastRaysAlongSegment(start, end, normal, distance, rayCount, out hitCount, out results);
        }

        /*
        Through the side in given direction, project n points outwards, outputting each hit(s).

        Angles of (315,45]=>right (45,135]=>top (135,180]=>left (180,315]=>bottom
        Note that results length is always equal to count.
        */
        public bool CastRaysFromSide(Vector2 direction, float distance, int rayCount, out int hitCount, out ReadOnlySpan<RaycastHit2D> results)
        {
            (Vector2 normal, Vector2 start, Vector2 end) = FindClosestSide(direction);
            return CastRaysAlongSegment(start, end, normal, distance, rayCount, out hitCount, out results);
        }


        /*
        Map given direction to a corner, returning it's position.
        Note that start is from bottom and left respectively. Relative to bottom-right-corner, angles map as:
        * [270,315)=>bottom-right [315,45)=>top-right-corner [45,135)=>top-left-corner [270,315)=>bottom-left-corner
        */
        private (Vector2 normal, Vector2 position) FindClosestCorner(Vector2 direction)
        {
            // map angle to side's normal and corner coordinates, checking from lower right corner of the box
            float degrees = Vector2.SignedAngle(new Vector2(0, -1), direction);
            if (degrees <= 0)
            {
                degrees += 360f;
            }

            Vector2 sign = degrees switch
            {
                <= 90f  => new Vector2( 1, -1),
                <= 180f => new Vector2( 1,  1),
                <= 270f => new Vector2(-1,  1),
                _       => new Vector2(-1, -1),
            };

            Vector2 center       = _boxCollider.bounds.center;
            Vector2 extents      = _boxCollider.bounds.extents;
            Vector2 radialOffset = _boxCollider.edgeRadius * NormalizedDiagonal;
            return (sign * NormalizedDiagonal, center + sign * (extents + radialOffset));
        }

        /*
        Map given direction to a side, returning it's normal and start end points.
        Note that start is from bottom and left respectively. Relative to right world-axis, angles map as:
        * (315,45]=>right (45,135]=>top (135,180]=>left (180,315]=>bottom
        */
        private (Vector2 normal, Vector2 start, Vector2 end) FindClosestSide(Vector2 direction)
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
            
            Vector2 center  = _boxCollider.bounds.center;
            Vector2 extents = (Vector2)_boxCollider.bounds.extents + new Vector2(_boxCollider.edgeRadius, _boxCollider.edgeRadius);
            return (normal, center + extents * cornerStart, center + extents * cornerEnd);
        }

        private void Scan(ref ContactSlot slot, Vector2 center, Vector2 extents, float distance)
        {
            // Scale anchor [point on edge of a unit square] by extents.
            // For example, for xy extents (1 / 4, 1) the scale is (0.50, 2)
            Vector2 offset = Vector2.Scale(SlotAnchors[(int)slot.Id], extents);

            slot.ScanOrigin = center + offset;
            slot.ScanDistance = (new Vector2(distance, distance) * slot.Normal).magnitude;
            if (_rigidbody.Cast(slot.Normal, _contactFilter, _hitBuffer, slot.ScanDistance) > 0)
            {
                slot.ScanHit = _hitBuffer[0];
            }
            else
            {
                slot.ScanHit = default;
            }

            #if UNITY_EDITOR
            Debug.Log($"{slot.Id} : from={slot.ScanOrigin} to={slot.ScanOrigin + slot.ScanDistance * slot.Normal}");
            DebugExtensions.DrawRayCast(slot.ScanOrigin, slot.Normal, slot.ScanDistance, slot.ScanHit, Time.fixedDeltaTime);
            #endif
        }
    }
}
