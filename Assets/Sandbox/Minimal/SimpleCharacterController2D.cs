using System;
using UnityEngine;


namespace PQ.TestScenes.Minimal
{
    public class SimpleCharacterController2D : ICharacterController2D
    {
        private Vector2 _position;
        private Bounds  _bounds;
        private Vector2 _forward;
        private Vector2 _up;
        private bool    _flipped;
        private bool    _isGrounded;
        private float   _contactOffset;

        private readonly ContactFilter2D   _contactFilter;
        private readonly RaycastHit2D[]    _horizontalHits;
        private readonly RaycastHit2D[]    _verticalHits;
        private readonly Rigidbody2D       _rigidBody;
        private readonly CapsuleCollider2D _capsule;

        public SimpleCharacterController2D(GameObject gameObject)
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

            
            _flipped        = false;
            _isGrounded     = false;
            _contactOffset  = 0f;
            _rigidBody      = rigidBody;
            _capsule        = capsule;
            _contactFilter  = new();
            _horizontalHits = new RaycastHit2D[rigidBody.attachedColliderCount];
            _verticalHits   = new RaycastHit2D[rigidBody.attachedColliderCount];

            _rigidBody.isKinematic = true;
            _contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
            _contactFilter.useLayerMask = true;

            SetXYOrientation(degreesAroundXAxis: 0f, degreesAroundYAxis: 0f);
            SyncPropertiesFromRigidBody();
        }

        Vector2 ICharacterController2D.Position   => _position;
        Bounds  ICharacterController2D.Bounds     => _capsule.bounds;
        Vector2 ICharacterController2D.Forward    => _forward;
        Vector2 ICharacterController2D.Up         => _up;
        bool    ICharacterController2D.IsGrounded => _isGrounded;
        bool    ICharacterController2D.Flipped    => _flipped;
        float   ICharacterController2D.ContactOffset { get => _contactOffset; set => _contactOffset = value; }

        public static bool DrawCastsInEditor { get; set; } = true;

        void ICharacterController2D.Flip()
        {
            _flipped = !_flipped;
            SetXYOrientation(degreesAroundXAxis: 0f, degreesAroundYAxis: _flipped ? 180f : 0f);
            SyncPropertiesFromRigidBody();
        }

        void ICharacterController2D.Move(Vector2 deltaPosition)
        {
            CastAndMove(_rigidBody, new(deltaPosition.x, 0), _contactFilter, _contactOffset, _horizontalHits);
            CastAndMove(_rigidBody, new(0, deltaPosition.y), _contactFilter, _contactOffset, _verticalHits);
            SyncPropertiesFromRigidBody();
        }
        
        private static void CastAndMove(Rigidbody2D body, Vector2 delta, in ContactFilter2D filter, float skinWidth,
            RaycastHit2D[] results)
        {
            if (delta == Vector2.zero)
            {
                return;
            }

            var distance  = delta.magnitude;
            var direction = delta / distance;
            int hitCount = body.Cast(delta, filter, results, distance + skinWidth);
            for (int i = 0; i < hitCount; i++)
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


        /*
        Apply bounciness/friction coefficients to hit position/normal, in proportion with the desired movement distance.


        In other words, collision parameters for an impact point, what's the adjusted position when taking into account
        desired delta, using a simple linear model (similar to what Unity's dynamic physics material provides).

        Note that bounciness is from 0 (no bounciness) to 1 (completely reflected),
        and that friction is from -1 ('boosts' velocity) to 0 (no resistance) to 1 (max resistance).
        */
        private static Vector2 ResolveCollision(Vector2 desiredPosition, Vector2 hitPosition, Vector2 hitNormal, float bounciness, float friction)
        {
            Vector2 delta  = desiredPosition - hitPosition;
            float remainingDistance = delta.magnitude;
            Vector2 reflected  = Vector2.Reflect(delta, hitNormal);
            Vector2 projection = Vector2.Dot(reflected, hitNormal) * hitNormal;
            Vector2 tangent    = reflected - projection;

            Vector2 perpendicularContribution = bounciness      * remainingDistance * projection.normalized;
            Vector2 tangentialContribution    = (1f - friction) * remainingDistance * tangent.normalized;
            return hitPosition + perpendicularContribution + tangentialContribution;
        }

        private void SetXYOrientation(float degreesAroundXAxis, float degreesAroundYAxis)
        {
            _rigidBody.transform.localEulerAngles =
                new Vector3(degreesAroundXAxis, degreesAroundYAxis, _rigidBody.transform.localEulerAngles.z);
        }
        
        private void SyncPropertiesFromRigidBody()
        {
            _position = _rigidBody.transform.position;
            _bounds   = _capsule.bounds;
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

            float duration = Time.fixedDeltaTime;
            var origin = hit.point - (distance * direction);
            var start  = origin    + (offset   * direction);
            var end    = hit.point;
            Debug.DrawLine(start,  end,    Color.red,     duration);
            Debug.DrawLine(start,  origin, Color.magenta, duration);
            Debug.DrawLine(origin, end,    Color.green,   duration);
        }
        #endif
    }
}
