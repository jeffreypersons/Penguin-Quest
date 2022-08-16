using System;
using System.Diagnostics.Contracts;
using UnityEngine;
using PQ.Common.Collisions;


namespace PQ.Common
{
    // todo: add more character movement settings for things like speed and jump height
    public class CharacterController2D : MonoBehaviour
    {
        public enum Facing { Left = -1, Right = 1 }
        
        private Rigidbody2D _rigidbody;
        private CollisionChecker2D _collisionChecker;
        public CharacterController2DSettings Settings { get; set; }

        // todo: add speed and stuff here
        private Vector2 _netImpulseForce;
        private Facing _facing;

        private bool _wasJumpRequested;
        private bool _wasMoveRequested;
        private Facing _facingRequested;
        private static readonly Quaternion ROTATION_FACING_RIGHT = Quaternion.Euler(0,   0, 0);
        private static readonly Quaternion ROTATION_FACING_LEFT  = Quaternion.Euler(0, 180, 0);
        
        private bool _isCurrentlyContactingGround;
        public event Action<bool> GroundContactChanged;

        public void ChangeFacing(Facing facing) => _facingRequested = facing;
        public void Jump()                      => _wasJumpRequested = true;
        public void MoveForward()               => _wasMoveRequested = true;

        private void UpdateGroundContactInfo(bool force = false)
        {
            if (_isCurrentlyContactingGround != _collisionChecker.IsGrounded || force)
            {
                _isCurrentlyContactingGround = _collisionChecker.IsGrounded;
                GroundContactChanged?.Invoke(_isCurrentlyContactingGround);
            }
        }

        private void Reset()
        {
            _rigidbody.MoveRotation(ComputeOrientationForGivenUpAxis(_rigidbody, Vector2.up));
            UpdateGroundContactInfo(force: true);
        }


        void Awake()
        {
            _rigidbody        = gameObject.GetComponent<Rigidbody2D>();
            _collisionChecker = gameObject.GetComponent<CollisionChecker2D>();
            _facing           = GetFacing(_rigidbody);
            _wasJumpRequested = false;
            _facingRequested  = _facing;

            _netImpulseForce = Vector2.zero;
        }

        private void Start()
        {
            Reset();
        }

        void Update()
        {
            UpdateGroundContactInfo();
        }

