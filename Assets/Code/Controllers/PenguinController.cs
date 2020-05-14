using UnityEngine;


public class PenguinController : MonoBehaviour
{
    private MovementState state;
    private enum MovementState { STANDING, LYING }

    [SerializeField] private float walkingSpeed = default;
    [SerializeField] private float slidingSpeed = default;

    private float amountToRotate;
    private float inputTolerance = 0.15f;
    [SerializeField] private string horizontalInputAxisName = default;
    [SerializeField] private string verticalInputAxisName   = default;

    private Vector2 initialSpawnPosition;
    private Vector2 inputVelocity;
    private Rigidbody2D rigidBody;

    public void Reset()
    {
        state = MovementState.STANDING;
        inputVelocity      = Vector2.zero;
        rigidBody.velocity = inputVelocity;
        rigidBody.position = initialSpawnPosition;
    }
    void Awake()
    {
        rigidBody            = gameObject.transform.GetComponent<Rigidbody2D>();
        initialSpawnPosition = rigidBody.position;
        inputVelocity        = Vector2.zero;
        rigidBody.velocity   = inputVelocity;
    }
    void Update()
    {
        float yInput = Input.GetAxisRaw(verticalInputAxisName);
        if (yInput != 0)
        {
            MovementState previousState = state;
            state = (yInput < 0) ? MovementState.LYING : MovementState.STANDING;

            amountToRotate = 0.0f;
            if (state == MovementState.STANDING && previousState == MovementState.LYING)
            {
                amountToRotate = 90.0f;
            }
            if (state == MovementState.LYING && previousState == MovementState.STANDING)
            {
                amountToRotate = -90.0f;
            }
        }

        float xInput = Input.GetAxisRaw(horizontalInputAxisName);
        if (Mathf.Abs(xInput) >= inputTolerance)
        {
            // only acceptable for right now...really bad...
            // try: https://stackoverflow.com/questions/26568542/flipping-a-2d-sprite-animation-in-unity-2d in morning
            rigidBody.transform.localScale = new Vector2(xInput < 0? -2.50f : 2.50f, 2.50f);
        }

        switch (state)
        {
            case MovementState.STANDING:
                inputVelocity = new Vector2(walkingSpeed * xInput, 0);
            break;
            case MovementState.LYING:    inputVelocity = new Vector2(slidingSpeed * xInput, 0);  break;
            default:                     inputVelocity = new Vector2(0, 0);  Debug.LogError($"State({state}) is not a valid enum"); break;
        }
    }
    void FixedUpdate()
    {
        rigidBody.velocity = inputVelocity;

        /*
        // really bad...try this instead in morning: https://forum.unity.com/threads/make-object-stand-up-after-thrown-on-the-floor.158142/
        // probably need to offset y as well, too...(should probably learn about quarternions first and why euler angles dont work, too!)
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = new Quaternion (0.0F,transform.rotation.y,0.0F,transform.rotation.w);
        transform.rotation = Quaternion.Slerp(startRotation, endRotation, Time.deltaTime*2.0F);
        if (amountToRotate != 0.0f)
        {
            rigidBody.transform.Rotate(new Vector3(0, amountToRotate, 0));
        }
        */
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = new Quaternion(0.0f, transform.rotation.y, 0.0f, transform.rotation.w);
        transform.rotation = Quaternion.Slerp(startRotation, endRotation, Time.fixedDeltaTime * 2.0f);
    }
}
