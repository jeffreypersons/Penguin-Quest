using System;
using UnityEngine;
using PenguinQuest.Data;


namespace PenguinQuest.Controllers
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(GroundChecker))]
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

        private Animator      penguinAnimator;
        private Rigidbody2D   penguinRigidBody;
        private GroundChecker groundChecker;

        [SerializeField] private CapsuleCollider2D torsoCollider = default;


        public void Reset()
        {
            groundChecker.Reset();

            // align penguin with surface normal in a single update
            groundChecker.CheckForGround(fromPoint: ComputeReferencePoint(), extraLineHeight: torsoCollider.bounds.extents.y);
            Vector2 targetUpAxis = groundChecker.WasDetected ? groundChecker.SurfaceNormalOfLastContact : Vector2.up;
            AlignPenguinWithUpAxis(targetUpAxis, forceInstantUpdate: true);

            groundChecker.Reset();
        }
        void Awake()
        {
            penguinAnimator  = gameObject.GetComponent<Animator>();
            penguinRigidBody = gameObject.GetComponent<Rigidbody2D>();
            groundChecker    = gameObject.GetComponent<GroundChecker>();
            
            Reset();
        }


        void Update()
        {
            penguinAnimator.SetBool("IsGrounded", groundChecker.WasDetected);
        }

        void FixedUpdate()
        {
            groundChecker.CheckForGround(fromPoint: ComputeReferencePoint(), extraLineHeight: torsoCollider.bounds.extents.y + 2.0f);
            if (groundChecker.Result == default)
            {
                return;
            }

            float degreesUnaligned = groundChecker.Result.DegreesFromSurfaceNormal(transform.up);
            if (Mathf.Abs(degreesUnaligned) > degreesFromSurfaceNormalThreshold)
            {
                AlignPenguinWithUpAxis(groundChecker.Result.normal);
                return;
            }
        }

        private Vector2 ComputeReferencePoint()
        {
            Vector2 root = penguinAnimator.rootPosition;
            return root + new Vector2(0, groundChecker.MaxDistanceFromGround);
        }

        private void AlignPenguinWithUpAxis(Vector3 targetUpAxis, bool forceInstantUpdate = false)
        {
            // we use the old forward direction of the penguin crossed with the axis we wish to align to, to get a perpendicular
            // vector pointing in or out of the screen (note unity uses the left hand system), with magnitude proportional to steepness.
            // then using our desired `up-axis` crossed with our `left` vector, we get a new forward direction of the penguin
            // that's parallel with the slope that our given up is normal to.
            Vector3 left = Vector3.Cross(transform.forward, targetUpAxis);
            Vector3 newForward = Vector3.Cross(targetUpAxis, left);

            Quaternion targetRotation = Quaternion.LookRotation(newForward, targetUpAxis);
            if (forceInstantUpdate)
            {
                penguinRigidBody.MoveRotation(targetRotation);
            }
            else
            {
                penguinRigidBody.MoveRotation(
                    Quaternion.Lerp(transform.rotation, targetRotation, surfaceAlignmentRotationalStrength));
            }
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
