﻿using UnityEngine;
using UnityEngine.InputSystem;


// todo: look into a way to use enums directly in animator (perhaps via passing int and using editor script, etc?)
[RequireComponent(typeof(PlayerInput))]
public class PenguinController : MonoBehaviour
{
    private bool isGrounded;
    private float xMotionIntensity;
    private Facing facing;
    private Posture posture;
    private enum Facing  { LEFT,    RIGHT   }
    private enum Posture { UPRIGHT, ONBELLY }

    [Header("Movement")]
    [Tooltip("Amount of progress made per frame when transitioning between states " +
             "(ie 0.05 for a blended delayed transition taking at least 20 frames," +
             "1.00 for an instant transition with no blending)")]
    [SerializeField] [Range(0.01f, 1.00f)] private float stateTransitionSpeed = 0.10f;

    [Header("Input Configuration")]
    private PlayerControls playerControls;

    private Vector2 initialSpawnPosition;
    private Animator penguinAnimator;
    private Rigidbody2D penguinRigidBody;
    private BoxCollider2D penguinCollider;

    private Vector3 PenguinCenter
    {
        get => penguinCollider.bounds.center;
    }
    private Vector3 PenguinScale
    {
        get => penguinRigidBody.transform.localScale;
        set => penguinRigidBody.transform.localScale = value;
    }
    private void UpdateAnimatorParameters()
    {
        // ideally we would use the enums directly, but enum is not a supported parameter type for animator
        penguinAnimator.SetBool("IsGrounded", isGrounded);
        penguinAnimator.SetBool("IsUpright",  posture == Posture.UPRIGHT);
        penguinAnimator.SetFloat("XMotionIntensity", xMotionIntensity);
    }

    // todo: add check for standup/facing-right/grounded/etc
    public void Reset()
    {
        penguinRigidBody.velocity = Vector2.zero;
        penguinRigidBody.position = initialSpawnPosition;

        isGrounded = true;
        xMotionIntensity = 0.00f;
        penguinAnimator.updateMode = AnimatorUpdateMode.AnimatePhysics;
        penguinAnimator.applyRootMotion = true;

        Standup();
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

        penguinRigidBody = gameObject.GetComponent<Rigidbody2D>();
        penguinCollider  = gameObject.GetComponent<BoxCollider2D>();
        penguinAnimator  = gameObject.GetComponent<Animator>();

        initialSpawnPosition = penguinRigidBody.position;
        Reset();
    }

    // todo: incorporate object references for fire/use request (ie what object is being used?)
    private bool IsFireRequested
    {
        get => playerControls.Gameplay.Fire.triggered;
    }
    private bool IsUseItemRequested
    {
        get => playerControls.Gameplay.Use.triggered;
    }
    private Vector2 MovementRequested
    {
        get => playerControls.Gameplay.Move.ReadValue<Vector2>();
    }

    void Update()
    {
        GroundCheck();
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

        if (inputAxes.y < 0.00f && isGrounded && posture == Posture.UPRIGHT)
        {
            LieDown();
        }
        else if (inputAxes.y > 0.00f && isGrounded && posture == Posture.UPRIGHT)
        {
            Jump();
        }
        else if (inputAxes.y > 0.00f && isGrounded && posture == Posture.ONBELLY)
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
        posture = Posture.UPRIGHT;
    }
    private void LieDown()
    {
        posture = Posture.ONBELLY;
    }
    private void Jump()
    {
        // todo: apply an impulse force
    }
    private void Fire()
    {
        // todo: throw a fish or something lol
        Debug.Log("Fire!");
    }
    private void Use()
    {
        // todo: use something (like equip an item, maybe?)
        Debug.Log("Using!");
    }
}
