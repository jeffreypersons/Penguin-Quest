using System;
using UnityEngine;


namespace PQ._Experimental.Physics.Move_007
{
    internal sealed class KinematicBody2D
    {
        public enum ContactSlotId
        {
            RightSide,
            TopRightCorner,
            TopSide,
            TopLeftCorner,
            LeftSide,
            BottomLeftCorner,
            BottomSide,
            BottomRightCorner,
        }

        public struct ContactSlot<SlotId> where SlotId : struct, Enum
        {
            public SlotId        Id           { get;      }
            public Vector2       Anchor       { get;      }
            public Vector2       Normal       { get;      }
            public Vector2       ScanOrigin   { get; set; }
            public float         ScanDistance { get; set; }
            public RaycastHit2D  ScanHit      { get; set; }

            public ContactSlot(SlotId id, Vector2 anchor)
            {
                Id           = id;
                Anchor       = anchor;
                Normal       = anchor.normalized;
                ScanOrigin   = default;
                ScanDistance = default;
                ScanHit      = default;
            }
        }

        private readonly Transform _transform;
        private readonly Rigidbody2D _rigidbody;
        private readonly BoxCollider2D _boxCollider;
        private ContactFilter2D  _contactFilter;

        private readonly RaycastHit2D[]   _singleHit;
        private readonly RaycastHit2D[]   _hitBuffer;
        private readonly Collider2D[]     _overlapBuffer;
        private readonly ContactPoint2D[] _contactBuffer;
        private readonly ContactSlot<ContactSlotId>[] _slots;

        private LayerMask _previousLayerMask;

        private const int DefaultBufferSize = 6;

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
        public Vector2 Extents   => _boxCollider.bounds.extents;
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
        
        /* Set the attached transform layer to ignore (typically prior to performing static ray casts). */
        private void DisableCollisionsWithAABB()
        {
            _previousLayerMask = _transform.gameObject.layer;
            _transform.gameObject.layer = Physics2D.IgnoreRaycastLayer;
        }

        /* After the object layer has been ignored (typically after performing static ray casts) - reassign it back to its original layer. */
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

            _rigidbody.simulated = true;
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody.useFullKinematicContacts = true;
            _rigidbody.constraints = RigidbodyConstraints2D.None;
            _slots = new ContactSlot<ContactSlotId>[]
            {
                new(ContactSlotId.RightSide,         new Vector2( 1,  0)),
                new(ContactSlotId.TopRightCorner,    new Vector2( 1,  1)),
                new(ContactSlotId.TopSide,           new Vector2( 0,  1)),
                new(ContactSlotId.TopLeftCorner,     new Vector2(-1,  1)),
                new(ContactSlotId.LeftSide,          new Vector2(-1,  0)),
                new(ContactSlotId.BottomLeftCorner,  new Vector2(-1, -1)),
                new(ContactSlotId.BottomSide,        new Vector2( 0, -1)),
                new(ContactSlotId.BottomRightCorner, new Vector2( 1, -1)),
            };
        }

        /* Check if the body is filtering out collisions with the given object or not. */
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
        
        /* Expand out bounds from the body center, ignoring attached AABB, outputting all overlapping colliders. Note order is not significant. */
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
            bool foundIntersection = _boxCollider.bounds.IntersectRay(new Ray(origin, direction), out distanceToEdge);

