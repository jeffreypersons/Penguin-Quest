﻿using System;
using UnityEngine;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GroundChecker))]
[RequireComponent(typeof(GameplayInputReciever))]
public class PenguinController : MonoBehaviour
{
    private const float JUMP_STRENGTH_DEFAULT =  50000.00f;
    private const float JUMP_STRENGTH_MIN     =  25000.00f;
    private const float JUMP_STRENGTH_MAX     = 250000.00f;
    private const float JUMP_ANGLE_DEFAULT    =     45.00f;
    private const float JUMP_ANGLE_MIN        =      0.00f;
    private const float JUMP_ANGLE_MAX        =     90.00f;

    private const float LOCOMOTION_BLEND_STEP_DEFAULT = 0.10f;
    private const float LOCOMOTION_BLEND_STEP_MIN     = 0.01f;
    private const float LOCOMOTION_BLEND_STEP_MAX     = 1.00f;

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

    private static readonly Quaternion ROTATION_FACING_LEFT  = Quaternion.Euler(0, 180, 0);
    private static readonly Quaternion ROTATION_FACING_RIGHT = Quaternion.Euler(0,   0, 0);


    [Header("Jump Settings")]
    [Tooltip("Strength of jump force in newtons")]
    [Range(JUMP_STRENGTH_MIN, JUMP_STRENGTH_MAX)]
    [SerializeField] private float jumpStrength = JUMP_STRENGTH_DEFAULT;

    [Tooltip("Angle to jump (in degrees counterclockwise to the penguin's forward facing direction)")]
    [Range(JUMP_ANGLE_MIN, JUMP_ANGLE_MAX)] [SerializeField] private float jumpAngle = JUMP_ANGLE_DEFAULT;


    [Header("Animation Settings")]
    [Tooltip("Step size used to adjust blend percent when transitioning between idle/moving states" +
             "(ie 0.05 for blended delayed transition taking at least 20 frames, 1 for instant transition)")]
    [Range(LOCOMOTION_BLEND_STEP_MIN, LOCOMOTION_BLEND_STEP_MAX)]
    [SerializeField] private float locomotionBlendSpeed = LOCOMOTION_BLEND_STEP_DEFAULT;


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

    [Header("Collider References")]
    [SerializeField] private CapsuleCollider2D headCollider              = default;
    [SerializeField] private CapsuleCollider2D torsoCollider             = default;
    [SerializeField] private CapsuleCollider2D frontFlipperUpperCollider = default;
    [SerializeField] private CapsuleCollider2D frontFlipperLowerCollider = default;
    [SerializeField] private CapsuleCollider2D frontFootCollider         = default;
    [SerializeField] private CapsuleCollider2D backFootCollider          = default;


    private Vector2 initialSpawnPosition;
    private GroundChecker groundChecker;
    private Rigidbody2D penguinRigidBody;
    private Animator penguinAnimator;
    private GameplayInputReciever input;

    private Facing facing;
    private Posture posture;
    private enum Facing  { LEFT, RIGHT }
    private enum Posture { UPRIGHT, ONBELLY, BENTOVER }

