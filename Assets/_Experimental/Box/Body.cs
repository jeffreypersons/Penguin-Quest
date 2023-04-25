using System;
using UnityEngine;


namespace PQ.TestScenes.Box
{
    public class Body : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D   _rigidBody;
        [SerializeField] private BoxCollider2D _boxCollider;

        [SerializeField] private LayerMask _layerMask = default;
        [SerializeField] [Range(1, 100)] private int _preallocatedHitBufferSize = 16;

        #if UNITY_EDITOR        
        [SerializeField] private bool _drawCastsInEditor = true;
        #endif

        private bool _initialized;
        private ContactFilter2D _castFilter;
        private RaycastHit2D[]  _hitBuffer;

        public override string ToString() =>
            $"Mover{{" +
                $"Position:{Position}," +
                $"Depth:{Depth}," +
                $"Forward:{Forward}," +
                $"Up:{Up}," +
                $"AABB: bounds(center:{Bounds.center}, extents:{Bounds.extents})," +
            $"}}";

        public Vector2 Position  => _rigidBody.position;
        public float   Depth     => _rigidBody.transform.position.z;
        public Bounds  Bounds    => _boxCollider.bounds;
        public Vector2 Forward   => _rigidBody.transform.right.normalized;
        public Vector2 Up        => _rigidBody.transform.up.normalized;


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
            _hitBuffer   = new RaycastHit2D[_preallocatedHitBufferSize];
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
        
        /*
        Project AABB along delta, and return ALL hits (if any).
        
        WARNING: Hits are intended to be used right away, as any subsequent casts will change the result.
        */
        public bool CastAll(Vector2 delta, out ReadOnlySpan<RaycastHit2D> hits)
        {
            return CastAABB(delta, out hits);
        }

        /*
        Project AABB along delta, and return CLOSEST hit (if any).
        
        WARNING: Hits are intended to be used right away, as any subsequent casts will change the result.
        */
        public bool CastClosest(Vector2 delta, out RaycastHit2D hit)
        {
            if (!CastAABB(delta, out ReadOnlySpan<RaycastHit2D> hits))
            {
                hit = default;
                return false;
            }

            int closestHitIndex = 0;
            for (int i = 0; i < hits.Length; i++)
            {
                if (_hitBuffer[i].distance < _hitBuffer[closestHitIndex].distance)
                {
                    closestHitIndex = i;
                }
            }
            hit = _hitBuffer[closestHitIndex];
            return true;
        }
        

        private bool CastAABB(Vector2 delta, out ReadOnlySpan<RaycastHit2D> hits)
        {
            _castFilter.SetLayerMask(_layerMask);
            int hitCount = _boxCollider.Cast(delta, _castFilter, _hitBuffer, delta.magnitude, ignoreSiblingColliders: true);
            hits = _hitBuffer.AsSpan(0, hitCount);
            
            #if UNITY_EDITOR
            if (_drawCastsInEditor)
            {
                float duration = Time.fixedDeltaTime;
                foreach (RaycastHit2D hit in hits)
                {
                    Debug.Log($"hit.centroid={hit.centroid}, rb.position={_rigidBody.position}, collider.bounds.center={_boxCollider.bounds.center}, transform.position={transform.position}");
                    Debug.DrawLine(hit.centroid, hit.centroid + delta, Color.red, duration);
                    Debug.DrawLine(hit.centroid, hit.point, Color.green, duration);
                }
            }
            #endif
            return !hits.IsEmpty;
        }


        void OnValidate()
        {
            if (!Application.IsPlaying(this) || !_initialized)
            {
                return;
            }

            if (_preallocatedHitBufferSize != _hitBuffer.Length)
            {
                _hitBuffer = new RaycastHit2D[_preallocatedHitBufferSize];
            }
        }

        void OnDrawGizmos()
        {
            // draw a bounding box that should be identical to the BoxCollider2D bounds in the editor window,
            // surrounded by an outer bounding box offset by our skin with, with a pair of arrows from the that
            // should be identical to the transform's axes in the editor window
            Bounds box = Bounds;
            Vector2 center = new(box.center.x, box.center.y);
            Vector2 xAxis  = box.extents.x * Forward;
            Vector2 yAxis  = box.extents.y * Up;

            GizmoExtensions.DrawArrow(from: center, to: center + xAxis, color: Color.blue);
            GizmoExtensions.DrawArrow(from: center, to: center + yAxis, color: Color.cyan);
        }
    }
}
