using System;
using UnityEngine;


public class PenguinController : MonoBehaviour
{
    private bool isGrounded;
    private Facing  facing;
    private Posture posture;
    private enum Facing  { LEFT,    RIGHT   }
    private enum Posture { UPRIGHT, ONBELLY }

    [Header("Movement")]
    [Tooltip("Amount of progress made per frame when transitioning between states " +
             "(ie 0.05 for a blended delayed transition taking at least 20 frames," +
             "1.00 for an instant transition with no blending)")]
    [SerializeField] [Range(0.01f, 1.00f)] private float stateTransitionSpeed = 0.10f;

    [Header("Input Configuration")]
    [Tooltip("Threshold for recognizing inputs (ie 0.0 for no filter, 1.0 for only sensing full keyboard presses)")]
    [SerializeField] [Range(0, 1)] private float inputThreshold = 0.10f;

    [SerializeField] private string horizontalInputAxisName = default;
    [SerializeField] private string verticalInputAxisName   = default;

    private Vector2 inputAxes;
    private Vector2 initialSpawnPosition;
    private Animator      penguinAnimator;
    private Rigidbody2D   penguinRigidBody;
    private BoxCollider2D penguinCollider;

    // in general, we treat unmoving, idle-like states as our zero intensity default states
    private float AnimatorMotionIntensity
    {
        get => penguinAnimator.GetFloat("Motion_Intensity");
        set => penguinAnimator.SetFloat("Motion_Intensity", value);
    }
    private Vector3 PenguinCenter
    {
        get => penguinCollider.bounds.center;
    }
    private Vector3 PenguinScale
    {
        get => penguinRigidBody.transform.localScale;
        set => penguinRigidBody.transform.localScale = value;
    }

    public void Reset()
    {
        inputAxes = Vector2.zero;
        penguinRigidBody.velocity = Vector2.zero;
        penguinRigidBody.position = initialSpawnPosition;

        AnimatorMotionIntensity = 0.00f;
        penguinAnimator.updateMode = AnimatorUpdateMode.AnimatePhysics;
        penguinAnimator.applyRootMotion = true;

        GroundCheck();
        TurnToFace(Facing.RIGHT);
        ChangeToPosture(Posture.UPRIGHT);
    }
    void Awake()
    {
        penguinRigidBody = gameObject.GetComponent<Rigidbody2D>();
        penguinCollider  = gameObject.GetComponent<BoxCollider2D>();
        penguinAnimator  = gameObject.GetComponent<Animator>();

        initialSpawnPosition = penguinRigidBody.position;
        Reset();
    }

    void Update()
    {
        GroundCheck();

        inputAxes = new Vector2(GetNormalizedInput(horizontalInputAxisName), GetNormalizedInput(verticalInputAxisName));


        if (Mathf.Approximately(inputAxes.x, 0.00f))
        {
            AnimatorMotionIntensity = Mathf.Clamp01(AnimatorMotionIntensity - (stateTransitionSpeed));
        }
        else
        {
            AnimatorMotionIntensity = Mathf.Clamp01(AnimatorMotionIntensity + (Mathf.Abs(inputAxes.x) * stateTransitionSpeed));
            TurnToFace(inputAxes.x < 0 ? Facing.LEFT : Facing.RIGHT);
        }

        if (inputAxes.y < 0.00f)
        {
            ChangeToPosture(Posture.ONBELLY);
        }
        else if (inputAxes.y > 0.00f)
        {
            AttemptToJump();
        }
    }

    private float GetNormalizedInput(string name)
    {
        float input = Input.GetAxisRaw(name);
        return Math.Abs(input) >= inputThreshold ? input : 0.00f;
    }

    private void GroundCheck()
    {
        // todo: add proper raycasting against platform layers
        isGrounded = true;
    }
    private void TurnToFace(Facing facing)
    {
        if (this.facing == facing)
        {
            return;
        }

        // todo: take posture into account...
        this.facing = facing;
        switch (this.facing)
        {
            case Facing.LEFT:  PenguinScale = new Vector3(-Mathf.Abs(PenguinScale.x), PenguinScale.y, PenguinScale.z); break;
            case Facing.RIGHT: PenguinScale = new Vector3( Mathf.Abs(PenguinScale.x), PenguinScale.y, PenguinScale.z); break;
            default: Debug.LogError($"Given value `{facing}` is not a valid Facing"); return;
        }
    }
    private void ChangeToPosture(Posture posture)
    {
        if (this.posture == posture)
        {
            return;
        }

        this.posture = posture;
        switch (this.posture)
        {
            case Posture.ONBELLY: /* todo: set animator parameters to try to trigger state change, if possible */; break;
            case Posture.UPRIGHT: /* todo: set animator parameters to try to trigger state change, if possible */; break;
            default: Debug.LogError($"Given value `{posture}` is not a valid Posture"); return;
        }
    }
    private void AttemptToJump()
    {
        if (this.posture == Posture.UPRIGHT)
        {
            // todo: apply an impulse force
        }
    }
}
