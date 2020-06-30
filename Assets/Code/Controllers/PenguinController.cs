using System;
using UnityEngine;


public class PenguinController : MonoBehaviour
{
    private Facing  facing;
    private Posture posture;
    private enum Facing  { LEFT, RIGHT }
    private enum Posture { UPRIGHT, ONBELLY, MIDAIR }

    [Header("Penguin Movement Speeds")]
    [Tooltip("How fast can the penguin walk (as a multiple of its default animation speed)?")]
    [SerializeField] private float walkingSpeedMultiplier = 1.00f;

    [Header("Input Configuration")]
    [Tooltip("How sensitive is the penguin to input? " +
             "0.0 for all inputs to be recognized, 1.0 for only full strength presses to be recognized")]
    [SerializeField] private float  inputTolerance = 0.10f;
    [SerializeField] private string horizontalInputAxisName = default;
    [SerializeField] private string verticalInputAxisName   = default;

    [Header("Input Configuration")]
    private Vector2 inputAxes;
    private Vector2 initialSpawnPosition;

    private Animator      penguinAnimator;
    private Rigidbody2D   penguinRigidBody;
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

    public void Reset()
    {
        inputAxes = Vector2.zero;
        penguinRigidBody.velocity = Vector2.zero;
        penguinRigidBody.position = initialSpawnPosition;
        TurnToFace(Facing.RIGHT);
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
        inputAxes = new Vector2(GetNormalizedInput(horizontalInputAxisName), GetNormalizedInput(verticalInputAxisName));
        if (inputAxes.x == 0)
        {
            SetAnimationPlaySpeed(0);
        }
        else
        {
            TurnToFace(inputAxes.x < 0 ? Facing.LEFT : Facing.RIGHT);
            SetAnimationPlaySpeed(Mathf.Abs(inputAxes.x));
        }

        penguinAnimator.applyRootMotion = true;
    }

    private float GetNormalizedInput(string name)
    {
        float input = Input.GetAxisRaw(name);
        return Math.Abs(input) >= inputTolerance ? input : 0.00f;
    }
    private void TurnToFace(Facing facing)
    {
        if (this.facing == facing)
        {
            return;
        }

        this.facing = facing;
        switch (facing)
        {
            case Facing.LEFT:  PenguinScale = new Vector3(-Mathf.Abs(PenguinScale.x), PenguinScale.y, PenguinScale.z); break;
            case Facing.RIGHT: PenguinScale = new Vector3( Mathf.Abs(PenguinScale.x), PenguinScale.y, PenguinScale.z); break;
            default: Debug.LogError($"Given value `{facing}` is not a valid facing"); return;
        }
    }
    private void SetAnimationPlaySpeed(float playSpeed)
    {
        switch (posture)
        {
            case Posture.UPRIGHT: penguinAnimator.SetFloat("PlaySpeed_Upright", playSpeed); break;
            case Posture.ONBELLY: penguinAnimator.SetFloat("PlaySpeed_OnBelly", playSpeed); break;
            case Posture.MIDAIR:  penguinAnimator.SetFloat("PlaySpeed_Midair",  playSpeed); break;
            default: Debug.LogError($"Field value `{posture}` is not a valid posture"); break;
        }
    }
}
