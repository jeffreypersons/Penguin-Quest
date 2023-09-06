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
        
        public struct ContactSlotInfo
        {
            public ContactSlotId Id           { get; init; }
            public float         Angle        { get; init; }
            public Vector2       Direction    { get; init; }
            public Vector2       LocalOrigin       { get; set;  }
            public float         ScanDistance { get; set;  }
            public RaycastHit2D  ClosestHit   { get; set;  }

            public override string ToString() =>
                $"{Id}: {(ClosestHit? ClosestHit.distance : "-")}";
        }
        
        private static EnumMap<ContactSlotId, ContactSlotInfo> ConstructContactMap()
        {
            EnumMap<ContactSlotId, ContactSlotInfo> contactSlots = new();
            for (int index = 0; index < 7; index++)
            {
                float degrees = 45 * index;
                float radians = Mathf.Deg2Rad * degrees;
                ContactSlotId slotId = (ContactSlotId)index;
                contactSlots.Add((ContactSlotId)index, new ContactSlotInfo
                {
                    Id           = slotId,
                    Angle        = degrees,
                    Direction    = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)),
                    LocalOrigin  = FindClosestLocalEdgePoint(degrees),
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
        private EnumMap<ContactSlotId, ContactSlotInfo> _contactSlots;

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

        public IReadOnlyList<ContactSlotInfo> GetContactInfo()
        {
            return _contactSlots.Values;
        }

        public void UpdateContactInfo(float contactOffset)
        {
            for (int index = 0; index < _contactSlots.Count; index++)
            {
                ContactSlotId slotId = (ContactSlotId)index;
                ContactSlotInfo info = _contactSlots[slotId];

                info.LocalOrigin = _rigidbody.position + (Vector2)_boxCollider.bounds.extents * info.Direction;
                info.ScanDistance = contactOffset;
                if (_rigidbody.Cast(info.Direction, _contactFilter, _hitBuffer, info.ScanDistance) > 0)
                {
                    info.ClosestHit = _hitBuffer[0];
                }
                else
                {
                    info.ClosestHit = default;
                }

                _contactSlots[slotId] = info;
            }
            
            #if UNITY_EDITOR
            for (int index = 0; index < _contactSlots.Count; index++)
            {
                var info = _contactSlots[(ContactSlotId)index];
                DebugExtensions.DrawRayCast(info.LocalOrigin, info.Direction, info.ScanDistance, info.ClosestHit, Time.fixedDeltaTime);
            }
            #endif
        }


        private static Vector2 FindClosestLocalEdgePoint(float angle)
        {
            #if UNITY_EDITOR
            if (angle is < 0 or > 360)
            {
                throw new ArgumentException($"Angle must be between 0 and 315, received index={angle}");
            }
            #endif
            return angle switch
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
        }
    }
}
