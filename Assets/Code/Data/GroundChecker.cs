using UnityEngine;


// provides functionality for checking if 'ground' is directly below given point
//
// features:
// * ray and object to check from are configured via inspector and tracked by the checker
// * reassigns result each time the check function is invoked, null if ground was not detected
// * vertical offsets (from source) can be set programmatically
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
        public Vector2 point;
        public Vector2 normal;
        public float distance;
        public Contact(Vector2 point, Vector2 normal, float distance)
        {
            this.point    = point;
            this.normal   = normal;
            this.distance = distance;
        }
    }

    private const float TOLERANCE_DEFAULT =  0.30f;
    private const float TOLERANCE_MIN     =  0.05f;
    private const float TOLERANCE_MAX     = 10.00f;
    private static readonly Color RAY_HIT_COLOR_DEFAULT  = Color.green;
    private static readonly Color RAY_MISS_COLOR_DEFAULT = Color.red;

    private Vector2 origin;
    private float extraLineHeight;
    public Contact Result       { get; private set; }
    public bool WasDetected     { get; private set; }

    [Tooltip("What do we consider to be 'ground'?")]
    [SerializeField] private LayerMask groundMask = default;

    [Tooltip("How close does it need to be to be considered touching? " +
             "(this determines the max length of the raycast used for performing the ground check)")]
    [SerializeField] [Range(TOLERANCE_MIN, TOLERANCE_MAX)]
    private float toleratedHeightFromGround = TOLERANCE_DEFAULT;

    [Header("Debug Settings")]
    [Tooltip("Draw raycasts in scene view for debugging purposes")]
    [SerializeField] private bool showRaycastsInSceneView = true;

    [Tooltip("Color of ray to draw if enabled and detected ground")]
    [SerializeField] private Color rayColorIfHit = RAY_HIT_COLOR_DEFAULT;

    [Tooltip("Color of ray to draw if enabled and didn't detect ground")]
    [SerializeField] private Color rayColorIfMiss = RAY_MISS_COLOR_DEFAULT;

    public override string ToString()
    {
        Vector2 givenPoint = new Vector2(origin.x, origin.y - extraLineHeight);
        if (WasDetected)
        {
            return $"Ground detected from source position{givenPoint} {Result.distance} units " +
                   $"below source object (at point {Result.point} with normal of {Result.normal}, " +
                   $"using an origin yOffset of {extraLineHeight}";
        }
        return $"No ground detected from source position{origin} " +
               $"using an origin yOffset of {extraLineHeight}";
    }

    public void Reset()
    {
        Result = default;
        WasDetected = false;
    }
    void Awake()
    {
        Reset();
    }

    // check for ground below the our source object,
    // with some extra line height to ensure it starts just above our targeted layer if given
    public void CheckForGround(Vector2 fromPoint, float extraLineHeight=0.00f)
    {
        this.extraLineHeight = extraLineHeight;
        origin           = new Vector2(fromPoint.x, fromPoint.y + extraLineHeight);
        Vector2 terminal = new Vector2(fromPoint.x, fromPoint.y - toleratedHeightFromGround);

        Debug.Log($"from {origin} to {terminal}");
        RaycastHit2D hitInfo = Physics2D.Linecast(origin, terminal, groundMask);
        if (hitInfo && (hitInfo.distance - extraLineHeight) < toleratedHeightFromGround)
        {
            WasDetected = true;
            Result = new Contact(hitInfo.centroid, hitInfo.normal, hitInfo.distance - extraLineHeight);
        }
        else
        {
            WasDetected = false;
            Result = default;
        }

        Debug.Log(this);
        #if UNITY_EDITOR
        if (showRaycastsInSceneView && WasDetected)
        {
            Debug.DrawLine(origin, Result.point, rayColorIfHit, Time.deltaTime);
        }
        if (showRaycastsInSceneView && !WasDetected)
        {
            Debug.DrawLine(origin, origin - new Vector2(0, toleratedHeightFromGround), rayColorIfMiss, Time.deltaTime);
        }
        #endif
    }
}
