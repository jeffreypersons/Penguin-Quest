using System;
using UnityEngine;
using PenguinQuest.Data;


namespace PenguinQuest.Controllers
{

    [RequireComponent(typeof(JumpUpHandler))]
    [RequireComponent(typeof(StandUpHandler))]
    [RequireComponent(typeof(LieDownHandler))]

    [RequireComponent(typeof(PenguinSkeleton))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(GroundChecker))]

    public class PenguinController : MonoBehaviour
    {
        private const float VELOCITY_THRESHOLD_DEFAULT         =    0.01f;
        private const float VELOCITY_THRESHOLD_MIN             =    0.01f;
        private const float VELOCITY_THRESHOLD_MAX             =   10.00f;
        private const float ANGLE_THRESHOLD_DEFAULT            =    0.01f;
        private const float ANGLE_THRESHOLD_MIN                =    0.01f;
        private const float ANGLE_THRESHOLD_MAX                =   10.00f;
        private const float SURFACE_ALIGNMENT_STRENGTH_DEFAULT =    0.10f;
        private const float SURFACE_ALIGNMENT_STRENGTH_MIN     =    0.00f;
        private const float SURFACE_ALIGNMENT_STRENGTH_MAX     =    1.00f;
        private const float SPEED_LIMIT_DEFAULT                =  500.00f;
        private const float SPEED_LIMIT_MIN                    =  100.00f;
        private const float SPEED_LIMIT_MAX                    = 1000.00f;
        private const bool  AUTOMATIC_AXIS_LOCKING_DEFAULT     =     true;

        private const float MASS_AMOUNT_DEFAULT       =   250.00f;
        private const float MASS_AMOUNT_MIN           =     0.00f;
        private const float MASS_AMOUNT_MAX           = 10000.00f;
        private const float MASS_CENTER_COORD_DEFAULT =     0.00f;
        private const float MASS_CENTER_COORD_MIN     =  -500.00f;
        private const float MASS_CENTER_COORD_MAX     =   500.00f;

        [Header("Movement Sensitivities")]
        [Tooltip("Sensitivity to small velocities (ie .10 units will be interpreted as zero [useful for jitter reduction])")]
        [Range(VELOCITY_THRESHOLD_MIN, VELOCITY_THRESHOLD_MAX)]
        [SerializeField] private float velocityThreshold = VELOCITY_THRESHOLD_DEFAULT;

        [Tooltip("Sensitivity to differences in alignment (ie .10 degree differences ignored [useful for jitter reduction])")]
        [Range(ANGLE_THRESHOLD_MIN, ANGLE_THRESHOLD_MAX)]
        [SerializeField] private float degreesFromSurfaceNormalThreshold = ANGLE_THRESHOLD_DEFAULT;

        [Tooltip("Rigidity of alignment with surface normal (ie 0 for max softness, 1 for no kinematic softness)")]
        [Range(SURFACE_ALIGNMENT_STRENGTH_MIN, SURFACE_ALIGNMENT_STRENGTH_MAX)]
        [SerializeField] private float surfaceAlignmentRotationalStrength = SURFACE_ALIGNMENT_STRENGTH_DEFAULT;

        [Tooltip("Maximum linear movement speed (ie clamp speed to 100)")]
        [Range(SPEED_LIMIT_MIN, SPEED_LIMIT_MAX)]
        [SerializeField] private float maxSpeed = SPEED_LIMIT_DEFAULT;

        [Tooltip("Enable automatic locking of movement axes when no movement or input [useful for jitter reduction]")]
        [SerializeField] private bool enableAutomaticAxisLockingWhenIdle = AUTOMATIC_AXIS_LOCKING_DEFAULT;


        [Header("Mass Settings")]
        [Tooltip("Constant (fixed) total mass for rigidbody")]
        [Range(MASS_AMOUNT_MIN, MASS_AMOUNT_MAX)]
        [SerializeField] private float mass = MASS_AMOUNT_DEFAULT;

        [Tooltip("Center of mass x component relative to skeletal root (ie smaller x means more prone to fall backwards)")]
        [Range(MASS_CENTER_COORD_MIN, MASS_CENTER_COORD_MAX)]
        [SerializeField] private float centerOfMassX = MASS_CENTER_COORD_DEFAULT;

        [Tooltip("Center of mass y component relative to skeletal root (ie smaller y means more resistant to falling over)")]
        [Range(MASS_CENTER_COORD_MIN, MASS_CENTER_COORD_MAX)]
        [SerializeField] private float centerOfMassY = MASS_CENTER_COORD_DEFAULT;

        private enum Posture { UPRIGHT, ONBELLY, BENTOVER }

        private Vector2       initialSpawnPosition;
        private GroundChecker groundChecker;

        private PenguinSkeleton penguinSkeleton;
        private Rigidbody2D     penguinRigidBody;
        private Animator        penguinAnimator;

        private Posture posture;


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
        private void ClampSpeed()
        {
            penguinRigidBody.velocity = Vector3.ClampMagnitude(penguinRigidBody.velocity, maxSpeed);
        }

        // update all animator parameters (except for triggers, as those should be set directly)
        private void UpdateAnimatorParameters()
        {
            // ideally we would use the enums directly, but enum is not a supported parameter type for animator
            penguinAnimator.SetBool("IsGrounded", groundChecker.WasDetected);
        }

        public void Reset()
        {
            UnlockAllAxes();
            enableAutomaticAxisLockingWhenIdle = true;

            groundChecker.Reset();
            penguinRigidBody.velocity = Vector2.zero;
            penguinRigidBody.position = initialSpawnPosition;
            penguinRigidBody.isKinematic  = false;
            penguinRigidBody.useAutoMass  = false;
            penguinRigidBody.mass         = mass;
            penguinRigidBody.centerOfMass = new Vector2(centerOfMassX, centerOfMassY);

            penguinAnimator.applyRootMotion = true;
            penguinAnimator.updateMode = AnimatorUpdateMode.Normal;

            penguinRigidBody.transform.localEulerAngles = Vector3.zero;

            UpdateAnimatorParameters();

            // align penguin with surface normal in a single update
            posture = Posture.UPRIGHT;
            groundChecker.CheckForGround(
                fromPoint: ComputeReferencePoint(),
                extraLineHeight: penguinSkeleton.ColliderTorso.bounds.extents.y
            );
            Vector2 targetUpAxis = groundChecker.WasDetected ? groundChecker.SurfaceNormalOfLastContact : Vector2.up;
            AlignPenguinWithUpAxis(targetUpAxis, forceInstantUpdate: true);
            groundChecker.Reset();
        }
        void Awake()
        {
            penguinSkeleton  = gameObject.GetComponent<PenguinSkeleton>();
            penguinAnimator  = gameObject.GetComponent<Animator>();
            penguinRigidBody = gameObject.GetComponent<Rigidbody2D>();
            groundChecker    = gameObject.GetComponent<GroundChecker>();

            initialSpawnPosition = penguinRigidBody.position;
            Reset();
        }
        
        #if UNITY_EDITOR
        void OnValidate()
        {
            if (!penguinRigidBody || !Application.IsPlaying(penguinRigidBody) || penguinRigidBody.useAutoMass)
            {
                return;
            }

            if (!Mathf.Approximately(centerOfMassX, penguinRigidBody.centerOfMass.x) ||
                !Mathf.Approximately(centerOfMassY, penguinRigidBody.centerOfMass.y))
            {
                penguinRigidBody.centerOfMass = new Vector2(centerOfMassX, centerOfMassY);
            }
            if (!Mathf.Approximately(mass, penguinRigidBody.mass))
            {
                penguinRigidBody.mass = mass;
            }
        }
        #endif

        private Vector2 ComputeReferencePoint()
        {
            Vector2 root = penguinAnimator.rootPosition;
            return posture == Posture.UPRIGHT ? root + new Vector2(0, groundChecker.MaxDistanceFromGround) : root;
        }

        void Update()
        {
            groundChecker.CheckForGround(
                fromPoint:       ComputeReferencePoint(),
                extraLineHeight: penguinSkeleton.ColliderTorso.bounds.extents.y
            );
            UpdateAnimatorParameters();
        }

        void FixedUpdate()
        {
            if (!groundChecker.WasDetected || posture == Posture.BENTOVER)
            {
                UnlockAllAxes();
                ClampSpeed();
                return;
            }

            float degreesUnaligned = groundChecker.Result.DegreesFromSurfaceNormal(transform.up);
            if (Mathf.Abs(degreesUnaligned) > degreesFromSurfaceNormalThreshold)
            {
                AlignPenguinWithUpAxis(groundChecker.Result.normal);
                return;
            }

            // if standing or lying on the ground idle and not already constrained freeze all axes to prevent jitter
            if (Mathf.Abs(degreesUnaligned) > degreesFromSurfaceNormalThreshold ||
                Mathf.Abs(penguinRigidBody.velocity.x) > velocityThreshold      ||
                Mathf.Abs(penguinRigidBody.velocity.y) > velocityThreshold)
            {
                UnlockAllAxes();
                ClampSpeed();
            }
            else if (enableAutomaticAxisLockingWhenIdle)
            {
                LockAllAxes();
            }
        }

        private void AlignPenguinWithUpAxis(Vector3 targetUpAxis, bool forceInstantUpdate=false)
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
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + (transform.rotation * new Vector2(centerOfMassX, centerOfMassY)), 0.50f);
        }
    }
}
