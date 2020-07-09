using UnityEngine;


public class EnemyController : MonoBehaviour
{
    [SerializeField] private float horizontalSpeedAtMaxDifficulty  = default;
    [SerializeField] private float minVerticalDistanceBeforeMoving = default;

    bool wasDifficultySet = false;
    private float difficulty;
    private float horizontalSpeed;

    private Rigidbody2D   enemyBody;
    private BoxCollider2D enemyCollider;
    private Vector2 initialPosition;

    private float targetY;

    public void Reset()
    {
        enemyBody.position = initialPosition;
    }
    public void SetDifficultyLevel(float ratio)
    {
        if (MathUtils.IsWithinRange(ratio, 0, 1))
        {
            wasDifficultySet = true;
            difficulty = ratio;
        }
        else
        {
            Debug.LogError("Ai's difficulty level cannot be set to a percentage outside the range [0, 100]");
        }
    }

    void Awake()
    {
        enemyBody       = gameObject.transform.GetComponent<Rigidbody2D>();
        enemyCollider   = gameObject.transform.GetComponent<BoxCollider2D>();
        initialPosition = enemyBody.position;
    }
    void Start()
    {
        if (wasDifficultySet)
        {
            float aiHandicap = difficulty / 100.0f;
            horizontalSpeed = horizontalSpeedAtMaxDifficulty * aiHandicap;
        }
        else
        {
            Debug.LogError("Difficulty level was not set, defaulting to a 100%");
            horizontalSpeed = horizontalSpeedAtMaxDifficulty;
        }
        Reset();
    }
    void FixedUpdate()
    {
        Vector2 target = initialPosition;
        if (Mathf.Abs(target.x - enemyBody.position.x) >= minVerticalDistanceBeforeMoving)
        {
            enemyBody.position = Vector2.MoveTowards(
                enemyBody.position,
                new Vector2(enemyBody.position.x, targetY),
                horizontalSpeed * Time.fixedDeltaTime
            );
        }
    }

    void OnEnable()
    {
        GameEventCenter.enemyHit.AddListener(RespondToHit);
    }
    void OnDisable()
    {
        GameEventCenter.enemyHit.RemoveListener(RespondToHit);
    }
    public void RespondToHit(string hitZoneInfo)
    {
    }
}