        void FixedUpdate()
        {
            // todo: come up with a better way of dealing with these movement 'requests' that's not so adhoc
            if (_facing != _facingRequested)
            {
                TurnToFace(_facingRequested);
                _facingRequested = _facing;
            }

            if (_wasJumpRequested)
            {
                _netImpulseForce += ComputeJumpForce(Settings.JumpAngle, Settings.JumpStrength);
                _wasJumpRequested = false;
                _wasMoveRequested = false;
                return;
            }

            if (_wasMoveRequested)
            {
                MoveAlongForwardAxis();
                _wasMoveRequested = false;
            }

            if (!_collisionChecker.IsGrounded)
            {
                _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
                AlignWithGivenUpAxis(Vector2.up);
                return;
            }

            _rigidbody.constraints = RigidbodyConstraints2D.None;
            if (Settings.MaintainPerpendicularityToSurface)
            {
                // keep our character perpendicular to the surface at all times if option enabled
                AlignWithGivenUpAxis(_collisionChecker.SurfaceNormal);
            }
            else
            {
                // keep our character onFeet at all times if main perpendicularity option is not enabled
                AlignWithGivenUpAxis(Vector2.up);
                _rigidbody.constraints |= RigidbodyConstraints2D.FreezeRotation;
            }

            // if movement is within thresholds, freeze all axes to prevent jitter
            if (Settings.EnableAutomaticAxisLockingForSmallVelocities &&
                Mathf.Abs(_rigidbody.velocity.x)      < Settings.LinearVelocityThreshold &&
                Mathf.Abs(_rigidbody.velocity.y)      < Settings.LinearVelocityThreshold &&
                Mathf.Abs(_rigidbody.angularVelocity) < Settings.AngularVelocityThreshold)
            {
                // todo: this will have to be covered in the state machine instead since we need
                //       to account for when there is no input...
                _rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }

        void LateUpdate()
        {
            if (_netImpulseForce != Vector2.zero)
            {
                _rigidbody.constraints = RigidbodyConstraints2D.None;
                _rigidbody.AddForce(_netImpulseForce, ForceMode2D.Impulse);
                _netImpulseForce = Vector2.zero;
            }
        }


        private void AlignWithGivenUpAxis(Vector2 targetUpAxis)
        {
            float degreesUnaligned = Vector2.SignedAngle(targetUpAxis, transform.up);
            if (Mathf.Abs(degreesUnaligned) >= Settings.DegreesFromSurfaceNormalThreshold)
            {
                Quaternion current = transform.rotation;
                Quaternion target  = ComputeOrientationForGivenUpAxis(_rigidbody, targetUpAxis);
                _rigidbody.MoveRotation(Quaternion.Lerp(current, target, Settings.SurfaceAlignmentRotationalStrength));
            }
        }

        /* Move along forward axis at a given horizontal speed contribution and time delta. */
        private void MoveAlongForwardAxis()
        {
            Vector2 movementAxis      = _facing == Facing.Right ? Vector2.right : Vector2.left;
            Vector2 movementDirection = _rigidbody.transform.forward.normalized;
            float   movementTime      = Time.deltaTime;
            float   movementSpeed     = Settings.HorizontalMovementPeakSpeed;

            Vector2 currentPosition = _rigidbody.transform.position;
            Vector2 displacement = ComputeMovementStep(movementAxis, movementDirection, movementSpeed, movementTime);
            _rigidbody.MovePosition(currentPosition + displacement);
        }


        private void TurnToFace(Facing facing)
        {
            if (_facing == facing)
            {
                return;
            }

            _facing = facing;
            switch (_facing)
            {
                case Facing.Left:
                    transform.localRotation = ROTATION_FACING_LEFT;
                    break;
                case Facing.Right:
                    transform.localRotation = ROTATION_FACING_RIGHT;
                    break;
                default:
                    Debug.LogError($"Given value `{facing}` is not a valid Facing");
                    break;
            }
        }


        [Pure]
        private static Facing GetFacing(Rigidbody2D rigidbody)
        {
            return Mathf.Abs(rigidbody.transform.localEulerAngles.y) <= 90.0f ?
                Facing.Right :
                Facing.Left;
        }

        [Pure]
        private static Quaternion ComputeOrientationForGivenUpAxis(Rigidbody2D rigidbody, Vector3 targetUpAxis)
        {
            Vector3 currentForwardAxis = rigidbody.transform.forward;
            Vector3 targetLeftAxis = Vector3.Cross(currentForwardAxis, targetUpAxis);
            Vector3 targetForwardAxis = Vector3.Cross(targetUpAxis, targetLeftAxis);
            return Quaternion.LookRotation(targetForwardAxis, targetUpAxis);
        }


        [Pure]
        private static Vector2 ComputeMovementStep(Vector2 surfaceAxis, Vector2 direction, float speed, float time)
        {
            // todo: might want to try taking movement axis as parameter instead of facing
            // todo: might want to do the projected horizontal speed contribution calculations in the
            //       ground handler script rather than here, and just use the same value regardless (same for midair)

            // project the given velocity along the forward axis (1 if forward is completely aligned, 0 if perpendicular)
            float   scale    = Vector2.Dot(surfaceAxis, direction);
            Vector2 velocity = speed * direction;
            return (scale * velocity) * time;
        }

        [Pure]
        private static Vector2 ComputeJumpForce(float angle, float magnitude)
        {
            float jumpAngle = angle * Mathf.Deg2Rad;
            float jumpStrength = magnitude;

            // todo: change computations such that jump is relative to the character's forward
            // todo: compute angle and strength as a function of height and width
            return jumpStrength * new Vector2(Mathf.Cos(jumpAngle), Mathf.Sin(jumpAngle));
        }
    }
}
