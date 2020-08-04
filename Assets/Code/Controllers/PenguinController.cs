using UnityEngine;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(GroundChecker))]
[RequireComponent(typeof(GameplayInputReciever))]
public class PenguinController : MonoBehaviour
{
    private const float BLEND_SPEED_DEFAULT = 0.10f;
    private const float BLEND_SPEED_MIN     = 0.10f;
    private const float BLEND_SPEED_MAX     = 1.00f;
    private const float JUMP_STRENGTH_DEFAULT =  50000.00f;
    private const float JUMP_STRENGTH_MIN     =  25000.00f;
    private const float JUMP_STRENGTH_MAX     = 250000.00f;
    private const float JUMP_ANGLE_DEFAULT = 45.00f;
    private const float JUMP_ANGLE_MIN     =  0.00f;
    private const float JUMP_ANGLE_MAX     = 90.00f;

    [Header("Animation Settings")]
    [Tooltip("Amount of progress made per frame when transitioning between idle/moving states " +
             "(ie 0.05 for a blended delayed transition taking at least 20 frames," +
             "1.00 for an instant transition with no blending)")]
    [SerializeField] [Range(BLEND_SPEED_MIN, BLEND_SPEED_MAX)]
    private float locomotionBlendSpeed = BLEND_SPEED_DEFAULT;

    [Header("Jump Settings")]
    [Tooltip("Strength of jump force in newtons")]
    [SerializeField] [Range(JUMP_STRENGTH_MIN, JUMP_STRENGTH_MAX)]
    private float jumpStrength = JUMP_STRENGTH_DEFAULT;

    [Tooltip("Angle to jump (in degrees counterclockwise to the penguin's forward axis)")]
    [SerializeField] [Range(JUMP_ANGLE_MIN, JUMP_ANGLE_MAX)]
    private float jumpAngle = JUMP_ANGLE_DEFAULT;

    private Vector3 upAxis;
    private Vector3 forwardAxis;
    private Vector2 initialSpawnPosition;

    private GroundChecker groundChecker;
    private GameplayInputReciever input;
    private Animator penguinAnimator;
    private Rigidbody2D penguinRigidBody;
    private BoxCollider2D penguinCollider;

    private Facing facing;
    private Posture posture;
    private enum Facing  { LEFT, RIGHT }
    private enum Posture { UPRIGHT, ONBELLY, BENTOVER }

    private Vector2 netImpulseForce;
    private float xMotionIntensity;

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
        netImpulseForce += jumpStrength * MathUtils.RotateBy(forwardAxis, jumpAngle);
    }
    void OnLiedownAnimationEventStart()
    {
        posture = Posture.BENTOVER;
    }
    void OnLiedownAnimationEventEnd()
    {
        posture = Posture.ONBELLY;
    }
    void OnStandupAnimationEventStart()
    {
        posture = Posture.BENTOVER;
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
    public void Reset()
    {
        groundChecker.Reset();
        netImpulseForce = Vector2.zero;
        penguinRigidBody.velocity = Vector2.zero;
        penguinRigidBody.position = initialSpawnPosition;

        xMotionIntensity = 0.00f;
        penguinAnimator.applyRootMotion = true;
        penguinAnimator.updateMode = AnimatorUpdateMode.Normal;
        ClearVerticalMovementTriggers();

        upAxis = Vector3.up;
        forwardAxis = Vector3.right;
        TurnToFace(Facing.RIGHT);
        UpdateAnimatorParameters();
    }
    void Awake()
    {
        penguinAnimator  = gameObject.GetComponent<Animator>();
        groundChecker    = gameObject.GetComponent<GroundChecker>();
        input            = gameObject.GetComponent<GameplayInputReciever>();
        penguinRigidBody = gameObject.GetComponentInChildren<Rigidbody2D>();
        penguinCollider  = gameObject.GetComponentInChildren<BoxCollider2D>();
        initialSpawnPosition = penguinRigidBody.position;
        Reset();
    }

    void Update()
    {
        // todo: utilize quaternions to rotate `upAxis`/`forwardAxis` to match `groundChecker.SurfaceNormalOfLastContact`
        groundChecker.CheckForGround(fromPoint: penguinAnimator.rootPosition,
                                     extraLineHeight: penguinCollider.bounds.extents.y);

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

    // things we want to do AFTER the animator updates positions
    void LateUpdate()
    {
        if (netImpulseForce != Vector2.zero)
        {
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

        Vector3 scale = penguinRigidBody.transform.localScale;
        this.facing = facing;
        switch (this.facing)
        {
            case Facing.LEFT:
                penguinRigidBody.transform.localScale = new Vector3(-Mathf.Abs(scale.x), scale.y, scale.z);
                break;
            case Facing.RIGHT:
                penguinRigidBody.transform.localScale = new Vector3( Mathf.Abs(scale.x), scale.y, scale.z);
                break;
            default:
                Debug.LogError($"Given value `{facing}` is not a valid Facing"); return;
        }
    }
}
