using System;
using UnityEngine;
using PenguinQuest.Controllers.AlwaysOnComponents;
using PenguinQuest.Utils;


namespace PenguinQuest.Controllers.Handlers
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]

    [RequireComponent(typeof(GroundChecker))]
    [RequireComponent(typeof(PenguinBody))]
    public class GroundHandler : MonoBehaviour
    {
        [Header("Movement Sensitives (Tolerances for Jitter Reduction)")]
        [Tooltip("Should we automatically lock movement axes when velocities are within thresholds?")]
        [SerializeField] private bool enableAutomaticAxisLockingWhenIdle = true;

        [Range(1.00f, 20.00f)] [SerializeField] private float linearVelocityThreshold  = 5.00f;
        [Range(1.00f, 20.00f)] [SerializeField] private float angularVelocityThreshold = 5.00f;
        
        [Header("Movement Sensitives (Tolerances for Jitter Reduction")]
        [Tooltip("Should we automatically rotate the penguin to align with the surface normal?")]
        [SerializeField] private bool automaticallyAlignToSurfaceNormal = true;

        [Tooltip("Rigidity of alignment with surface normal (ie 0 for max softness, 1 for no kinematic softness)")]
        [Range(0.00f, 1.00f)] [SerializeField] private float surfaceAlignmentRotationalStrength = 0.10f;

        [Tooltip("At what degrees between up axis and surface normal is considered to be misaligned?")]
        [Range(1.00f, 20.00f)] [SerializeField] private float degreesFromSurfaceNormalThreshold = 0.01f;


        private Animator        penguinAnimator;
        private Rigidbody2D     penguinRigidBody;
        private GroundChecker   groundChecker;
        private PenguinBody penguinSkeleton;

        private void Reset()
        {
            Vector2 targetUpAxis = groundChecker.IsGrounded ? groundChecker.SurfaceNormal : Vector2.up;
            penguinRigidBody.MoveRotation(ComputeOrientationForGivenUpAxis(targetUpAxis));
        }

        void Awake()
        {
            penguinAnimator  = gameObject.GetComponent<Animator>();
            penguinRigidBody = gameObject.GetComponent<Rigidbody2D>();
            groundChecker    = gameObject.GetComponent<GroundChecker>();
            penguinSkeleton  = gameObject.GetComponent<PenguinBody>();
            Reset();
        }


        void Update()
        {
            penguinAnimator.SetBool("IsGrounded", groundChecker.IsGrounded);
        }


        // todo: move any non grounded logic to midair handler script
        void FixedUpdate()
        {
            if (!groundChecker.IsGrounded)
            {
                UnlockAllAxes();
                return;
            }

            Vector2 surfaceNormal = groundChecker.SurfaceNormal;
            float degreesUnaligned = Vector2.SignedAngle(surfaceNormal, transform.up);
            if (automaticallyAlignToSurfaceNormal && Mathf.Abs(degreesUnaligned) > degreesFromSurfaceNormalThreshold)
            {
                UnlockAllAxes();
                Quaternion current = transform.rotation;
                Quaternion target  = ComputeOrientationForGivenUpAxis(surfaceNormal);
                penguinRigidBody.MoveRotation(Quaternion.Lerp(current, target, surfaceAlignmentRotationalStrength));
            }
            // if standing or lying on the ground idle and not already constrained freeze all axes to prevent jitter
            else if (Mathf.Abs(penguinRigidBody.velocity.x)      > linearVelocityThreshold  ||
                     Mathf.Abs(penguinRigidBody.velocity.y)      > linearVelocityThreshold  ||
                     Mathf.Abs(penguinRigidBody.angularVelocity) > angularVelocityThreshold)
            {
                UnlockAllAxes();
            }
            else if (enableAutomaticAxisLockingWhenIdle)
            {
                LockAllAxes();
            }
        }


        private Quaternion ComputeOrientationForGivenUpAxis(Vector3 targetUpAxis)
        {
            Vector3 currentForwardAxis = transform.forward;
            Vector3 targetLeftAxis     = Vector3.Cross(currentForwardAxis, targetUpAxis);
            Vector3 targetForwardAxis  = Vector3.Cross(targetUpAxis,       targetLeftAxis);
            return Quaternion.LookRotation(targetForwardAxis, targetUpAxis);
        }

        private void LockAllAxes()
        {
            if (penguinRigidBody.constraints != RigidbodyConstraints2D.FreezeAll)
            {
                penguinRigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
        private void UnlockAllAxes()
        {
            if (penguinRigidBody.constraints != RigidbodyConstraints2D.None)
            {
                penguinRigidBody.constraints = RigidbodyConstraints2D.None;
            }
        }
    }
}
