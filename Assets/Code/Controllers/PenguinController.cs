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
    [SerializeField] private string  horizontalInputAxisName       = default;
    [SerializeField] private string  verticalInputAxisName         = default;
    [SerializeField] private Vector2 pivotWhenRightAndRotatingDownToLying  = default;
    [SerializeField] private Vector2 pivotWhenLeftAndRotatingDownToLying   = default;
    [SerializeField] private Vector2 pivotWhenRightAndRotatingUpToStanding = default;
    [SerializeField] private Vector2 pivotWhenLeftAndRotatingUpToStanding  = default;

    private Vector2 inputAxes;
    private float inputMoveSpeed;
    private float inputRotationAngle;
    private Vector3 inputRotationPivot;

    private Vector2 initialSpawnPosition;
    private Rigidbody2D   penguinRigidBody;
    private BoxCollider2D penguinCollider;

    private Vector3 PenguinAxisUp      { get => penguinRigidBody.transform.up;      }
    private Vector3 PenguinAxisForward { get => penguinRigidBody.transform.forward; }
    private Vector3 PenguinCenter      { get => penguinCollider.bounds.center;      }

    public void Reset()
    {
        inputAxes = Vector2.zero;
        inputMoveSpeed = walkingSpeed;
        inputRotationAngle = 0.0f;

        penguinRigidBody.rotation = inputRotationAngle;
        penguinRigidBody.velocity = inputAxes * inputMoveSpeed;
        penguinRigidBody.position = initialSpawnPosition;

        facing  = Facing.RIGHT;
        posture = Posture.STANDING;
        if (penguinRigidBody.transform.localScale.x < 0.0f)
        {
            penguinRigidBody.transform.localScale *= new Vector2(-1, 0);
        }
    }
    void Awake()
    {
        penguinRigidBody = gameObject.GetComponent<Rigidbody2D>();
        penguinCollider  = gameObject.GetComponent<BoxCollider2D>();
        initialSpawnPosition = penguinRigidBody.position;
        Reset();
    }

    void Update()
    {
        inputAxes = new Vector2(GetNormalizedInput(horizontalInputAxisName), GetNormalizedInput(verticalInputAxisName));
        if (inputAxes.x < 0 && facing == Facing.RIGHT)
        {
            penguinRigidBody.transform.localScale *= posture == Posture.STANDING? new Vector2(-1, 1) : new Vector2(1, -1);
            facing = Facing.LEFT;
        }
        else if (inputAxes.x > 0 && facing == Facing.LEFT)
        {
            penguinRigidBody.transform.localScale *= posture == Posture.STANDING ? new Vector2(-1, 1) : new Vector2(1, -1);
            facing = Facing.RIGHT;
        }

        inputRotationAngle = 0.0f;
        if (inputAxes.y < 0 && posture == Posture.STANDING)
        {
            Vector2 pivotRatio = facing == Facing.RIGHT? pivotWhenRightAndRotatingDownToLying : pivotWhenLeftAndRotatingDownToLying;
            inputRotationAngle = facing == Facing.RIGHT? -90 : 90;
            inputRotationPivot = MathUtils.GetPointInsideBounds(penguinCollider.bounds, pivotRatio);
            inputMoveSpeed     = slidingSpeed;
            posture            = Posture.LYING;
        }
        else if (inputAxes.y > 0 && posture == Posture.LYING)
        {
            Vector2 pivotRatio = facing == Facing.RIGHT? pivotWhenRightAndRotatingUpToStanding : pivotWhenLeftAndRotatingUpToStanding;
            inputRotationAngle = facing == Facing.RIGHT? 90 : -90;
            inputRotationPivot = MathUtils.GetPointInsideBounds(penguinCollider.bounds, pivotRatio);
            inputMoveSpeed     = walkingSpeed;
            posture            = Posture.STANDING;
        }
    }

    void FixedUpdate()
    {
        // todo: add checking for succeeded lying/standing, and that it is in state where it can do so...
        // ... (for example, it might not be able to right in front of a wall, so it shouldn't be toggled then)
        //
        // todo: find a way to reduce computations per frame by adding some movement/positional checks
        // ideally, once penguin sliding is adjusted for slope angles, sliding can be used to quickly travel like a sled
        // (the above will probably mean that forward/up axes will need to be rotate in the opposite direction that the penguin
        // is rotated when switching between standing and sliding states
        if (posture == Posture.STANDING)
        {
            AlignPenguinWithUpAxis(newUp: GetSurfaceNormalOfGroundRelativeToPenguin());
        }
        if (inputRotationAngle != 0.0f)
        {
            MathUtils.RotateRigidBodyAroundPointBy(
                rigidBody: penguinRigidBody,
                origin:    inputRotationPivot,
                axis:      penguinRigidBody.transform.forward,
                angle:     inputRotationAngle);
        }
        penguinRigidBody.velocity = new Vector2(inputAxes.x * inputMoveSpeed, 0);
    }

    private Vector2 GetSurfaceNormalOfGroundRelativeToPenguin(float maxRayDistance=100.0f)
    {
        RaycastHit2D hitInfo = Physics2D.Raycast(PenguinCenter,
            Vector3.down, maxRayDistance, LayerMask.GetMask("Ground"));
        return (hitInfo.transform == null) ? Vector2.up : hitInfo.normal;
    }

    private void AlignPenguinWithUpAxis(Vector3 newUp)
    {
        // we use the old forward direction of the penguin crossed with the axis we wish to align to, to get a perpendicular
        // vector pointing in or out of the screen (note unity uses the left hand system), with magnitude proportional to steepness.
        // then using our desired `up-axis` crossed with our `left` vector, we get a new forward direction of the penguin
        // that's parallel with the slope that our given up is normal to.
        Vector3 left = Vector3.Cross(penguinRigidBody.transform.forward, newUp);
        Vector3 newForward = Vector3.Cross(newUp, left);

        Quaternion oldRotation = penguinRigidBody.transform.rotation;
        Quaternion newRotation = Quaternion.LookRotation(newForward, newUp);
        penguinRigidBody.MoveRotation(Quaternion.Lerp(oldRotation, newRotation, kinematicRotationStrength));
    }

    private float GetNormalizedInput(string name)
    {
        float input = Input.GetAxisRaw(name);
        return Math.Abs(input) >= inputTolerance ? input : 0.00f;
    }
}
