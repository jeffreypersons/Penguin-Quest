using System;
using UnityEngine;


namespace PQ._Experimental.Overlap_002
{
    public class Body : MonoBehaviour
    {
        private const int _preallocatedHitBufferSize = 16;

        private Rigidbody2D     _rigidbody;
        private BoxCollider2D   _boxCollider;
        private RaycastHit2D[]  _hitBuffer;
        private ContactFilter2D _contactFilter;

        public override string ToString() =>
            $"Mover{{" +
                $"Position:{Position}," +
                $"Depth:{Depth}," +
                $"Forward:{Forward}," +
                $"Up:{Up}," +
                $"AABB: bounds(center:{Bounds.center}, extents:{Bounds.extents})," +
            $"}}";

        public Vector2 Position => _rigidbody.position;
        public float   Depth    => _rigidbody.transform.position.z;
        public Bounds  Bounds   => _boxCollider.bounds;
        public Vector2 Forward  => _rigidbody.transform.right.normalized;
        public Vector2 Up       => _rigidbody.transform.up.normalized;
        public Vector2 Extents  => _boxCollider.bounds.extents;


        void Awake()
        {
            if (!transform.TryGetComponent<Rigidbody2D>(out var rigidbody))
            {
                throw new MissingComponentException($"Expected attached rigidbody2D - not found on {transform}");
            }
            if (!transform.TryGetComponent<BoxCollider2D>(out var boxCollider))
            {
                throw new MissingComponentException($"Expected attached collider2D - not found on {transform}");
            }

            _rigidbody   = rigidbody;
            _boxCollider = boxCollider;

            _contactFilter = new ContactFilter2D();
            _hitBuffer     = new RaycastHit2D[_preallocatedHitBufferSize];

            _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rigidbody.isKinematic = true;
            _rigidbody.simulated   = true;
            _rigidbody.useFullKinematicContacts = true;
            _rigidbody.constraints = RigidbodyConstraints2D.None;

            _contactFilter.useTriggers    = false;
            _contactFilter.useNormalAngle = false;
            _contactFilter.SetLayerMask(LayerMask.GetMask("Solids"));
        }

        /* Immediately move body to given point. */
        public void MoveTo(Vector2 position)
        {
            _rigidbody.position = position;
        }

        /* Immediately move body by given amount. */
        public void MoveBy(Vector2 delta)
        {
            _rigidbody.position += delta;
        }

        /* Interpolated move this amount. */
        public void MovePosition(Vector2 startPositionThisFrame, Vector2 targetPositionThisFrame)
        {
            _rigidbody.position = startPositionThisFrame;
            _rigidbody.MovePosition(targetPositionThisFrame);
        }


        public bool CastAABB(Vector2 direction, float distance, out RaycastHit2D hit)
        {
            hit = default;
            if (_boxCollider.Cast(direction, _contactFilter, _hitBuffer, distance) > 0)
            {
                hit = _hitBuffer[0];
            }
            return hit;
        }

        /*
        Compute vector representing overlap amount between body and given collider, if any.

        Note that uses separating axis theorem to determine overlap, so may require more invocations to resolve overlap
        for complex collider shapes (eg convex polygons).
        */
        public ColliderDistance2D ComputeMinimumSeparation(Collider2D collider)
        {
            if (collider == null)
            {
                throw new ArgumentNullException("Error state - invalid minimum separation between body and given collider");
            }

            ColliderDistance2D minimumSeparation = _boxCollider.Distance(collider);
            if (!minimumSeparation.isValid)
            {
                throw new InvalidOperationException("Error state - invalid minimum separation between body and given collider");
            }
            return minimumSeparation;
        }
    }
}
