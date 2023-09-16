using System;
using System.Collections.Generic;
using UnityEngine;


namespace PQ._Experimental.Physics.Contact_006
{
    internal sealed class Body
    {
        public enum ContactSlotId
        {
            BottomRightCorner,
            RightSide,
            TopRightCorner,
            TopSide,
            TopLeftCorner,
            LeftSide,
            BottomLeftCorner,
            BottomSide,
        }
        
        public struct ContactSlot
        {
            public ContactSlotId Id           { get; init; }
            public float         Angle        { get; init; }
            public Vector2       Direction    { get; init; }
            public Vector2       LocalOrigin  { get; set;  }
            public float         ScanDistance { get; set;  }
            public RaycastHit2D  ClosestHit   { get; set;  }

            public override string ToString() => $"{Id}: {(ClosestHit? ClosestHit.distance : "-")}";
        }
        
        private static EnumMap<ContactSlotId, ContactSlot> ConstructContactMap()
        {
            EnumMap<ContactSlotId, ContactSlot> contactSlots = new();
            for (int index = 0; index < 7; index++)
            {
                float degrees = 45 * index;
                (Vector2 point, Vector2 direction) = FindClosestLocalEdgePoint(degrees);
                ContactSlotId slotId = (ContactSlotId)index;
                contactSlots.Add((ContactSlotId)index, new ContactSlot
                {
                    Id           = slotId,
                    Angle        = degrees,
                    Direction    = direction,
                    LocalOrigin  = point,
                    ScanDistance = default,
                    ClosestHit   = default
                });
            }
            return contactSlots;
        }


        private Transform       _transform;
        private Rigidbody2D     _rigidbody;
        private BoxCollider2D   _boxCollider;
        private ContactFilter2D _contactFilter;
        private RaycastHit2D[]  _hitBuffer;
        private EnumMap<ContactSlotId, ContactSlot> _contactSlots;

        private LayerMask _previousLayerMask;

        public Vector2 Position => _rigidbody.position;
        public Vector2 Extents  => _boxCollider.bounds.extents;
        public Vector2 Forward  => _transform.right.normalized;
        public Vector2 Up       => _transform.up.normalized;

        private const float DefaultEpsilon = 0.005f;
        private const int DefaultBufferSize = 1;
        private static readonly Vector2 NormalizedDiagonal = Vector2.one.normalized;

        public bool IsFlippedHorizontal => _rigidbody.transform.localEulerAngles.y >= 90f;
        public bool IsFlippedVertical   => _rigidbody.transform.localEulerAngles.x >= 90f;

        private void DisableCollisionsWithAABB()
        {
            _previousLayerMask = _transform.gameObject.layer;
            _transform.gameObject.layer = Physics2D.IgnoreRaycastLayer;
        }

        private void ReEnableCollisionsWithAABB()
        {
            _transform.gameObject.layer = _previousLayerMask;
        }


        public Body(Transform transform)
        {
            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform));
            }
            if (!transform.TryGetComponent(out Rigidbody2D rigidbody2D))
            {
                throw new MissingComponentException($"Expected attached {nameof(Rigidbody2D)} - not found on {nameof(transform)}");
            }
            if (!transform.TryGetComponent(out BoxCollider2D boxCollider))
            {
                throw new MissingComponentException($"Expected attached {nameof(BoxCollider2D)} - not found on {nameof(transform)}");
            }
            if (!ReferenceEquals(boxCollider.attachedRigidbody, rigidbody2D))
            {
                throw new MissingComponentException($"Expected attached {nameof(Rigidbody2D)} - not found on {nameof(boxCollider)}");
            }

            _transform     = rigidbody2D.transform;
            _rigidbody     = rigidbody2D;
            _boxCollider   = boxCollider;
            _contactFilter = new ContactFilter2D();
            _hitBuffer     = new RaycastHit2D[DefaultBufferSize];

            _contactFilter.useTriggers    = false;
            _contactFilter.useNormalAngle = false;
            _contactFilter.SetLayerMask(LayerMask.GetMask("Solids"));

            _rigidbody.simulated   = true;
            _rigidbody.isKinematic = true;
            _rigidbody.useFullKinematicContacts = true;
            _rigidbody.constraints = RigidbodyConstraints2D.None;

            _contactSlots = ConstructContactMap();
        }

        public IReadOnlyList<ContactSlot> GetContactInfo()
        {
            return _contactSlots.Values;
        }

        public void UpdateContactInfo(float contactOffset)
        {
            for (int index = 0; index < _contactSlots.Count; index++)
            {
                ContactSlotId slotId = (ContactSlotId)index;
                ContactSlot slot = _contactSlots[slotId];

                slot.ScanDistance = contactOffset;
                if (_rigidbody.Cast(slot.Direction, _contactFilter, _hitBuffer, slot.ScanDistance) > 0)
                {
                    slot.ClosestHit = _hitBuffer[0];
                }
                else
                {
                    slot.ClosestHit = default;
                }

                _contactSlots[slotId] = slot;
            }
            
            #if UNITY_EDITOR
            for (int index = 0; index < _contactSlots.Count; index++)
            {
                var slot = _contactSlots[(ContactSlotId)index];
                Vector2 origin = _rigidbody.position + _boxCollider.bounds.extents * slot.LocalOrigin;
                DebugExtensions.DrawRayCast(origin, slot.Direction, slot.ScanDistance, slot.ClosestHit, Time.fixedDeltaTime);
            }
            #endif
        }


        private static (Vector2 point, Vector2 direction) FindClosestLocalEdgePoint(float angle)
        {
            #if UNITY_EDITOR
            if (angle is < 0 or > 360)
            {
                throw new ArgumentException($"Angle must be between 0 and 360, received index={angle}");
            }
            #endif
            Vector2 point = angle switch
            {
                < 45f  => new Vector2( 1,  0),
                < 90f  => new Vector2( 1,  1),
                < 135f => new Vector2( 0,  1),
                < 180f => new Vector2(-1,  1),
                < 225f => new Vector2(-1,  0),
                < 270f => new Vector2(-1, -1),
                < 315f => new Vector2( 0, -1),
                < 360f => new Vector2( 1, -1),
                _ => Vector2.zero
            };
            Vector2 normal = point.x == point.y ? point * NormalizedDiagonal : point;

            return (point, normal);
        }
    }
}
