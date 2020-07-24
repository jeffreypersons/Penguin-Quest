using UnityEngine;
using UnityEngine.Experimental.U2D.Animation;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteLibrary))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(GroundChecker))]
public class PenguinController : MonoBehaviour
{
    private const float STATE_TRANSITION_SPEED_DEFAULT = 0.10f;
    private const float STATE_TRANSITION_SPEED_MIN     = 0.10f;
    private const float STATE_TRANSITION_SPEED_MAX     = 1.00f;
    private const float JUMP_FORCE_DEFAULT =  50000.00f;
    private const float JUMP_FORCE_MIN     =  25000.00f;
    private const float JUMP_FORCE_MAX     = 250000.00f;

    private Vector2 initialSpawnPosition;
    private Animator penguinAnimator;
    private Rigidbody2D penguinRigidBody;
    private BoxCollider2D penguinCollider;
    private PlayerControls playerControls;
    private GroundChecker groundChecker;

    private float xMotionIntensity;
    private Facing facing;
    private Posture posture;
    private enum Facing  { LEFT,    RIGHT   }
    private enum Posture { UPRIGHT, ONBELLY }

    [Header("Animation Settings")]
    [Tooltip("Amount of progress made per frame when transitioning between states " +
             "(ie 0.05 for a blended delayed transition taking at least 20 frames," +
             "1.00 for an instant transition with no blending)")]
    [SerializeField] [Range(STATE_TRANSITION_SPEED_MIN, STATE_TRANSITION_SPEED_MAX)]
    private float stateTransitionSpeed = STATE_TRANSITION_SPEED_DEFAULT;

    [Header("Jump Settings")]
    [Tooltip("Jump force in newtons")]
    [SerializeField] [Range(JUMP_FORCE_MIN, JUMP_FORCE_MAX)]
    private float jumpForce = JUMP_FORCE_DEFAULT;

    [Header("Misc Settings")]
    [Tooltip("Reference to the bottom-most root joint of the penguin skeletal model")]
    [SerializeField]
    private Transform skeletalRoot = default;

    private Vector3 PenguinScale
    {
        get => penguinRigidBody.transform.localScale;
        set => penguinRigidBody.transform.localScale = value;
    }
    // todo: incorporate object references for fire/use request (ie what object is being used?)
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

    // todo: add check for standup/facing-right/grounded/etc
    public void Reset()
    {
        penguinRigidBody.velocity = Vector2.zero;
        penguinRigidBody.position = initialSpawnPosition;

        xMotionIntensity = 0.00f;
        penguinAnimator.updateMode = AnimatorUpdateMode.AnimatePhysics;
        penguinAnimator.applyRootMotion = true;

        Standup();
        TurnToFace(Facing.RIGHT);
        groundChecker.Reset();
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
        penguinRigidBody = gameObject.GetComponent<Rigidbody2D>();
        penguinCollider  = gameObject.GetComponent<BoxCollider2D>();
        penguinAnimator  = gameObject.GetComponent<Animator>();
        groundChecker    = gameObject.GetComponent<GroundChecker>();
        initialSpawnPosition = penguinRigidBody.position;
        Reset();
    }

    void Update()
    {
        groundChecker.CheckForGround(fromPoint: skeletalRoot.position,
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
            LieDown();
        }
        else if (inputAxes.y > 0.00f && groundChecker.WasDetected && posture == Posture.UPRIGHT)
        {
            Jump();
        }
        else if (inputAxes.y > 0.00f && groundChecker.WasDetected && posture == Posture.ONBELLY)
        {
            Standup();
        }

        if (IsFireRequested)
        {
            Fire();
        }
        if (IsUseItemRequested)
        {
            Use();
        }

        UpdateAnimatorParameters();
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
            case Facing.LEFT:  PenguinScale = new Vector3(-Mathf.Abs(PenguinScale.x), PenguinScale.y, PenguinScale.z); break;
            case Facing.RIGHT: PenguinScale = new Vector3( Mathf.Abs(PenguinScale.x), PenguinScale.y, PenguinScale.z); break;
            default: Debug.LogError($"Given value `{facing}` is not a valid Facing"); return;
        }
    }
    private void Standup()
    {
        Debug.Log("Standup!");
        posture = Posture.UPRIGHT;
    }
    private void LieDown()
    {
        Debug.Log("Lie Down!");
        posture = Posture.ONBELLY;
    }
    private void Jump()
    {
        Debug.Log("Jump!");
        penguinRigidBody.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
        penguinAnimator.SetTrigger("Jump");
    }
    private void Fire()
    {
        Debug.Log("Fire!");

        // todo: throw a fish or something lol
    }
    private void Use()
    {
        Debug.Log("Using!");

        // todo: use something (like equip an item, maybe?)
    }
}
