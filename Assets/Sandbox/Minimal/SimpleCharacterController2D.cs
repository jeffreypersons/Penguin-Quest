using System;
using UnityEngine;


namespace PQ.TestScenes.Minimal
{
    public class SimpleCharacterController2D : ICharacterController2D
    {
        public struct Settings
        {
            public readonly int   hitBufferSize;
            public readonly float contactOffset;
            public Settings(int hitBufferSize, float contactOffset)
            {
                this.hitBufferSize = hitBufferSize;
                this.contactOffset = contactOffset;
            }
        }

        private Vector2  _position;
        private Vector2  _forward;
        private Vector2  _up;
        private bool     _flipped;
        private bool     _isGrounded;
        private Settings _settings;

        private readonly ContactFilter2D   _contactFilter;
        private readonly RaycastHit2D[]    _hitBuffer;
        private readonly Rigidbody2D       _rigidBody;
        private readonly CapsuleCollider2D _capsule;

        public SimpleCharacterController2D(GameObject gameObject, Settings settings)
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException($"Expected non-null game object");
            }
            if (!gameObject.TryGetComponent<Rigidbody2D>(out var rigidBody))
            {
                throw new MissingComponentException($"Expected attached rigidbody2D - not found on {gameObject}");
            }
            if (!gameObject.TryGetComponent<CapsuleCollider2D>(out var capsule))
            {
                throw new MissingComponentException($"Expected attached collider2D - not found on {gameObject}");
            }

            _contactFilter = new();
            _hitBuffer     = new RaycastHit2D[settings.hitBufferSize];
            _rigidBody     = rigidBody;
            _capsule       = capsule;
            _settings      = settings;

            _rigidBody.isKinematic = true;
            _contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
            _contactFilter.useLayerMask = true;

            _flipped = false;
            _isGrounded = false;
            SetXYOrientation(degreesAroundXAxis: 0f, degreesAroundYAxis: 0f);
            SyncPropertiesFromRigidBody();
        }

        Vector2 ICharacterController2D.Position   => _position;
        Vector2 ICharacterController2D.Forward    => _forward;
        Vector2 ICharacterController2D.Up         => _up;
        bool    ICharacterController2D.IsGrounded => _isGrounded;
        bool    ICharacterController2D.Flipped    => _flipped;
        public static bool DrawCastsInEditor { get; set; } = true;

        void ICharacterController2D.Flip()
        {
            _flipped = !_flipped;
            SetXYOrientation(degreesAroundXAxis: 0f, degreesAroundYAxis: _flipped ? 180f : 0f);
            SyncPropertiesFromRigidBody();
        }

        void ICharacterController2D.Move(Vector2 deltaPosition)
        {
            Vector2 horizontal = new(deltaPosition.x, 0);
            Vector2 vertical   = new(0, deltaPosition.y);
            CastAndMove(_rigidBody, horizontal, _contactFilter, _hitBuffer, _settings.contactOffset);
            CastAndMove(_rigidBody, vertical,   _contactFilter, _hitBuffer, _settings.contactOffset);

            SyncPropertiesFromRigidBody();
        }
        
        private static void CastAndMove(Rigidbody2D body, Vector2 delta, in ContactFilter2D filter, RaycastHit2D[] results, float skinWidth)
        {
            if (delta == Vector2.zero)
            {
                return;
            }

            var distance  = delta.magnitude;
            var direction = delta / distance;
            Debug.Log($"dis={distance},dir={direction}");
            int hits = body.Cast(delta, filter, results, distance + skinWidth);
            for (int i = 0; i < hits; i++)
            {
                #if UNITY_EDITOR
                if (DrawCastsInEditor)
                    DrawCastResultAsLineInEditor(results[i], skinWidth, direction, distance);
                #endif
                float adjustedDistance = results[i].distance - skinWidth;
                distance = adjustedDistance < distance ? adjustedDistance : distance;
            }
            body.position += direction.normalized * distance;
        }

        private void SetXYOrientation(float degreesAroundXAxis, float degreesAroundYAxis)
        {
            _rigidBody.transform.localEulerAngles =
                new Vector3(degreesAroundXAxis, degreesAroundYAxis, _rigidBody.transform.localEulerAngles.z);
        }
        
        private void SyncPropertiesFromRigidBody()
        {
            _position = _rigidBody.transform.position;
            _forward  = _rigidBody.transform.right.normalized;
            _up       = _rigidBody.transform.up.normalized;
        }
        
        #if UNITY_EDITOR
        private static void DrawCastResultAsLineInEditor(RaycastHit2D hit, float offset, Vector2 direction, float distance)
        {
            if (!hit)
            {
                // unfortunately we can't reliably find the origin of the cast
                // if there was no hit (as far as I'm aware), so nothing to draw
                return;
            }

            Vector2 origin = hit.point - (distance * direction);
            Vector2 start  = origin    + (offset   * direction);
            Vector2 end    = hit.point;

            float duration = Time.fixedDeltaTime;
            Debug.DrawLine(start,  end,       Color.red,     duration);
            Debug.DrawLine(start,  origin,    Color.magenta, duration);
            Debug.DrawLine(origin, hit.point, Color.green,   duration);
        }
        #endif
    }
}
