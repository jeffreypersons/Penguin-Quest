using System;
using UnityEngine;
using PQ.Common.Collisions;


namespace PQ.Common
{
    // todo: add more character movement settings for things like speed and jump height
    public class CharacterController2D : MonoBehaviour
    {
        public enum Facing { Left = -1, Right = 1 }
        
        private Rigidbody2D _rigidbody;
        private CollisionChecker _collisionChecker;
        public CharacterController2DSettings Settings { get; set; }

        private Facing _facing;
        private Facing _facingRequested;
        private static readonly Quaternion ROTATION_FACING_RIGHT = Quaternion.Euler(0,   0, 0);
        private static readonly Quaternion ROTATION_FACING_LEFT  = Quaternion.Euler(0, 180, 0);
        
        public void ChangeFacing(Facing facing)
        {
            _facingRequested = facing;
        }


        private bool _isCurrentlyContactingGround;
        public event Action<bool> GroundContactChanged;

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
            _collisionChecker = gameObject.GetComponent<CollisionChecker>();
            _facing = GetFacing(_rigidbody);
            _facingRequested = _facing;
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
            if (_facing != _facingRequested)
            {
                TurnToFace(_facingRequested);
                _facingRequested = _facing;
            }

            if (!_collisionChecker.IsGrounded)
            {
                _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
                AlignPenguinWithGivenUpAxis(Vector2.up);
                return;
            }

            _rigidbody.constraints = RigidbodyConstraints2D.None;
            if (Settings.MaintainPerpendicularityToSurface)
            {
                // keep our character perpendicular to the surface at all times if option enabled
                AlignPenguinWithGivenUpAxis(_collisionChecker.SurfaceNormal);
            }
            else
            {
                // keep our character onFeet at all times if main perpendicularity option is not enabled
                AlignPenguinWithGivenUpAxis(Vector2.up);
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

        private void AlignPenguinWithGivenUpAxis(Vector2 targetUpAxis)
        {
            float degreesUnaligned = Vector2.SignedAngle(targetUpAxis, transform.up);
            if (Mathf.Abs(degreesUnaligned) >= Settings.DegreesFromSurfaceNormalThreshold)
            {
                Quaternion current = transform.rotation;
                Quaternion target  = ComputeOrientationForGivenUpAxis(_rigidbody, targetUpAxis);
                _rigidbody.MoveRotation(Quaternion.Lerp(current, target, Settings.SurfaceAlignmentRotationalStrength));
            }
        }


        private static Quaternion ComputeOrientationForGivenUpAxis(Rigidbody2D rigidbody, Vector3 targetUpAxis)
        {
            Vector3 currentForwardAxis = rigidbody.transform.forward;
            Vector3 targetLeftAxis    = Vector3.Cross(currentForwardAxis, targetUpAxis);
            Vector3 targetForwardAxis = Vector3.Cross(targetUpAxis,       targetLeftAxis);
            return Quaternion.LookRotation(targetForwardAxis, targetUpAxis);
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


        private static Facing GetFacing(Rigidbody2D rigidbody)
        {
            return Mathf.Abs(rigidbody.transform.localEulerAngles.y) <= 90.0f ?
                Facing.Right :
                Facing.Left;
        }

        /* Move along forward axis at a given horizontal speed contribution and time delta. */
        private static void MoveHorizontal(Rigidbody2D rigidbody, Facing facing, float speed, float time)
        {
            // todo: might want to try taking movement axis as parameter instead of facing
            // todo: might want to do the projected horizontal speed contribution calculations in the
            //       ground handler script rather than here, and just use the same value regardless (same for midair)
            Vector2 movementAxis    = facing == Facing.Right? Vector2.right : Vector2.left;
            Vector2 currentPosition = rigidbody.transform.position;
            Vector2 currentForward  = rigidbody.transform.forward.normalized;

            // project the given velocity along the forward axis (1 if forward is completely horizontal, 0 if vertical)
            Vector2 displacement = Vector2.Dot(movementAxis, currentForward) * (speed * time) * movementAxis;
            rigidbody.MovePosition(currentPosition + displacement);
        }
    }
}