    private Vector2 netImpulseForce;
    private float xMotionIntensity;

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
        penguinAnimator.SetBool("IsUpright",  posture == Posture.UPRIGHT);
        penguinAnimator.SetFloat("XMotionIntensity", xMotionIntensity);
    }
    private void ClearVerticalMovementTriggers()
    {
        penguinAnimator.ResetTrigger("Jump");
        penguinAnimator.ResetTrigger("Standup");
        penguinAnimator.ResetTrigger("Liedown");
    }

    void OnJumpAnimationEventImpulse()
    {
        // clear jump trigger to avoid triggering a jump after landing,
        // in the case that jump is pressed twice in a row
        ClearVerticalMovementTriggers();
        float angleFromGround = jumpAngle * Mathf.Deg2Rad;
        netImpulseForce += jumpStrength * new Vector2(Mathf.Cos(angleFromGround), Mathf.Sin(angleFromGround));
    }
    void OnLiedownAnimationEventStart()
    {
        posture = Posture.BENTOVER;
    }
    void OnLiedownAnimationEventMid()
    {
        frontFootCollider.enabled = false;
        backFootCollider.enabled  = false;
    }
    void OnLiedownAnimationEventEnd()
    {
        posture = Posture.ONBELLY;
        frontFlipperUpperCollider.enabled = false;
        frontFlipperLowerCollider.enabled = false;
    }
    void OnStandupAnimationEventStart()
    {
        posture = Posture.BENTOVER;
        frontFlipperUpperCollider.enabled = true;
        frontFlipperLowerCollider.enabled = true;
        frontFootCollider.enabled = true;
        backFootCollider.enabled  = true;
    }
    void OnStandupAnimationEventEnd()
    {
        posture = Posture.UPRIGHT;
    }
    void OnFireAnimationEvent()
    {

    }
    void OnUseAnimationEvent()
    {

    }

    public override string ToString()
    {
        return $"Penguin with a {Enum.GetName(typeof(Posture), posture)} posture and " +
               $"facing towards the {Enum.GetName(typeof(Facing), facing)}";
    }

    public void Reset()
    {
        UnlockAllAxes();
        enableAutomaticAxisLockingWhenIdle = true;

        groundChecker.Reset();
        netImpulseForce = Vector2.zero;
        penguinRigidBody.velocity = Vector2.zero;
        penguinRigidBody.position = initialSpawnPosition;
        penguinRigidBody.isKinematic  = false;
        penguinRigidBody.useAutoMass  = false;
        penguinRigidBody.mass         = mass;
        penguinRigidBody.centerOfMass = new Vector2(centerOfMassX, centerOfMassY);

        xMotionIntensity = 0.00f;
        penguinAnimator.applyRootMotion = true;
        penguinAnimator.updateMode = AnimatorUpdateMode.Normal;
        ClearVerticalMovementTriggers();

        TurnToFace(Facing.RIGHT);
        UpdateAnimatorParameters();

        // align penguin with surface normal in a single update
        posture = Posture.UPRIGHT;
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
        input            = gameObject.GetComponent<GameplayInputReciever>();

        initialSpawnPosition = penguinRigidBody.position;
        Reset();
    }

    #if UNITY_EDITOR
    void OnValidate()
    {
        if (!penguinRigidBody || !Application.IsPlaying(penguinRigidBody))
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
        groundChecker.CheckForGround(fromPoint: ComputeReferencePoint(), extraLineHeight: torsoCollider.bounds.extents.y);
        if (Mathf.Approximately(input.Axes.x, 0.00f))
        {
            xMotionIntensity = Mathf.Clamp01(xMotionIntensity - (locomotionBlendSpeed));
        }
        else
        {
            xMotionIntensity = Mathf.Clamp01(xMotionIntensity + (Mathf.Abs(input.Axes.x) * locomotionBlendSpeed));
            TurnToFace(input.Axes.x < 0 ? Facing.LEFT : Facing.RIGHT);
        }

        if (input.Axes.y < 0.00f && groundChecker.WasDetected && posture == Posture.UPRIGHT)
        {
            penguinAnimator.SetTrigger("Liedown");
        }
        else if (input.Axes.y > 0.00f && groundChecker.WasDetected && posture == Posture.ONBELLY)
        {
            penguinAnimator.SetTrigger("Standup");
        }
        else if (input.Axes.y > 0.00f && groundChecker.WasDetected && posture == Posture.UPRIGHT)
        {
            penguinAnimator.SetTrigger("Jump");
        }

        if (input.FireHeldThisFrame)
        {
            penguinAnimator.SetTrigger("Fire");
        }
        if (input.UseHeldThisFrame)
        {
            penguinAnimator.SetTrigger("Use");
        }

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
        if (!MathUtils.AreComponentsEqual(input.Axes, Vector2.zero)         ||
            Mathf.Abs(degreesUnaligned) > degreesFromSurfaceNormalThreshold ||
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

    // things we want to do AFTER the animator updates positions
    void LateUpdate()
    {
        if (netImpulseForce != Vector2.zero)
        {
            UnlockAllAxes();
            penguinRigidBody.AddForce(netImpulseForce, ForceMode2D.Impulse);
            netImpulseForce = Vector2.zero;
        }
    }

    private void TurnToFace(Facing facing)
    {
        if (this.facing == facing)
        {
            return;
        }

        this.facing = facing;
        switch (this.facing)
        {
            case Facing.LEFT:
                transform.localRotation = ROTATION_FACING_LEFT;
                break;
            case Facing.RIGHT:
                transform.localRotation = ROTATION_FACING_RIGHT;
                break;
            default:
                Debug.LogError($"Given value `{facing}` is not a valid Facing");
                break;
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
