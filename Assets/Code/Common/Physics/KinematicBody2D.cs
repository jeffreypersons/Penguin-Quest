using System;
using UnityEngine;
using PQ.Common.Extensions;


namespace PQ.Common.Physics
{
    /*
    Represents a physical body aligned with an AAB and driven by kinematic physics.

    Notes
    * Assumes always upright bounding box, with kinematic rigidbody
    * Corresponding game object is fixed in rotation to enforce alignment with global up
    * Caching is done only for cast results, position caching is intentionally left to any calling code
    */
    [AddComponentMenu("KinematicBody2D")]
    public sealed class KinematicBody2D : MonoBehaviour
    {
        [SerializeField] [Range(1, 100)] private int _preallocatedHitBufferSize = 16;
        [SerializeField] private bool _drawCastsInEditor = true;

        private bool  _flippedHorizontal;
        private bool  _flippedVertical;
        private float _skinWidth;
        private Rigidbody2D     _rigidBody;
        private Collider2D      _collider;
        private ContactFilter2D _castFilter;
        private RaycastHit2D[]  _castHits;

        public bool    FlippedHorizontal => _flippedHorizontal;
        public bool    FlippedVertical   => _flippedVertical;
        public Vector2 Position          => _rigidBody.position;
        public float   Depth             => _rigidBody.transform.position.z;
        public Bounds  Bounds            => _collider.bounds;
        public float   SkinWidth         => _skinWidth;
        public Vector2 Forward           => _rigidBody.transform.right.normalized;
        public Vector2 Up                => _rigidBody.transform.up.normalized;

        public Bounds BoundsOuter
        {
            get
            {
                var bounds = _collider.bounds;
                bounds.Expand(amount: 2f * _skinWidth);
                return _collider.bounds;
            }
        }

        public override string ToString() =>
            $"{GetType()}(" +
                $"Position:{Position}," +
                $"Depth:{Depth}," +
                $"Forward:{Forward}," +
                $"Up:{Up}," +
                $"SkinWidth:{SkinWidth}," +
                $"AAB: bounds(center:{Bounds.center}, extents:{Bounds.extents})," +
            $")";

        
        void Awake()
        {
            if (!gameObject.TryGetComponent<Rigidbody2D>(out var rigidBody))
            {
                throw new MissingComponentException($"Expected attached rigidbody2D - not found on {gameObject}");
            }
            if (!gameObject.TryGetComponent<Collider2D>(out var collider))
            {
                throw new MissingComponentException($"Expected attached collider2D - not found on {gameObject}");
            }

            _skinWidth  = 0f;
            _rigidBody  = rigidBody;
            _collider   = collider;
            _castFilter = new ContactFilter2D();
            _castHits   = new RaycastHit2D[_preallocatedHitBufferSize];
            _castFilter.useLayerMask = true;

            _rigidBody.isKinematic = true;
            _rigidBody.useFullKinematicContacts = true;
            _rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;

            Flip(horizontal: false, vertical: false);
        }

        public void Flip(bool horizontal, bool vertical)
        {
            _rigidBody.constraints &= ~RigidbodyConstraints2D.FreezeRotation;
            _rigidBody.transform.localEulerAngles = new Vector3(
                x: vertical?   180f : 0f,
                y: horizontal? 180f : 0f,
                z: 0f);
            _rigidBody.constraints |= RigidbodyConstraints2D.FreezeRotation;

            _flippedHorizontal = horizontal;
            _flippedVertical   = vertical;
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

        /* How much is our desired contact offset for collisions? */
        public void SetSkinWidth(float skinWidth)
        {
            if (Mathf.Approximately(_skinWidth, skinWidth))
            {
                return;
            }

            // todo: add depenetration algo here
            _skinWidth = skinWidth;
        }

        /* Resize preallocated raycast hit buffer to given amount (warning: causes allocations!). */
        public void ResizeHitBuffer(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentException($"Buffer size must be at least 1 - received {size} instead");
            }
            if (_castHits.Length != size)
            {
                _castHits = new RaycastHit2D[size];
            }
        }