            // discard sign since distance is negative if starts within bounds (contrary to other ray methods)
            distanceToEdge = Mathf.Abs(distanceToEdge);
            #if UNITY_EDITOR
            DrawCastInEditorIfEnabled(origin, direction, Mathf.Infinity, foundIntersection ? distanceToEdge : null);
            #endif
            return foundIntersection;
        }

        /* Starting from the right going counter-clockwise, sweep-test given distance out from AABB. */
        public void FireAllContactSensors(float distance, out ReadOnlySpan<ContactSlot<ContactSlotId>> slots)
        {
            // todo: replace full sweep-test with BoxCast(origin: slot.position)
            // todo: create and use a higher level contact struct for above/below/behind/front
            // Note that for each slot, scale anchor [point on edge of a unit square] by extents
            // For example, for xy extents (1 / 4, 1) the scale is (0.50, 2)
            Vector2 center  = _boxCollider.bounds.center;
            Vector2 extents = _boxCollider.bounds.extents;
            for (int index = 0; index < _slots.Length; index++)
            {
                FireContactSensor(ref _slots[index], center, extents, distance);
            }
            slots = _slots.AsSpan();
        }

        /* Project a rectangle along delta, ignoring ALL attached colliders, and stopping at the first hit (if any). */
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

        /* Project a rectangle along delta, ignoring attached AABB, and stopping at the first hit (if any). */
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

        /* Project a point along the delta, ignoring attached AABB, and stopping at the first hit (if any). */
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

        /* Project a point along the delta, ignoring attached AABB, and stopping at the first hit (if any) to _given_ collider. */
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

        /* Project N points along the given direction, evenly spaced between start end points, ignoring attached AABB, and stopping at the first hit (if any) for each. */
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
        Through the corner closest to the given direction, project n points outwards, outputting each hit(s).

        SpreadExtent is defined as the half-length of the segment spanning tangent to the closest corner.
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
        Through the side in the given direction, project n points outwards, outputting each hit(s).

        Angles of (315,45]=>right (45,135]=>top (135,180]=>left (180,315]=>bottom
        Note that results length is always equal to count.
        */
        public bool CastRaysFromSide(Vector2 direction, float distance, int rayCount, out int hitCount, out ReadOnlySpan<RaycastHit2D> results)
        {
            (Vector2 normal, Vector2 start, Vector2 end) = FindClosestSide(direction);
            return CastRaysAlongSegment(start, end, normal, distance, rayCount, out hitCount, out results);
        }


        /*
        Map given the direction to a corner, returning its position.
        Note that the start is from bottom and left respectively. Relative to bottom-right-corner, angles map as:
        * [270,315)=>bottom-right [315,45)=>top-right-corner [45,135)=>top-left-corner [270,315)=>bottom-left-corner
        */
        private (Vector2 normal, Vector2 position) FindClosestCorner(Vector2 direction)
        {
            // map angle to side's normal and corner coordinates, checking from the lower right corner of the box
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

            Vector2 normalizedDiagonal = Vector2.one.normalized;
            Vector2 center       = _boxCollider.bounds.center;
            Vector2 extents      = _boxCollider.bounds.extents;
            Vector2 radialOffset = _boxCollider.edgeRadius * normalizedDiagonal;
            return (sign * normalizedDiagonal, center + sign * (extents + radialOffset));
        }

        /*
        Map given direction to a side, returning its normal and start end points.
        Note that the start is from bottom and left respectively. Relative to right world-axis, angles map as:
        * (315,45]=>right (45,135]=>top (135,180]=>left (180,315]=>bottom
        */
        private (Vector2 normal, Vector2 start, Vector2 end) FindClosestSide(Vector2 direction)
        {
            // map angle to side's normal and corner coordinates, checking from the lower right corner of the box
            float degrees = Vector2.SignedAngle(new Vector2(1, -1), direction);
            if (degrees <= 0)
            {
                degrees += 360f;
            }
            (Vector2 normal, Vector2 cornerStart, Vector2 cornerEnd) = degrees switch
            {
                <= 90f  => (Vector2.right, new Vector2( 1, -1), new Vector2( 1,  1)),
                <= 180f => (Vector2.up,    new Vector2(-1,  1), new Vector2( 1,  1)),
                <= 270f => (Vector2.left,  new Vector2(-1, -1), new Vector2(-1,  1)),
                _       => (Vector2.down,  new Vector2(-1, -1), new Vector2( 1, -1)),
            };
            
            Vector2 center  = _boxCollider.bounds.center;
            Vector2 extents = _boxCollider.bounds.extents;
            return (normal, center + extents * cornerStart, center + extents * cornerEnd);
        }

        private void FireContactSensor(ref ContactSlot<ContactSlotId> slot, Vector2 center, Vector2 extents, float distance)
        {
            // todo: replace full sweep-test with BoxCast(origin: slot.position)
            // Scale anchor [point on edge of a unit square] by extents.
            // For example, for xy extents (1 / 4, 1) the scale is (0.50, 2)
            Vector2 offset = Vector2.Scale(slot.Anchor, extents);

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
            DebugExtensions.DrawRayCast(slot.ScanOrigin, slot.Normal, slot.ScanDistance, slot.ScanHit, Time.fixedDeltaTime);
            #endif
        }
    }
}
