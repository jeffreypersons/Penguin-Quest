using System;
using UnityEngine;


public class PenguinController : MonoBehaviour
{
    private Facing  facing;
    private Posture posture;
    private enum Facing  { LEFT,     RIGHT }
    private enum Posture { STANDING, LYING }

    [SerializeField] private float walkingSpeed = default;
    [SerializeField] private float slidingSpeed = default;

    private float inputTolerance = 0.15f;
    [SerializeField] private string horizontalInputAxisName = default;
    [SerializeField] private string verticalInputAxisName   = default;

    private Vector2 initialSpawnPosition;
    private Vector2 inputVelocity;
    private Rigidbody2D rigidBody;
    private BoxCollider2D collider;

    public void Reset()
    {
        facing  = Facing.RIGHT;
        posture = Posture.STANDING;

        inputVelocity      = Vector2.zero;
        rigidBody.velocity = inputVelocity;
        rigidBody.position = initialSpawnPosition;
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
        float xInput = GetNormalizedInput(horizontalInputAxisName, inputTolerance);
        if (xInput < 0 && facing == Facing.RIGHT)
        {
            facing = Facing.LEFT;
            rigidBody.transform.localScale *= new Vector2(-1.00f, 1.00f);
        }
        else if (xInput > 0 && facing == Facing.LEFT)
        {
            facing = Facing.RIGHT;
            rigidBody.transform.localScale *= new Vector2(-1.00f, 1.00f);
        }

        float yInput = GetNormalizedInput(verticalInputAxisName, inputTolerance);
        if (yInput < 0 && posture == Posture.STANDING)
        {
            LieDown();
        }
        else if (yInput > 0 && posture == Posture.LYING)
        {
            StandUp();
        }

        switch (posture)
        {
            case Posture.STANDING: inputVelocity = new Vector2(walkingSpeed * xInput, 0);          break;
            case Posture.LYING:    inputVelocity = new Vector2(slidingSpeed * xInput, 0);          break;
            default:               inputVelocity = new Vector2(0, 0); LogInvalidEnumError(facing); break;
        }
    }
    void FixedUpdate()
    {
        rigidBody.velocity = inputVelocity;

        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = new Quaternion(0.0f, transform.rotation.y, 0.0f, transform.rotation.w);
        transform.rotation = Quaternion.Slerp(startRotation, endRotation, Time.fixedDeltaTime * 2.0f);
    }

    void ChangePosture(Posture newPosture)
    {
        if (newPosture == posture)
        {
            return;
        }
        switch (newPosture)
        {
            case Posture.STANDING: StandUp(); break;
            case Posture.LYING:    LieDown(); break;
            default: LogInvalidEnumError(newPosture); break;
        }
    }

    void FaceLeft()
    {
        facing = Facing.LEFT;
        rigidBody.transform.localScale *= new Vector2(-1.00f, 1.00f);
    }
    void FaceRight()
    {
        facing = Facing.RIGHT;
        rigidBody.transform.localScale *= new Vector2(-1.00f, 1.00f);
    }
    void LieDown()
    {
        posture = Posture.LYING;
    }
    void StandUp()
    {
        posture = Posture.STANDING;

    }
    void RotateTo()
    {
    }

    private static float GetNormalizedInput(string name, float tolerance)
    {
        float input = Input.GetAxisRaw(name);
        return Math.Abs(input) >= tolerance ? input : 0.00f;
    }
    private void LogInvalidEnumError(Enum enumValue)
    {
        Debug.LogError($"{nameof(enumValue)}({enumValue}) is not a valid enum");
    }
}
