using System;
using UnityEngine;


public class PenguinController : MonoBehaviour
{
    private Facing  facing;
    private Posture posture;
    private enum Facing  { LEFT,     RIGHT }
    private enum Posture { STANDING, LYING }

    [Header("Penguin Movement Speeds")]
    [SerializeField] private float walkingSpeed = default;
    [SerializeField] private float slidingSpeed = default;

    [Header("Input Configuration")]
    [SerializeField] private float   inputTolerance                = default;
    [SerializeField] private Vector2 pivotWhenRotatingDownToLying  = default;
    [SerializeField] private Vector2 pivotWhenRotatingUpToStanding = default;
    [SerializeField] private string  horizontalInputAxisName       = default;
    [SerializeField] private string  verticalInputAxisName         = default;

    private Vector2 inputAxes;
    private float inputMoveSpeed;
    private float inputRotationAngle;
    private Vector3 inputRotationPivot;

    private Vector2 initialSpawnPosition;
    private Rigidbody2D rigidBody;
    private BoxCollider2D collider;

    public void Reset()
    {
        inputAxes = Vector2.zero;
        inputMoveSpeed = walkingSpeed;
        inputRotationAngle = 0.0f;

        rigidBody.rotation = inputRotationAngle;
        rigidBody.velocity = inputAxes * inputMoveSpeed;
        rigidBody.position = initialSpawnPosition;

        facing = Facing.RIGHT;
        posture = Posture.STANDING;
        if (rigidBody.transform.localScale.x < 0.0f)
        {
            rigidBody.transform.localScale *= new Vector2(-1, 0);
        }
    }
    void Awake()
    {
        rigidBody = gameObject.GetComponent<Rigidbody2D>();
        collider  = gameObject.GetComponent<BoxCollider2D>();
        initialSpawnPosition = rigidBody.position;
        Reset();
    }

    void Update()
    {
        inputAxes = new Vector2(GetNormalizedInput(horizontalInputAxisName), GetNormalizedInput(verticalInputAxisName));
        if (inputAxes.x < 0 && facing == Facing.RIGHT)
        {
            rigidBody.transform.localScale *= posture == Posture.STANDING? new Vector2(-1, 1) : new Vector2(1, -1);
            facing = Facing.LEFT;
        }
        else if (inputAxes.x > 0 && facing == Facing.LEFT)
        {
            rigidBody.transform.localScale *= posture == Posture.STANDING ? new Vector2(-1, 1) : new Vector2(1, -1);
            facing = Facing.RIGHT;
        }

        inputRotationAngle = 0.0f;
        if (inputAxes.y < 0 && posture == Posture.STANDING)
        {
            inputRotationAngle = -90;
            inputRotationPivot = MathUtils.GetPointInsideBounds(collider.bounds, pivotWhenRotatingDownToLying);
            inputMoveSpeed     = slidingSpeed;
            posture            = Posture.LYING;
        }
        else if (inputAxes.y > 0 && posture == Posture.LYING)
        {
            inputRotationAngle = 90;
            inputRotationPivot = MathUtils.GetPointInsideBounds(collider.bounds, pivotWhenRotatingUpToStanding);
            inputMoveSpeed     = walkingSpeed;
            posture            = Posture.STANDING;
        }
    }
    void FixedUpdate()
    {
        if (inputRotationAngle != 0.0f)
        {
            //rigidBody.rotation = inputRotationAngle;
            MathUtils.RotateRigidBodyAroundPointBy(rigidBody, inputRotationPivot, Vector3.forward, inputRotationAngle);
        }
        rigidBody.velocity = new Vector2(inputAxes.x * inputMoveSpeed, 0);
    }

    private float GetNormalizedInput(string name)
    {
        float input = Input.GetAxisRaw(name);
        return Math.Abs(input) >= inputTolerance ? input : 0.00f;
    }
}
