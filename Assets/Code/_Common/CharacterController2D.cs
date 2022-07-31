using System;
using UnityEngine;
using PQ.Common.Collisions;


namespace PQ.Common
{
    public class CharacterController2D : MonoBehaviour
    {
        public Rigidbody2D Rigidbody { get; private set; }
        public CollisionChecker2D CollisionChecker { get; private set; }
        public CharacterController2DSettings Settings { get; set; }

        // todo: look into better consolidation of collision checker and event forwarding
        private bool _isInContactWithGround;
        public event Action<bool> OnGroundContactChanged;


        private void Reset()
        {
            Rigidbody.MoveRotation(ComputeOrientationForGivenUpAxis(Rigidbody, Vector2.up));
        }

        void Awake()
        {
            Reset();
        }

        void Update()
        {
            // todo: find a better way to deal with these sort of events..
            bool current = CollisionChecker.IsGrounded;
            if (_isInContactWithGround != current)
            {
                _isInContactWithGround = current;
                OnGroundContactChanged(current);
            }
        }

        void FixedUpdate()
        {
            if (!CollisionChecker.IsGrounded)
            {
                Rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
                AlignPenguinWithGivenUpAxis(Vector2.up);
                return;
            }

            Rigidbody.constraints = RigidbodyConstraints2D.None;
            if (Settings.MaintainPerpendicularityToSurface)
            {
                // keep our penguin perpendicular to the surface at all times if option enabled
                AlignPenguinWithGivenUpAxis(CollisionChecker.SurfaceNormal);
            }
            else
            {
                // keep our penguin onFeet at all times if main perpendicularity option is not enabled
                AlignPenguinWithGivenUpAxis(Vector2.up);
                Rigidbody.constraints |= RigidbodyConstraints2D.FreezeRotation;
            }

            // if movement is within thresholds, freeze all axes to prevent jitter
            if (Settings.EnableAutomaticAxisLockingForSmallVelocities &&
                Mathf.Abs(Rigidbody.velocity.x)      < Settings.LinearVelocityThreshold &&
                Mathf.Abs(Rigidbody.velocity.y)      < Settings.LinearVelocityThreshold &&
                Mathf.Abs(Rigidbody.angularVelocity) < Settings.AngularVelocityThreshold)
            {
                // todo: this will have to be covered in the state machine instead since we need
                //       to account for when there is no input...
                Rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }

        private void AlignPenguinWithGivenUpAxis(Vector2 targetUpAxis)
        {
            float degreesUnaligned = Vector2.SignedAngle(targetUpAxis, transform.up);
            if (Mathf.Abs(degreesUnaligned) >= Settings.DegreesFromSurfaceNormalThreshold)
            {
                Quaternion current = transform.rotation;
                Quaternion target  = ComputeOrientationForGivenUpAxis(Rigidbody, targetUpAxis);
                Rigidbody.MoveRotation(Quaternion.Lerp(current, target, Settings.SurfaceAlignmentRotationalStrength));
            }
        }


        private static Quaternion ComputeOrientationForGivenUpAxis(Rigidbody2D rigidbody, Vector3 targetUpAxis)
        {
            Vector3 currentForwardAxis = rigidbody.transform.forward;
            Vector3 targetLeftAxis    = Vector3.Cross(currentForwardAxis, targetUpAxis);
            Vector3 targetForwardAxis = Vector3.Cross(targetUpAxis,       targetLeftAxis);
            return Quaternion.LookRotation(targetForwardAxis, targetUpAxis);
        }
    }
}
