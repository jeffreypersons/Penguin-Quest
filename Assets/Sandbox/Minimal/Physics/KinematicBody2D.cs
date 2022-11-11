using PQ.Common.Extensions;
using System;
using UnityEngine;


namespace PQ.TestScenes.Minimal.Physics
{
    /*
    Represents a physical body aligned with an AAB and driven by kinematic physics.
    Physics component 

    Assumes always upright bounding box, with kinematic rigidbody. Note that this means there is no rotation.

    Note there is no caching of positions (but there is for casts).
    */
    [AddComponentMenu("KinematicBody2DNew")]
    public sealed class KinematicBody2D : MonoBehaviour
    {        
        private float _skinWidth;
        private int   _lastHitCount;
        private Rigidbody2D     _rigidBody;
        private BoxCollider2D   _boxCollider;
        private ContactFilter2D _castFilter;
        private RaycastHit2D[]  _castHits;

        public Vector2 Position  => _rigidBody.position;
        public float   Depth     => _rigidBody.transform.position.z;
        public Bounds  Bounds    => _boxCollider.bounds;
        public float   SkinWidth => _skinWidth;
        public Vector2 Right     => _rigidBody.transform.right.normalized;
        public Vector2 Up        => _rigidBody.transform.up.normalized;

        public bool DrawCastsInEditor { get; set; } = true;

        public Bounds BoundsWithSkinWidth
        {
            get
            {
                var bounds = _boxCollider.bounds;
                bounds.Expand(amount: 2f * _skinWidth);
                return _boxCollider.bounds;
            }
        }

        public override string ToString() =>
            $"{GetType()}(" +
                $"Position:{Position}," +
                $"Depth:{Depth}," +
                $"Forward:{Right}," +
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
            if (!gameObject.TryGetComponent<BoxCollider2D>(out var boxCollider))
            {
                throw new MissingComponentException($"Expected attached collider2D - not found on {gameObject}");
            }

            _skinWidth    = 0f;
            _lastHitCount = 0;

            _rigidBody   = rigidBody;
            _boxCollider = boxCollider;
            _castFilter  = new ContactFilter2D();
            _castHits    = new RaycastHit2D[rigidBody.attachedColliderCount];
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


        /* Cast along delta, taking skin width and attached colliders into account, and return the closest distance/normal. */
        public bool TryFindClosestCollisionAlongDelta(Vector2 delta, LayerMask layerMask,
            out float hitDistance, out Vector2 hitNormal)
        {
            _castFilter.SetLayerMask(layerMask);
            _lastHitCount = _rigidBody.Cast(delta, _castFilter, _castHits, delta.magnitude + _skinWidth);

            var closestHitNormal   = Vector2.zero;
            var closestHitDistance = delta.magnitude;
            for (int i = 0; i < _lastHitCount; i++)
            {
                float adjustedDistance = _castHits[i].distance - _skinWidth;
                if (adjustedDistance > 0f && adjustedDistance < closestHitDistance)
                {
                    closestHitNormal   = _castHits[i].normal;
                    closestHitDistance = adjustedDistance;
                }
            }

            if (closestHitNormal == Vector2.zero)
            {
                hitDistance = default;
                hitNormal   = default;
                return false;
            }
            hitDistance = closestHitDistance;
            hitNormal   = closestHitNormal;
            
            #if UNITY_EDITOR
            if (DrawCastsInEditor)
                DrawLastSuccessfulCasts(delta, new Vector2(_skinWidth, _skinWidth));
            #endif
            return true;
        }
        

        #if UNITY_EDITOR
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
            Vector2 center    = box.center;
            Vector2 xAxis     = box.extents.x * Right;
            Vector2 yAxis     = box.extents.y * Up;
            Vector2 skinRatio = new(1f + (_skinWidth / box.extents.x), 1f + (_skinWidth / box.extents.y));

            GizmoExtensions.DrawRect(center, xAxis, yAxis, Color.gray);
            GizmoExtensions.DrawRect(center, skinRatio.x * xAxis, skinRatio.y * yAxis, Color.magenta);
            GizmoExtensions.DrawArrow(from: center, to: center + xAxis, color: Color.red);
            GizmoExtensions.DrawArrow(from: center, to: center + yAxis, color: Color.green);
        }

        private void DrawLastSuccessfulCasts(Vector2 castDelta, Vector2 castOffset)
        {
            float duration = Time.fixedDeltaTime;
            for (int i = 0; i < _lastHitCount; i++)
            {
                var origin = _castHits[i].point - castDelta;
                var start  = _castHits[i].point + castOffset;
                var end    = _castHits[i].point;

                Debug.DrawLine(start,  end,    Color.red,     duration);
                Debug.DrawLine(start,  origin, Color.magenta, duration);
                Debug.DrawLine(origin, end,    Color.green,   duration);
            }
        }
        #endif
    }
}
