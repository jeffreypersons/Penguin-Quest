using System;
using UnityEngine;


namespace PenguinQuest.Controllers.AlwaysOnComponents
{
    [RequireComponent(typeof(PenguinEntity))]
    [RequireComponent(typeof(GroundChecker))]
    public class CharacterController2D : MonoBehaviour
    {
        [Header("Movement Sensitives (Tolerances for Jitter Reduction)")]
        [Tooltip("Should we automatically lock movement axes when velocities are within thresholds?")]
        [SerializeField] private bool enableAutomaticAxisLockingWhenIdle = true;

        [Range(0.01f, 20.00f)] [SerializeField] private float linearVelocityThreshold  = 5.00f;
        [Range(0.01f, 20.00f)] [SerializeField] private float angularVelocityThreshold = 5.00f;
        
        [Header("Movement Sensitives (Tolerances for Jitter Reduction)")]
        [Tooltip("Should we automatically rotate the penguin to align with the surface normal," +
                 "or if false, just not rotate at all?")]
        [SerializeField] private bool maintainPerpendicularityToSurface = true;

        [Tooltip("Rigidity of alignment with surface normal (ie 0 for max softness, 1 for no kinematic softness)")]
        [Range(0.00f, 1.00f)] [SerializeField] private float surfaceAlignmentRotationalStrength = 0.10f;

        [Tooltip("At what degrees between up axis and surface normal is considered to be misaligned?")]
        [Range(0.01f, 20.00f)] [SerializeField] private float degreesFromSurfaceNormalThreshold = 0.01f;

        [Tooltip("At what slope angle do we allow the penguin to walk up to?")]
        [Range(0.00f, 70.00f)] [SerializeField] private float maxGroundAngle = 45.00f;

        private PenguinEntity penguinEntity;
        private GroundChecker groundChecker;

        private void Reset()
        {
            penguinEntity.Rigidbody.MoveRotation(ComputeOrientationForGivenUpAxis(penguinEntity.Rigidbody, Vector2.up));
        }

        void Awake()
        {
            // todo: replace ground checker with a 2d character controller that reports surroundings,
            //       and will be a property of penguinEntity
            groundChecker = gameObject.GetComponent<GroundChecker>();
            penguinEntity = gameObject.GetComponent<PenguinEntity>();

            // todo: handle gravity for our custom slope handling
            //penguinEntity.Rigidbody.gravityScale = 0;
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
            penguinEntity.Animation.SetParamIsGrounded(groundChecker.IsGrounded);
        }


        void FixedUpdate()
        {
            if (!groundChecker.IsGrounded)
            {
                // todo: move any of this non grounded logic to a new midair handler script
                return;
            }

            penguinEntity.Rigidbody.constraints = RigidbodyConstraints2D.None;
            if (maintainPerpendicularityToSurface)
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
            if (enableAutomaticAxisLockingWhenIdle &&
                Mathf.Abs(penguinEntity.Rigidbody.velocity.x)      < linearVelocityThreshold &&
                Mathf.Abs(penguinEntity.Rigidbody.velocity.y)      < linearVelocityThreshold &&
                Mathf.Abs(penguinEntity.Rigidbody.angularVelocity) < angularVelocityThreshold)
            {
                // todo: this will have to be covered in the state machine instead since we need
                //       to account for when there is no input...
                penguinEntity.Rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }

        private void AlignPenguinWithGivenUpAxis(Vector2 targetUpAxis)
        {
            float degreesUnaligned = Vector2.SignedAngle(targetUpAxis, transform.up);
            if (Mathf.Abs(degreesUnaligned) >= degreesFromSurfaceNormalThreshold)
            {
                Quaternion current = transform.rotation;
                Quaternion target  = ComputeOrientationForGivenUpAxis(penguinEntity.Rigidbody, targetUpAxis);
                penguinEntity.Rigidbody.MoveRotation(Quaternion.Lerp(current, target, surfaceAlignmentRotationalStrength));
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
