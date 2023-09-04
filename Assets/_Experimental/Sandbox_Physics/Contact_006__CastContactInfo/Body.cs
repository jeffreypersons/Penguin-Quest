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
            public ContactSlotId Id           { get; set; }
            public float         Angle        { get; set; }
            public Vector2       Direction    { get; set; }
            public Vector2       Origin       { get; set; }
            public float         ScanDistance { get; set; }
            public RaycastHit2D  ClosestHit   { get; set; }

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
                contactSlots.Add(slotId, new ContactSlotInfo
                {
                    Id           = slotId,
                    Angle        = degrees,
                    Direction    = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)),
                    Origin       = default,
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
            Transform transform = _rigidbody.transform;
            Vector2 right = transform.right.normalized;
            Vector2 up    = transform.up.normalized;
            Vector2 left  = -right;
            Vector2 down  = -up;

            for (int index = 0; index < _contactSlots.Count; index++)
            {
                ContactSlotId slotId = (ContactSlotId)index;
                ContactSlotInfo info = _contactSlots[slotId];

                info.Origin = (Vector2)_boxCollider.bounds.extents * info.Direction;
                info.ScanDistance = contactOffset;
                if (_rigidbody.Cast(info.Direction, _contactFilter, _hitBuffer, contactOffset) > 0)
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
            Bounds bounds = _boxCollider.bounds;
            Vector2 center    = new Vector2(bounds.center.x, bounds.center.y);
            Vector2 skinRatio = new Vector2(1f + (contactOffset / bounds.extents.x), 1f + (contactOffset / bounds.extents.y));
            Vector2 xAxis     = bounds.extents.x * right;
            Vector2 yAxis     = bounds.extents.y * up;

            float duration = Time.fixedDeltaTime;
            Debug.DrawLine(center + xAxis, center + skinRatio * xAxis, Color.magenta, duration);
            Debug.DrawLine(center - xAxis, center - skinRatio * xAxis, Color.magenta, duration);
            Debug.DrawLine(center + yAxis, center + skinRatio * yAxis, Color.magenta, duration);
            Debug.DrawLine(center - yAxis, center - skinRatio * yAxis, Color.magenta, duration);
            #endif
        }

        public bool CheckDirection(Vector2 direction, float distance, out RaycastHit2D hit)
        {
            // note that there is no need to disable colliders as that is accounted for by collider instance
            if (_rigidbody.Cast(direction, _contactFilter, _hitBuffer, distance) > 0)
            {
                hit = _hitBuffer[0];
            }
            else
            {
                hit = default;
            }
            return hit;
        }
    }
}
