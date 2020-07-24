using UnityEngine;


// todo: might be better to configure this using a variable offset and collider
// provides functionality for checking if 'ground' is below given collider
//
// features:
// * ray and object to check from are configured via inspector and tracked by the checker
// * reassigns result each time the check function is invoked, null if ground was not detected
//
// notes:
// * all the queries and info reflect the results of the ground check, as last updated
// * assumes that the project's physics settings are configured such that the raycast
//   doesn't trigger the collider it starts in (otherwise, casting from inside ground layer will fail to get a result)
[System.Serializable]
[AddComponentMenu("GroundChecker")]
public class GroundChecker : MonoBehaviour
{
    public struct Contact
    {
        public Vector2 rayOrigin;
        public Vector2 point;
        public Vector2 normal;
        public float distance;
        public Contact(Vector2 rayOrigin, Vector2 point, Vector2 normal, float distance)
        {
            this.rayOrigin = rayOrigin;
            this.point = point;
            this.normal = normal;
            this.distance = distance;
        }
    }

    public Contact Result         { get; private set; }
    public bool WasDetected       { get; private set; }
    public float MaxRayDistance   { get; private set; }
    public Vector2 SourcePosition { get => source.TransformVector(source.position); }

    private readonly Color RAY_DEBUG_DRAW_COLOR = Color.green;
    private const float TOLERANCE_DEFAULT = 0.30f;
    private const float TOLERANCE_MIN = 0.05f;
    private const float TOLERANCE_MAX = 10.00f;
    public static readonly Vector2 RAY_DIRECTION = Vector2.down;

    [Header("Raycast Settings")]
    [Tooltip("Object to raycast from (ie skeletal model's root joint)")]
    [SerializeField] private Transform source;
    [Tooltip("Offset from raycast source (ie (0, 2) will cast from y=2 above source)")]
    [SerializeField] private Vector2 offset;

    [Tooltip("What do we consider to be 'ground'?")]
    [SerializeField] private LayerMask groundMask = default;

    [Tooltip("How close does it need to be to be considered touching? " +
            "(this determines the max length of the raycast used for performing the ground check)")]
    [SerializeField] [Range(TOLERANCE_MIN, TOLERANCE_MAX)]
    private float toleratedDistance = TOLERANCE_DEFAULT;

    [Tooltip("Draw raycasts in scene view for debugging purposes")]
    [SerializeField] private bool showRaycastsInSceneView = true;

    public override string ToString()
    {
        if (WasDetected)
        {
            return $"Ground detected from source position{Result.rayOrigin} {Result.distance} units " +
                   $"below source object (at point {Result.point} with normal of {Result.normal}";
        }
        return $"No ground detected from source position{Result.rayOrigin}";
    }

    #if UNITY_EDITOR
    void OnValidate()
    {
        MaxRayDistance = offset.magnitude + toleratedDistance;
        if (offset.y < 0.00f)
        {
            Debug.LogWarning("Negative y offsets may cause a raycast to start from the target ground layer, " +
                             "and thus possibly fail to properly detect ground");
        }
    }
    #endif

    void Awake()
    {
        Result = default;
        WasDetected = false;
        MaxRayDistance = offset.magnitude + toleratedDistance;
    }

    // check for ground below the our source object
    public void CheckForGround()
    {
        Vector2 origin = SourcePosition + offset;
        RaycastHit2D hitInfo = Physics2D.Raycast(origin, RAY_DIRECTION, MaxRayDistance, groundMask);

        WasDetected = hitInfo;
        Result = WasDetected ? new Contact(origin, hitInfo.centroid, hitInfo.normal, hitInfo.distance) : default;
        #if UNITY_EDITOR
        if (showRaycastsInSceneView)
        {
            Debug.DrawLine(origin, Result.point, RAY_DEBUG_DRAW_COLOR, Time.deltaTime);
        }
        #endif
    }
}
