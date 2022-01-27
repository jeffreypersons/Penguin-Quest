using System;
using UnityEngine;
using PenguinQuest.Controllers.AlwaysOnComponents;
using PenguinQuest.Utils;


namespace PenguinQuest.Controllers.Handlers
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(GroundChecker))]
    [RequireComponent(typeof(PenguinSkeleton))]
    public class GroundHandler : MonoBehaviour
    {
        [Header("Movement Sensitives (Tolerances for Jitter Reduction)")]
        [Tooltip("Should we automatically lock movement axes when velocities are within thresholds?")]
        [SerializeField] private bool enableAutomaticAxisLockingWhenIdle = true;

        [Range(1.00f, 20.00f)] [SerializeField] private float linearVelocityThreshold  = 5.00f;
        [Range(1.00f, 20.00f)] [SerializeField] private float angularVelocityThreshold = 5.00f;
        
        [Header("Movement Sensitives (Tolerances for Jitter Reduction")]
        [Tooltip("Should we automatically rotate the penguin to align with the surface normal," +
                 "or if false, just not rotate at all?")]
        [SerializeField] private bool maintainPerpendicularityToSurface = true;

        [Tooltip("Rigidity of alignment with surface normal (ie 0 for max softness, 1 for no kinematic softness)")]
        [Range(0.00f, 1.00f)] [SerializeField] private float surfaceAlignmentRotationalStrength = 0.10f;

        [Tooltip("At what degrees between up axis and surface normal is considered to be misaligned?")]
        [Range(1.00f, 20.00f)] [SerializeField] private float degreesFromSurfaceNormalThreshold = 0.01f;


        private Animator        penguinAnimator;
        private Rigidbody2D     penguinRigidbody;
        private Collider2D      penguinCollider;
        private GroundChecker   groundChecker;
        private PenguinSkeleton penguinSkeleton;

        private void Reset()
        {
            penguinRigidbody.MoveRotation(ComputeOrientationForGivenUpAxis(penguinRigidbody, Vector2.up));
        }

        void Awake()
        {
            penguinAnimator  = gameObject.GetComponent<Animator>();
            penguinRigidbody = gameObject.GetComponent<Rigidbody2D>();
            penguinCollider  = gameObject.GetComponent<Collider2D>();
            groundChecker    = gameObject.GetComponent<GroundChecker>();
            penguinSkeleton  = gameObject.GetComponent<PenguinSkeleton>();
            Reset();
        }

        // temporary public interface methods
        public bool MaintainPerpendicularityToSurface
        {
            get => maintainPerpendicularityToSurface;
            set => maintainPerpendicularityToSurface = value;
        }


        void Update()
        {
            penguinAnimator.SetBool("IsGrounded", groundChecker.IsGrounded);
        }


        void FixedUpdate()
        {
            if (!groundChecker.IsGrounded)
            {
                // todo: move any of this non grounded logic to a new midair handler script
                return;
            }

            penguinRigidbody.constraints = RigidbodyConstraints2D.None;
            if (maintainPerpendicularityToSurface)
            {
                // keep our penguin perpendicular to the surface at all times if option enabled
                AlignPenguinWithGivenUpAxis(groundChecker.SurfaceNormal);
            }
            else
            {
                // keep our penguin upright at all times if main perpendicularity option is not enabled
                AlignPenguinWithGivenUpAxis(Vector2.up);
                penguinRigidbody.constraints |= RigidbodyConstraints2D.FreezeRotation;
            }

            // if movement is within thresholds, freeze all axes to prevent jitter
            if (enableAutomaticAxisLockingWhenIdle &&
                Mathf.Abs(penguinRigidbody.velocity.x)      <= linearVelocityThreshold &&
                Mathf.Abs(penguinRigidbody.velocity.y)      <= linearVelocityThreshold &&
                Mathf.Abs(penguinRigidbody.angularVelocity) <= angularVelocityThreshold)
            {
                //penguinRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }

        private void AlignPenguinWithGivenUpAxis(Vector2 targetUpAxis)
        {
            float degreesUnaligned = Vector2.SignedAngle(targetUpAxis, transform.up);
            if (Mathf.Abs(degreesUnaligned) > degreesFromSurfaceNormalThreshold)
            {
                Quaternion current = transform.rotation;
                Quaternion target  = ComputeOrientationForGivenUpAxis(penguinRigidbody, targetUpAxis);
                penguinRigidbody.MoveRotation(Quaternion.Lerp(current, target, surfaceAlignmentRotationalStrength));
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
