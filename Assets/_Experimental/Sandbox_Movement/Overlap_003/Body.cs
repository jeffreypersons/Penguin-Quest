using UnityEngine;


namespace PQ._Experimental.Overlap_003
{
    public class Body : MonoBehaviour
    {
        private const int _preallocatedHitBufferSize = 16;

        private Rigidbody2D      _rigidbody;
        private CircleCollider2D _circleCollider;
        private RaycastHit2D[]   _hitBuffer;
        private ContactFilter2D  _contactFilter;

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
        public Bounds  Bounds   => _circleCollider.bounds;
        public Vector2 Forward  => _rigidbody.transform.right.normalized;
        public Vector2 Up       => _rigidbody.transform.up.normalized;
        public Vector2 Extents  => _circleCollider.bounds.extents;


        void Awake()
        {
            if (!transform.TryGetComponent<Rigidbody2D>(out var rigidbody))
            {
                throw new MissingComponentException($"Expected attached Rigidbody2D - not found on {transform}");
            }
            if (!transform.TryGetComponent<CircleCollider2D>(out var circleCollider))
            {
                throw new MissingComponentException($"Expected attached CircleCollider2D - not found on {transform}");
            }

            _rigidbody      = rigidbody;
            _circleCollider = circleCollider;

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


        public bool CastCircle(Vector2 direction, float distance, out RaycastHit2D hit, bool includeAlreadyOverlappingColliders)
        {
            hit = default;

            bool queriesStartInColliders = Physics2D.queriesStartInColliders;
            Physics2D.queriesStartInColliders = includeAlreadyOverlappingColliders;

            if (_circleCollider.Cast(direction, _contactFilter, _hitBuffer, distance) > 0)
            {
                hit = _hitBuffer[0];
            }

            Physics2D.queriesStartInColliders = queriesStartInColliders;
            return hit;
        }
        
        /*
        Project a point along given direction until specific given collider is hit.

        Note that in 3D we have collider.RayCast for this, but in 2D we have no built in way of checking a
        specific collider (collider2D.RayCast confusingly casts _from_ it instead of _at_ it).
        */
        public bool CastRayAt(Collider2D collider, Vector2 origin, Vector2 direction, float distance, out RaycastHit2D hit, bool includeAlreadyOverlappingColliders)
        {
            int layer = collider.gameObject.layer;
            bool queriesStartInColliders = Physics2D.queriesStartInColliders;
            LayerMask includeLayers = _contactFilter.layerMask;
            collider.gameObject.layer = Physics2D.IgnoreRaycastLayer;
            Physics2D.queriesStartInColliders = includeAlreadyOverlappingColliders;
            _contactFilter.SetLayerMask(~collider.gameObject.layer);

            int hitCount = Physics2D.Raycast(origin, direction, _contactFilter, _hitBuffer, distance);

            collider.gameObject.layer = layer;
            _contactFilter.SetLayerMask(includeLayers);
            Physics2D.queriesStartInColliders = queriesStartInColliders;

            hit = default;
            for (int i = 0; i < hitCount; i++)
            {
                if (_hitBuffer[i].collider == collider)
                {
                    hit = _hitBuffer[i];
                    break;
                }
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
                return default;
            }
            ColliderDistance2D minimumSeparation = _circleCollider.Distance(collider);
            return minimumSeparation.isValid ? minimumSeparation : default;
        }
    }
}
