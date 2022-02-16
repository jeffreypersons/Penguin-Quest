﻿using UnityEngine;
using PenguinQuest.Data;


namespace PenguinQuest.Controllers.AlwaysOnComponents
{
    [RequireComponent(typeof(PenguinEntity))]
    [RequireComponent(typeof(CollisionChecker))]
    public class CharacterController2D : MonoBehaviour
    {
        public CharacterController2DSettings Settings { get; set; }

        // todo: get rid of penguin entity and use dependency injection or something as this should be more generic
        private PenguinEntity penguinEntity;
        private CollisionChecker groundChecker;

        private void Reset()
        {
            penguinEntity.Rigidbody.MoveRotation(ComputeOrientationForGivenUpAxis(penguinEntity.Rigidbody, Vector2.up));
        }

        void Awake()
        {
            // todo: replace ground checker with a 2d character controller that reports surroundings,
            //       and will be a property of penguinEntity
            groundChecker = gameObject.GetComponent<CollisionChecker>();
            penguinEntity = gameObject.GetComponent<PenguinEntity>();
            Reset();
        }

        void Update()
        {
            penguinEntity.Animation.SetParamIsGrounded(groundChecker.IsGrounded);
        }


        void FixedUpdate()
        {
            if (!groundChecker.IsGrounded)
            {
                penguinEntity.Rigidbody.constraints = RigidbodyConstraints2D.None;
                return;
            }

            penguinEntity.Rigidbody.constraints = RigidbodyConstraints2D.None;
            if (Settings.MaintainPerpendicularityToSurface)
            {
                // keep our penguin perpendicular to the surface at all times if option enabled
                AlignPenguinWithGivenUpAxis(groundChecker.SurfaceNormal);
            }
            else
            {
                // keep our penguin upright at all times if main perpendicularity option is not enabled
                AlignPenguinWithGivenUpAxis(Vector2.up);
                penguinEntity.Rigidbody.constraints |= RigidbodyConstraints2D.FreezeRotation;
            }

            // if movement is within thresholds, freeze all axes to prevent jitter
            if (Settings.EnableAutomaticAxisLockingForSmallVelocities &&
                Mathf.Abs(penguinEntity.Rigidbody.velocity.x)      < Settings.LinearVelocityThreshold &&
                Mathf.Abs(penguinEntity.Rigidbody.velocity.y)      < Settings.LinearVelocityThreshold &&
                Mathf.Abs(penguinEntity.Rigidbody.angularVelocity) < Settings.AngularVelocityThreshold)
            {
                // todo: this will have to be covered in the state machine instead since we need
                //       to account for when there is no input...
                penguinEntity.Rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }

        private void AlignPenguinWithGivenUpAxis(Vector2 targetUpAxis)
        {
            float degreesUnaligned = Vector2.SignedAngle(targetUpAxis, transform.up);
            if (Mathf.Abs(degreesUnaligned) >= Settings.DegreesFromSurfaceNormalThreshold)
            {
                Quaternion current = transform.rotation;
                Quaternion target  = ComputeOrientationForGivenUpAxis(penguinEntity.Rigidbody, targetUpAxis);
                penguinEntity.Rigidbody.MoveRotation(Quaternion.Lerp(current, target, Settings.SurfaceAlignmentRotationalStrength));
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
