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
    public class Contact
    {
        // approximate slope at area of contact (negative degrees if descending left to right, positive if ascending)
        public Vector2 tangent { get; private set; }
        public Vector2 point   { get; private set; }
        public Vector2 normal  { get; private set; }

        // (unsigned) degrees slope of the line tangent to point of contact
        public float slope                      { get; private set; }
        public Vector2 referencePoint           { get; private set; }
        public float distanceFromReferencePoint { get; private set; }

        // compute angle between given vec and surface normal within the 2d plane
        //
        // Note
        // * if vec points to the left of surface normal,  degrees are neg (vec is oriented counter-clockwise from normal)
        // * if vec points to the right of surface normal, degrees are neg (vec is oriented clockwise from normal)
        // (+ degrees: clockwise, - count-clockwise in other words, if vector points left of normal, degrees are
        public float DegreesFromSurfaceNormal(Vector2 vector)
        {
            return Vector2.SignedAngle(normal, vector);
        }
        // compute angle between given vec and surface tangent within the 2d plane
        //
        // Note
        // * if vec points to the left of surface normal,  degrees are neg (vec is oriented counter-clockwise from tangent)
        // * if vec points to the right of surface normal, degrees are neg (vec is oriented clockwise from tangent)
        // (+ degrees: clockwise, - count-clockwise in other words, if vector points left of normal, degrees are
        public float DegreesFromSurfaceTangent(Vector2 vector)
        {
            return Vector2.SignedAngle(tangent, vector);
        }

        public Contact() { }

        public void Update(Vector2 pointOfReference, Vector2 point, Vector2 normal)
        {
            bool isNormalSameAsPrevious = MathUtils.AreComponentsEqual(this.normal, normal);
            if (isNormalSameAsPrevious &&
                MathUtils.AreComponentsEqual(this.referencePoint, pointOfReference) &&
                MathUtils.AreComponentsEqual(this.point, point))
            {
                return;
            }

            this.point = point;
            this.referencePoint = pointOfReference;
            this.distanceFromReferencePoint = pointOfReference.y - point.y;
            if (!isNormalSameAsPrevious)
            {
                this.normal  = normal;
                this.tangent = MathUtils.PerpendicularClockwise(normal);
                this.slope   = MathUtils.AngleFromXAxis(this.tangent);
            }
        }
    }

    private const float TOLERANCE_DEFAULT =  0.30f;
    private const float TOLERANCE_MIN     =  0.05f;
    private const float TOLERANCE_MAX     = 10.00f;
    private static readonly Color RAY_EXTENSION_COLOR_DEFAULT     = Color.cyan;
    private static readonly Color RAY_BELOW_SOURCE_COLOR_DEFAULT  = Color.blue;
    private static readonly Color RAY_HIT_INDICATED_COLOR_DEFAULT = Color.magenta;

    private Vector2 linecastOrigin;
    private float extraLineHeight;
    private Contact _result;
    public Contact Result   { get => WasDetected? _result : default; }
    public bool WasDetected { get; private set; }
    public Vector2 SurfaceNormalOfLastContact { get; private set; }
    public float MaxDistanceFromGround { get => toleratedHeightFromGround; }

    [Tooltip("What do we consider to be 'ground'?")]
    [SerializeField] private LayerMask groundMask = default;

    [Tooltip("How close does it need to be to be considered touching? " +
             "(this determines the max length of the raycast used for performing the ground check)")]
    [SerializeField] [Range(TOLERANCE_MIN, TOLERANCE_MAX)]
    private float toleratedHeightFromGround = TOLERANCE_DEFAULT;

    [Header("Debug Settings")]
    [Tooltip("Enable drawing of visual aids in scene view to indicate raycasts and results")]
    [SerializeField] private bool displayVisualAids = true;

    [Tooltip("Color of the ray extending from y='extraLineHeight' above origin to given origin")]
    [SerializeField] private Color rayColorTop = RAY_EXTENSION_COLOR_DEFAULT;

    [Tooltip("Color of the ray extending from given origin to tolerated height below origin")]
    [SerializeField] private Color rayColorLower = RAY_BELOW_SOURCE_COLOR_DEFAULT;

    [Tooltip("Color of ray to draw perpendicular to above lines if ground detected")]
    [SerializeField] private Color rayColorBottom = RAY_HIT_INDICATED_COLOR_DEFAULT;

    public override string ToString()
    {
        if (WasDetected)
        {
            return $"Ground detected from source position{Result.referencePoint} {Result.distanceFromReferencePoint} units " +
                   $"below source object (at point {Result.point} with normal of {Result.normal}, " +
                   $"using an origin yOffset of {extraLineHeight} and with a slope of {Result.slope}";
        }
        return $"No ground detected from reference point{linecastOrigin} " +
               $"using an origin yOffset of {extraLineHeight}";
    }

    public void Reset()
    {
        _result = new Contact();
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
        linecastOrigin = new Vector2(fromPoint.x, fromPoint.y + extraLineHeight);
        Vector2 terminal = new Vector2(fromPoint.x, fromPoint.y - toleratedHeightFromGround);

        RaycastHit2D hitInfo = Physics2D.Linecast(linecastOrigin, terminal, groundMask);
        if (hitInfo && (hitInfo.distance - extraLineHeight) < toleratedHeightFromGround)
        {
            WasDetected = true;
            _result.Update(fromPoint, hitInfo.centroid, hitInfo.normal);
            SurfaceNormalOfLastContact = hitInfo.normal;
        }
        else
        {
            WasDetected = false;
        }
    }

    void OnDrawGizmos()
    {
        if (!displayVisualAids)
        {
            return;
        }

        Vector2 mid    = new Vector2(linecastOrigin.x, linecastOrigin.y - extraLineHeight);
        Vector2 bottom = new Vector2(linecastOrigin.x, mid.y - toleratedHeightFromGround);
        GizmosUtils.DrawLine(linecastOrigin, mid, rayColorTop);
        GizmosUtils.DrawLine(mid, bottom, rayColorLower);
        if (WasDetected)
        {
            Vector2 offset = new Vector2(toleratedHeightFromGround, 0);
            GizmosUtils.DrawLine(Result.point - offset, Result.point + offset, rayColorBottom);
        }
    }
}
