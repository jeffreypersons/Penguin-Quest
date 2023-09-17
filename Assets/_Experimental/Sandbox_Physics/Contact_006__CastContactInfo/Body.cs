using System;
using UnityEngine;


namespace PQ._Experimental.Physics.Contact_006
{
    internal sealed class Body
    {
        private const float DefaultEpsilon = 0.005f;
        private const int DefaultBufferSize = 1;
        private static readonly Vector2 NormalizedDiagonal = Vector2.one.normalized;

        private static readonly Vector2[] SlotAnchors = new Vector2[]
        {
            new(1,0), new(1,1), new(0,1), new(-1,1), new(-1,0), new(-1,-1), new(0,-1), new(1,-1),
        };

        public enum ContactSlotId
        {
            RightSide,
            TopRightCorner,
            TopSide,
            TopLeftCorner,
            LeftSide,
            BottomLeftCorner,
            BottomSide,
            BottomRightCorner
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

        private Transform       _transform;
        private Rigidbody2D     _rigidbody;
        private BoxCollider2D   _boxCollider;
        private ContactFilter2D _contactFilter;
        private RaycastHit2D[]  _hitBuffer;

        private ContactSlot[] _slots;
        private LayerMask _previousLayerMask;

        public Vector2 Position => _rigidbody.position;
        public Vector2 Extents  => _boxCollider.bounds.extents;
        public Vector2 Forward  => _transform.right.normalized;
        public Vector2 Up       => _transform.up.normalized;

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
