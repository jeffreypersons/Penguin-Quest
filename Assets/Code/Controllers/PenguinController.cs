using UnityEngine;


public class PenguinController : MonoBehaviour
{
    [SerializeField] private float  penguinHorizontalSpeed   = default;
    [SerializeField] private string inputAxisName            = default;

    private Vector2 initialSpawnPosition;
    private Vector2 inputVelocity;
    private Rigidbody2D rigidBody;

    public void Reset()
    {
        inputVelocity       = Vector2.zero;
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
        inputVelocity = new Vector2(penguinHorizontalSpeed * Input.GetAxisRaw(inputAxisName), 0);
    }
    void FixedUpdate()
    {
        rigidBody.velocity = inputVelocity;
    }
}
