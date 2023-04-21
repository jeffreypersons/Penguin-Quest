using UnityEngine;


namespace PQ.TestScenes.Box
{
    public sealed class Mover
    {
        private const int PreallocatedHitBufferSize = 16;

        private bool  _flippedHorizontal;
        private bool  _flippedVertical;
        private float _skinWidth;
        private Rigidbody2D     _body;
        private BoxCollider2D   _aabb;
        private ContactFilter2D _castFilter;
        private RaycastHit2D[]  _castHits;

        public override string ToString() =>
            $"Mover{{" +
                $"Position:{Position}," +
                $"Depth:{Depth}," +
                $"Forward:{Forward}," +
                $"Up:{Up}," +
                $"SkinWidth:{SkinWidth}," +
                $"AAB: bounds(center:{Bounds.center}, extents:{Bounds.extents})," +
            $"}}";

        public Vector2 Position          => _body.position;
        public float   Depth             => _body.transform.position.z;
        public Bounds  Bounds            => _aabb.bounds;
        public float   SkinWidth         => _skinWidth;
        public Vector2 Forward           => _body.transform.right.normalized;
        public Vector2 Up                => _body.transform.up.normalized;


        public Mover(Transform transform)
        {
            if (!transform.TryGetComponent<Rigidbody2D>(out var rigidBody))
            {
                throw new MissingComponentException($"Expected attached rigidbody2D - not found on {transform}");
            }
            if (!transform.TryGetComponent<BoxCollider2D>(out var boxCollider))
            {
                throw new MissingComponentException($"Expected attached collider2D - not found on {transform}");
            }

            _skinWidth  = 0f;
            _body       = rigidBody;
            _aabb       = boxCollider;
            _castFilter = new ContactFilter2D();
            _castHits   = new RaycastHit2D[PreallocatedHitBufferSize];
            _castFilter.useLayerMask = true;

            _body.isKinematic = true;
            _body.simulated   = true;
            _body.useFullKinematicContacts = true;
            _body.constraints = RigidbodyConstraints2D.FreezeRotation;

            Flip(horizontal: false, vertical: false);
        }
        
        public void Flip(bool horizontal, bool vertical)
        {
            _body.constraints &= ~RigidbodyConstraints2D.FreezeRotation;
            _body.transform.localEulerAngles = new Vector3(
                x: vertical   ? 180f : 0f,
                y: horizontal ? 180f : 0f,
                z: 0f);
            _body.constraints |= RigidbodyConstraints2D.FreezeRotation;
        }

        public void Move(Vector2 deltaPosition)
        {
            // no op
        }
    }
}