        /* What's the delta between the AAB and the expanded AAB (with skin width) from center in given direction? */
        public Vector2 ComputeContactOffset(Vector2 direction)
        {
            if (Mathf.Approximately(_skinWidth, 0f))
            {
                return Vector2.zero;
            }

            Vector2 center    = Vector2.zero;
            Vector2 size      = new(_collider.bounds.size.x, _collider.bounds.size.y);
            Vector2 maxOffset = new(_skinWidth, _skinWidth);

            Ray    ray   = new(center, direction);
            Bounds inner = new(center, size);
            Bounds outer = new(center, size + maxOffset);
            inner.IntersectRay(ray, out float distanceToInner);
            outer.IntersectRay(ray, out float distanceToOuter);
            return (distanceToOuter - distanceToInner) * direction.normalized;
        }

        /* Check each side for _any_ colliders occupying the region between AAB and the outer perimeter defined by skin width. */
        public CollisionFlags2D CheckForOverlappingContacts(in LayerMask layerMask, float maxAngle)
        {
            //_castFilter.SetDepth(0, _skinWidth);
            _castFilter.SetLayerMask(layerMask);

            Transform transform = _rigidBody.transform;
            Vector2 right = transform.right.normalized;
            Vector2 up    = transform.up.normalized;
            Vector2 left  = -right;
            Vector2 down  = -up;

            CollisionFlags2D flags = CollisionFlags2D.None;
            if (_collider.Cast(right, _castFilter, _castHits, _skinWidth) >= 1)
            {
                flags |= CollisionFlags2D.Front;
            }
            if (_collider.Cast(up, _castFilter, _castHits, _skinWidth) >= 1)
            {
                flags |= CollisionFlags2D.Above;
            }
            if (_collider.Cast(left, _castFilter, _castHits, _skinWidth) >= 1)
            {
                flags |= CollisionFlags2D.Behind;
            }
            if (_collider.Cast(down, _castFilter, _castHits, _skinWidth) >= 1)
            {
                flags |= CollisionFlags2D.Below;
            }
            return flags;
        }
        
        /*
        Project AAB along delta, taking skin width into account, and return the closest distance/normal.
        
        WARNING: Hits are intended to be used right away, as any subsequent casts will change the result.
        */
        public bool CastAAB(Vector2 delta, in LayerMask layerMask, out ReadOnlySpan<RaycastHit2D> hits)
        {
            _castFilter.SetLayerMask(layerMask);

            int hitCount = _collider.Cast(delta, _castFilter, _castHits, delta.magnitude);
            hits = _castHits.AsSpan(0, hitCount);
            if (_drawCastsInEditor)
            {
                foreach (var hit in hits)
                {
                    DrawCastResultAsLineInEditor(hit, delta, _skinWidth);
                }
            }
            return hitCount >= 1;
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.IsPlaying(this))
            {
                return;
            }

            if (_preallocatedHitBufferSize != _castHits.Length)
            {
                _castHits = new RaycastHit2D[_preallocatedHitBufferSize];
            }
        }

        void OnDrawGizmos()
        {
            if (!Application.IsPlaying(this) || !enabled)
            {
                return;
            }

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

        private static void DrawCastResultAsLineInEditor(RaycastHit2D hit, Vector2 delta, float offset)
        {
            if (!hit)
            {
                // unfortunately we can't reliably find the origin of the cast
                // if there was no hit (as far as I'm aware), so nothing to draw
                return;
            }
            
            float duration  = Time.fixedDeltaTime;
            Vector2 direction = delta.normalized;
            Vector2 start     = hit.point - hit.distance * direction;
            Vector2 origin    = hit.point - (hit.distance - offset) * direction;
            Vector2 hitPoint  = hit.point;
            Vector2 end       = hit.point + (1f - hit.fraction) * (delta.magnitude + offset) * direction;

            Debug.DrawLine(start,    origin,   Color.magenta, duration);
            Debug.DrawLine(origin,   hitPoint, Color.green,   duration);
            Debug.DrawLine(hitPoint, end,      Color.red,     duration);
        }
        #endif
    }
}
