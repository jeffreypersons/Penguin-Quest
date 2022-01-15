using System;
using UnityEngine;
using PenguinQuest.Controllers.AlwaysOnComponents;


namespace PenguinQuest.Controllers.Handlers
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]

    [RequireComponent(typeof(GroundChecker))]
    [RequireComponent(typeof(PenguinSkeleton))]
    public class GroundHandler : MonoBehaviour
    {
        private const float ANGLE_THRESHOLD_DEFAULT            =  0.01f;
        private const float ANGLE_THRESHOLD_MIN                =  0.01f;
        private const float ANGLE_THRESHOLD_MAX                = 10.00f;
        private const float SURFACE_ALIGNMENT_STRENGTH_DEFAULT =  0.10f;
        private const float SURFACE_ALIGNMENT_STRENGTH_MIN     =  0.00f;
        private const float SURFACE_ALIGNMENT_STRENGTH_MAX     =  1.00f;

        [Tooltip("Sensitivity to differences in alignment (ie .10 degree differences ignored [useful for jitter reduction])")]
        [Range(ANGLE_THRESHOLD_MIN, ANGLE_THRESHOLD_MAX)]
        [SerializeField] private float degreesFromSurfaceNormalThreshold = ANGLE_THRESHOLD_DEFAULT;

        [Tooltip("Rigidity of alignment with surface normal (ie 0 for max softness, 1 for no kinematic softness)")]
        [Range(SURFACE_ALIGNMENT_STRENGTH_MIN, SURFACE_ALIGNMENT_STRENGTH_MAX)]
        [SerializeField] private float surfaceAlignmentRotationalStrength = SURFACE_ALIGNMENT_STRENGTH_DEFAULT;

        private Animator        penguinAnimator;
        private Rigidbody2D     penguinRigidBody;
        private GroundChecker   groundChecker;
        private PenguinSkeleton penguinSkeleton;

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
            penguinSkeleton  = gameObject.GetComponent<PenguinSkeleton>();
            Reset();
        }


        void Update()
        {
            penguinAnimator.SetBool("IsGrounded", groundChecker.IsGrounded);
        }

        void FixedUpdate()
        {
            if (!groundChecker.IsGrounded)
            {
                return;
            }

            // todo: add actual calculate here...
            float degreesUnaligned = 0.0f;
            if (Mathf.Abs(degreesUnaligned) > degreesFromSurfaceNormalThreshold)
            {
                Quaternion current = transform.rotation;
                Quaternion target  = ComputeOrientationForGivenUpAxis(groundChecker.SurfaceNormal);
                penguinRigidBody.MoveRotation(Quaternion.Lerp(current, target, surfaceAlignmentRotationalStrength));
            }
        }

        private Quaternion ComputeOrientationForGivenUpAxis(Vector3 targetUpAxis)
        {
            Vector3 currentForwardAxis = transform.forward;
            Vector3 targetLeftAxis     = Vector3.Cross(currentForwardAxis, targetUpAxis);
            Vector3 targetForwardAxis  = Vector3.Cross(targetUpAxis,       targetLeftAxis);
            return Quaternion.LookRotation(targetForwardAxis, targetUpAxis);
        }
        
        void OnDrawGizmos()
        {
            if (!penguinAnimator)
            {
                return;
            }

            Gizmos.color = Color.black;
            Gizmos.DrawSphere(penguinAnimator.rootPosition, 1.00f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.TransformPoint(penguinRigidBody.centerOfMass), 0.50f);
        }
    }
}
