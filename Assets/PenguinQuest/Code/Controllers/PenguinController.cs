using System;
using UnityEngine;


namespace PenguinQuest.Controllers
{
    [RequireComponent(typeof(JumpUpHandler))]
    [RequireComponent(typeof(StandUpHandler))]
    [RequireComponent(typeof(LieDownHandler))]
    [RequireComponent(typeof(GroundHandler))]

    [RequireComponent(typeof(PenguinSkeleton))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
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

        private Vector2     initialSpawnPosition;
        private Rigidbody2D penguinRigidBody;
        private Animator    penguinAnimator;

        public void Reset()
        {
            enableAutomaticAxisLockingWhenIdle = true;

            penguinRigidBody.velocity = Vector2.zero;
            penguinRigidBody.position = initialSpawnPosition;
            penguinRigidBody.isKinematic  = false;
            penguinRigidBody.useAutoMass  = false;
            penguinRigidBody.mass         = mass;
            penguinRigidBody.centerOfMass = new Vector2(centerOfMassX, centerOfMassY);

            penguinAnimator.applyRootMotion = true;
            penguinAnimator.updateMode = AnimatorUpdateMode.Normal;

            penguinRigidBody.transform.localEulerAngles = Vector3.zero;
        }

        void Awake()
        {
            penguinAnimator  = gameObject.GetComponent<Animator>();
            penguinRigidBody = gameObject.GetComponent<Rigidbody2D>();

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
    }
}
