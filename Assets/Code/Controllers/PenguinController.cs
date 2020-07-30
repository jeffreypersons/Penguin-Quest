using UnityEngine;
using UnityEngine.Experimental.U2D.Animation;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(GroundChecker))]
public class PenguinController : MonoBehaviour
{
    private const float STATE_TRANSITION_SPEED_DEFAULT = 0.10f;
    private const float STATE_TRANSITION_SPEED_MIN     = 0.10f;
    private const float STATE_TRANSITION_SPEED_MAX     = 1.00f;
    private const float JUMP_STRENGTH_DEFAULT =  50000.00f;
    private const float JUMP_STRENGTH_MIN     =  25000.00f;
    private const float JUMP_STRENGTH_MAX     = 250000.00f;
    private const float JUMP_ANGLE_DEFAULT = 45.00f;
    private const float JUMP_ANGLE_MIN     =  0.00f;
    private const float JUMP_ANGLE_MAX     = 90.00f;

    [Header("Animation Settings")]
    [Tooltip("Amount of progress made per frame when transitioning between states " +
             "(ie 0.05 for a blended delayed transition taking at least 20 frames," +
             "1.00 for an instant transition with no blending)")]
    [SerializeField] [Range(STATE_TRANSITION_SPEED_MIN, STATE_TRANSITION_SPEED_MAX)]
    private float stateTransitionSpeed = STATE_TRANSITION_SPEED_DEFAULT;

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
    private Animator penguinAnimator;
    private PlayerControls playerControls;
    private GroundChecker groundChecker;

    private Rigidbody2D penguinRigidBody;
    private BoxCollider2D penguinCollider;

    private float xMotionIntensity;
    private Facing facing;
    private Posture posture;
    private enum Facing  { LEFT,    RIGHT   }
    private enum Posture { UPRIGHT, ONBELLY }

    private bool    IsFireRequested    => playerControls.Gameplay.Fire.triggered;
    private bool    IsUseItemRequested => playerControls.Gameplay.Use.triggered;
    private Vector2 MovementRequested  => playerControls.Gameplay.Move.ReadValue<Vector2>();

    // update all animator parameters (except for triggers, as those should be set directly)
    private void UpdateAnimatorParameters()
    {
        // ideally we would use the enums directly, but enum is not a supported parameter type for animator
        penguinAnimator.SetBool("IsGrounded", groundChecker.WasDetected);
        penguinAnimator.SetBool("IsUpright",  posture == Posture.UPRIGHT);
        penguinAnimator.SetFloat("XMotionIntensity", xMotionIntensity);
    }
    void OnLiedownAnimationEvent()
    {

    }
    void OnJumpAnimationEvent()
    {
        Vector3 jumpDirection = MathUtils.RotateBy(forwardAxis, jumpAngle);
        penguinRigidBody.AddForce(jumpStrength * jumpDirection, ForceMode2D.Impulse);
    }
    void OnStandupAnimationEvent()
    {

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
        penguinRigidBody.velocity = Vector2.zero;
        penguinRigidBody.position = initialSpawnPosition;

        xMotionIntensity = 0.00f;
        penguinAnimator.updateMode = AnimatorUpdateMode.AnimatePhysics;
        penguinAnimator.applyRootMotion = true;

        upAxis      = Vector3.up;
        forwardAxis = Vector3.right;
        TurnToFace(Facing.RIGHT);
        UpdateAnimatorParameters();
    }
    void OnEnable()
    {
        playerControls.Gameplay.Enable();
    }
    void OnDisable()
    {
        playerControls.Gameplay.Disable();
    }
    void Awake()
    {
        playerControls = new PlayerControls();
        penguinAnimator  = gameObject.GetComponent<Animator>();
        groundChecker    = gameObject.GetComponent<GroundChecker>();
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

        Vector2 inputAxes = MovementRequested;
        if (Mathf.Approximately(inputAxes.x, 0.00f))
        {
            xMotionIntensity = Mathf.Clamp01(xMotionIntensity - (stateTransitionSpeed));
        }
        else
        {
            xMotionIntensity = Mathf.Clamp01(xMotionIntensity + (Mathf.Abs(inputAxes.x) * stateTransitionSpeed));
            TurnToFace(inputAxes.x < 0 ? Facing.LEFT : Facing.RIGHT);
        }

        if (inputAxes.y < 0.00f && groundChecker.WasDetected && posture == Posture.UPRIGHT)
        {
            penguinAnimator.SetTrigger("Liedown");
            posture = Posture.ONBELLY;
        }
        else if (inputAxes.y > 0.00f && groundChecker.WasDetected && posture == Posture.UPRIGHT)
        {
            penguinAnimator.SetTrigger("Jump");
        }
        else if (inputAxes.y > 0.00f && groundChecker.WasDetected && posture == Posture.ONBELLY)
        {
            penguinAnimator.SetTrigger("Standup");
            posture = Posture.UPRIGHT;
        }

        if (IsFireRequested)
        {
            penguinAnimator.SetTrigger("Fire");
        }
        if (IsUseItemRequested)
        {
            penguinAnimator.SetTrigger("Use");
        }

        UpdateAnimatorParameters();
    }
    void LateUpdate()
    {
        Vector3 jumpDirection = MathUtils.RotateBy(forwardAxis, jumpAngle);
        penguinRigidBody.AddForce(jumpStrength * jumpDirection, ForceMode2D.Impulse);
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
