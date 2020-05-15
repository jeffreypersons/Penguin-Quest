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
    [Tooltip("How strong is the rotation when moving up slopes? 0.0 for max softness, 1.0f for no kinematic softness")]
    [SerializeField] private float   kinematicRotationStrength     = default;
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
    private Vector2 forward;
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
        // todo: account for sliding on slopes, and find a way to reduce computations per frame without the penguin falling back...
        // also look into friction, slipperiness, etc...
        // ideally, once penguin sliding adjust for slope angle, sliding can be used to quickly travel down slopes and back up them..
        Vector3 groundNormal = GetSurfaceNormalOfGroundRelativeToPenguin();
        AlignPenguinWithGivenUpAxis(groundNormal);
        if (inputRotationAngle != 0.0f)
        {
            MathUtils.RotateRigidBodyAroundPointBy(rigidBody, inputRotationPivot, groundNormal, inputRotationAngle);
        }
        rigidBody.velocity = new Vector2(inputAxes.x * inputMoveSpeed, 0);
    }

    private Vector3 GetSurfaceNormalOfGroundRelativeToPenguin(float maxRayDistance=100.0f)
    {
        Vector3 colliderCenterInWorldSpace = collider.transform.TransformPoint(collider.bounds.center);
        RaycastHit2D hitInfo = Physics2D.Raycast(colliderCenterInWorldSpace,
            Vector3.down, maxRayDistance, LayerMask.GetMask("Ground"));
        return (hitInfo.transform == null) ? Vector2.up : hitInfo.normal;
    }

    private void AlignPenguinWithGivenUpAxis(Vector3 newUp)
    {
        // we use the old forward direction of the penguin crossed with the axis we wish to align to, to get a perpendicular
        // vector pointing in or out of the screen (note unity uses the left hand system), with magnitude proportional to steepness.
        // then using our desired `up-axis` crossed with our `left` vector, we get a new forward direction of the penguin
        // that's parallel with the slope that our given up is normal to.
        Vector3 left = Vector3.Cross(rigidBody.transform.forward, newUp);
        Vector3 newForward = Vector3.Cross(newUp, left);

        Quaternion oldRotation = rigidBody.transform.rotation;
        Quaternion newRotation = Quaternion.LookRotation(newForward, newUp);
        rigidBody.MoveRotation(Quaternion.Lerp(oldRotation, newRotation, kinematicRotationStrength));
    }


    private float GetNormalizedInput(string name)
    {
        float input = Input.GetAxisRaw(name);
        return Math.Abs(input) >= inputTolerance ? input : 0.00f;
    }
}
