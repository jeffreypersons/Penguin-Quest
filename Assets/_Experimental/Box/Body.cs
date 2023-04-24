using UnityEngine;


namespace PQ.TestScenes.Box
{
    public struct Hit
    {
        public Vector2    Point;     // position of intersection
        public Vector2    Normal;    // surface normal of hit
        public float      Distance;  // distance between edge of collider and intersection point
        public Collider2D Collider;  // collider hit
        public float      Buffer;    // offset distance
        public Vector2    Centroid;  // center of the collider that performed the cast
    }
    public class Body : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D   _rigidBody;
        [SerializeField] private BoxCollider2D _boxCollider;

        [SerializeField] private LayerMask _layerMask = default;
        [SerializeField] [Range(0, 1)]   private float _skinWidth = 0.025f;
        [SerializeField] [Range(1, 100)] private int _preallocatedHitBufferSize = 16;

        private bool _initialized;
        private ContactFilter2D _castFilter;
        private RaycastHit2D[]  _rawResults;
        private Hit[]           _hits;

        public override string ToString() =>
            $"Mover{{" +
                $"Position:{Position}," +
                $"Depth:{Depth}," +
                $"Forward:{Forward}," +
                $"Up:{Up}," +
                $"SkinWidth:{SkinWidth}," +
                $"AAB: bounds(center:{Bounds.center}, extents:{Bounds.extents})," +
            $"}}";

        public Vector2 Position  => _rigidBody.position;
        public float   Depth     => _rigidBody.transform.position.z;
        public Bounds  Bounds    => _boxCollider.bounds;
        public float   SkinWidth => _skinWidth;
        public Vector2 Forward   => _rigidBody.transform.right.normalized;
        public Vector2 Up        => _rigidBody.transform.up.normalized;

        public Bounds BoundsOuter
        {
            get
            {
                var bounds = _boxCollider.bounds;
                bounds.Expand(amount: 2f * _skinWidth);
                return _boxCollider.bounds;
            }
        }

        void Awake()
        {
            if (!transform.TryGetComponent<Rigidbody2D>(out var rigidBody))
            {
                throw new MissingComponentException($"Expected attached rigidbody2D - not found on {transform}");
            }
            if (!transform.TryGetComponent<BoxCollider2D>(out var boxCollider))
            {
                throw new MissingComponentException($"Expected attached collider2D - not found on {transform}");
            }

            _rigidBody   = rigidBody;
            _boxCollider = boxCollider;
            _castFilter  = new ContactFilter2D();
            _rawResults  = new RaycastHit2D[_preallocatedHitBufferSize];
            _hits        = new Hit[_preallocatedHitBufferSize];
            _castFilter.useLayerMask = true;
            _castFilter.SetLayerMask(_layerMask);

            _rigidBody.isKinematic = true;
            _rigidBody.simulated   = true;
            _rigidBody.useFullKinematicContacts = true;
            _rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;

            _initialized = true;
        }

        public void Flip(bool horizontal, bool vertical)
        {
            _rigidBody.constraints &= ~RigidbodyConstraints2D.FreezeRotation;
            _rigidBody.transform.localEulerAngles = new Vector3(
                x: vertical   ? 180f : 0f,
                y: horizontal ? 180f : 0f,
                z: 0f);
            _rigidBody.constraints |= RigidbodyConstraints2D.FreezeRotation;
        }

        /* Immediately move body by given amount. */
        public void MoveTo(Vector2 position)
        {
            _rigidBody.position = position;
        }

        /* Immediately move body by given amount. */
        public void MoveBy(Vector2 delta)
        {
            _rigidBody.position += delta;
        }

        // assumes nonzero delta
        public bool Cast_Closest(Vector2 delta, out Hit hit)
        {
            _castFilter.SetLayerMask(_layerMask);
            int hitCount = _boxCollider.Cast(delta, _castFilter, _rawResults, delta.magnitude, ignoreSiblingColliders: true);

            int closestHitIndex = 0;
            for (int i = 0; i < hitCount; i++)
            {
                if (_rawResults[i].distance < _rawResults[closestHitIndex].distance)
                {
                    closestHitIndex = i;
                }
            }

            RaycastHit2D result = _rawResults[closestHitIndex];
            hit = new Hit
            {
                Point    = result.point,
                Normal   = result.normal,
                Distance = result.distance,
                Collider = result.collider,
                Buffer   = _skinWidth,
                Centroid = result.centroid,
            };
            DrawCastResultAsLineInEditor(delta, hitCount > 0? hit.Point : null);
            return hitCount > 0;
        }


        /* What's the delta between the AABB and the expanded AABB (with skin width) from center in given direction? */
        public void ComputeOffset(Vector2 direction, out Vector2 deltaInner, out Vector2 deltaOuter)
        {
            Vector2 center    = Vector2.zero;
            Vector2 size      = new(_boxCollider.bounds.size.x, _boxCollider.bounds.size.y);
            Vector2 maxOffset = new(_skinWidth, _skinWidth);

            Ray    ray   = new(center, direction);
            Bounds inner = new(center, size);
            Bounds outer = new(center, size + maxOffset);
            inner.IntersectRay(ray, out float distanceToInner);
            outer.IntersectRay(ray, out float distanceToOuter);

            deltaInner = distanceToInner * direction.normalized;
            deltaOuter = distanceToOuter * direction.normalized;
        }
        
        
        private void DrawCastResultAsLineInEditor(Vector2 delta, Vector2? hitPoint)
        {
            float duration = Time.fixedDeltaTime;
            Vector2 centroid = Bounds.center;
            Debug.DrawLine(centroid, centroid + delta, Color.red, duration);
            if (hitPoint != null)
            {
                Debug.DrawLine(centroid, hitPoint.Value, Color.green, duration);
            }
        }

        void OnValidate()
        {
            if (!Application.IsPlaying(this) || !_initialized)
            {
                return;
            }

            if (_preallocatedHitBufferSize != _rawResults.Length)
            {
                _rawResults = new RaycastHit2D[_preallocatedHitBufferSize];
            }
        }

        void OnDrawGizmos()
        {
            // draw a bounding box that should be identical to the BoxCollider2D bounds in the editor window,
            // surrounded by an outer bounding box offset by our skin with, with a pair of arrows from the that
            // should be identical to the transform's axes in the editor window
            Bounds box = Bounds;
            Vector2 center    = new(box.center.x, box.center.y);
            Vector2 skinRatio = new(1f + (_skinWidth / box.extents.x), 1f + (_skinWidth / box.extents.y));
            Vector2 xAxis     = box.extents.x * Forward;
            Vector2 yAxis     = box.extents.y * Up;

            GizmoExtensions.DrawRect(center, xAxis, yAxis, Color.gray);
            GizmoExtensions.DrawRect(center, skinRatio.x * xAxis, skinRatio.y * yAxis, Color.magenta);
            GizmoExtensions.DrawArrow(from: center, to: center + xAxis, color: Color.red);
            GizmoExtensions.DrawArrow(from: center, to: center + yAxis, color: Color.green);
        }
    }
}
