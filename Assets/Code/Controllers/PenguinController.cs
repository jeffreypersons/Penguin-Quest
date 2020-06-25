using System;
using UnityEngine;


public class PenguinController : MonoBehaviour
{
    private Facing facing;
    private enum Facing { LEFT, RIGHT }

    [Header("Penguin Movement Speeds")]
    [Tooltip("How fast can the penguin walk (as a multiple of its default animation speed)?")]
    [SerializeField] private float walkingSpeedMultiplier = 1.00f;

    [Header("Input Configuration")]
    [Tooltip("How sensitive is the penguin to input? " +
             "0.0 for all inputs to be recognized, 1.0 for only full strength presses to be recognized")]
    [SerializeField] private float inputTolerance = 0.10f;
    [SerializeField] private string horizontalInputAxisName = default;
    [SerializeField] private string verticalInputAxisName   = default;

    private Vector2 inputAxes;
    private Vector2 initialSpawnPosition;

    [Header("Components from child")]
    [SerializeField] private Animator      penguinAnimator;
    [SerializeField] private Rigidbody2D   penguinRigidBody;
    [SerializeField] private BoxCollider2D penguinCollider;

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

        penguinRigidBody.velocity = inputAxes * walkingSpeedMultiplier;
        penguinRigidBody.position = initialSpawnPosition;

        TurnToFace(Facing.RIGHT);
    }
    void Awake()
    {
        initialSpawnPosition = penguinRigidBody.position;
        Reset();
    }

    // currently assumes posture is walking
    void Update()
    {
        inputAxes = new Vector2(GetNormalizedInput(horizontalInputAxisName), GetNormalizedInput(verticalInputAxisName));

        penguinAnimator.SetFloat("Upright_Speed", Mathf.Abs(inputAxes.x));
        TurnToFace(inputAxes.x < 0 ? Facing.LEFT : Facing.RIGHT);
        penguinAnimator.applyRootMotion = true;
        /*
         * todo: figure out best way to apply the `walkingSpeedMultiplier`
        if (penguinAnimator.GetCurrentAnimatorStateInfo(0).IsName("Upright_Walk"))
        {
            penguinAnimator.speed = penguinAnimator.GetCurrentAnimatorStateInfo(0).speed * walkingSpeedMultiplier * Mathf.Abs(inputAxes.x);
        }
        */
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
}
