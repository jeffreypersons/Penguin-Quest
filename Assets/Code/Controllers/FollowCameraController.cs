using UnityEngine;
using UnityEngine.SocialPlatforms;


// todo: look into adding some sort of smoothing when the camera moves
[ExecuteAlways]
public class FollowCameraController : MonoBehaviour
{
    private const float FOV_DEFAULT    =   50.00f;
    private const float FOV_MIN        =   10.00f;
    private const float FOV_MAX        =  100.00f;
    private const float OFFSET_DEFAULT =    0.00f;
    private const float OFFSET_MIN     = -100.00f;
    private const float OFFSET_MAX     =  100.00f;
    private const float ZOOM_SPEED_DEFAULT =  10.00f;
    private const float ZOOM_SPEED_MIN     =   0.01f;
    private const float ZOOM_SPEED_MAX     =  50.00f;
    private const float MOVE_SPEED_DEFAULT =  10.00f;
    private const float MOVE_SPEED_MIN     =   0.01f;
    private const float MOVE_SPEED_MAX     = 100.00f;
    private const float TARGET_DISTANCE_TOLERANCE = 0.15f;

    private Camera cam;
    private ViewportInfo viewport;
    public enum FollowMode { MoveWithSubject, MoveAfterLeavingView };

    [Header("Subject to Follow")]
    [Tooltip("Transform of subject for camera to follow (does not have to be 'visible')")]
    [SerializeField]  private Transform subject;
    [HideInInspector] private Renderer subjectRenderer;

    [Tooltip("Adjust move speed (how fast the camera follows the subject)")]
    [Range(MOVE_SPEED_MIN, MOVE_SPEED_MAX)] [SerializeField] private float moveSpeed = MOVE_SPEED_DEFAULT;

    [Header("Follow Position Relative to Subject")]
    [Tooltip("x offset from subject (subject is on left of camera if positive, right if negative)")]
    [Range(OFFSET_MIN, OFFSET_MAX)] [SerializeField] private float xOffset = OFFSET_DEFAULT;
    [Tooltip("y offset from subject (subject is bellow camera if positive, above if negative)")]
    [Range(OFFSET_MIN, OFFSET_MAX)] [SerializeField] private float yOffset = OFFSET_DEFAULT;

    [Header("Follow Behavior")]
    [Tooltip("Mode for camera movement relative to to subject")]
    [SerializeField] private FollowMode followMode = FollowMode.MoveWithSubject;
    [Tooltip("Clamp offsets to prevent subject's renderer from leaving camera viewport")]
    [SerializeField] private bool keepSubjectInView = true;
    [Tooltip("Toggle for actively following")]
    [SerializeField] private bool isActivelyRunning = true;

    [Header("Zoom Behavior")]
    [Tooltip("Adjust field of view (how 'zoomed in' the camera is)")]
    [Range(FOV_MIN, FOV_MAX)] [SerializeField] private float fieldOfView = FOV_DEFAULT;
    [Tooltip("Adjust zoom speed (how fast the camera FOV is adjusted)")]
    [Range(ZOOM_SPEED_MIN, ZOOM_SPEED_MAX)] [SerializeField] private float zoomSpeed = ZOOM_SPEED_DEFAULT;

    void Awake()
    {
        cam = gameObject.GetComponent<Camera>();
        Debug.Log("A" + cam);
        subjectRenderer = gameObject.GetComponent<Renderer>();
        if (!subject)
        {
            Debug.LogError($"No subject assigned to follow, `{GetType().Name}` - no object assigned");
        }
    }
    void Start()
    {
        viewport = new ViewportInfo(cam);
    }
    void LateUpdate()
    {
        if (!isActivelyRunning)
        {
            return;
        }
        bool hasViewportChanged = viewport.SyncChanges();
        bool hasSubjectChanged  = subject.transform.hasChanged;
        bool isAtTargetPosition    = Mathf.Abs(fieldOfView - cam.transform.position.x) <= TARGET_DISTANCE_TOLERANCE;
        bool isAtTargetFieldOfView = Mathf.Abs(fieldOfView - cam.fieldOfView) <= TARGET_DISTANCE_TOLERANCE;
        if (hasViewportChanged || !isAtTargetFieldOfView)
        {
            Debug.Log("Fov");
            cam.fieldOfView = Mathf.MoveTowards(cam.fieldOfView, fieldOfView, Time.deltaTime * zoomSpeed);
        }
        if (hasViewportChanged || !isAtTargetPosition)
        {
            Vector2 target = Vector2.MoveTowards(cam.transform.position, ComputeTargetPosition(), Time.deltaTime * moveSpeed);
            cam.transform.position = new Vector3(target.x, target.y, viewport.NearClipOffset);
            Debug.Log(cam.transform.position);
        }
    }

    // we don't worry about the case that the subject overlaps multiple sides,
    // in other words, we assume its bounds are smaller than viewport
    private Vector2 ComputeTargetPosition()
    {
        Vector2 subjectPosition;
        Vector2 subjectHalfSize;
        if (subjectRenderer == null)
        {
            subjectPosition = subject.position;
            subjectHalfSize = Vector2.zero;
        }
        else
        {
            subjectPosition = subjectRenderer.bounds.center;
            subjectHalfSize = subjectRenderer.bounds.extents;
        }

        if (keepSubjectInView)
        {
            return new Vector3(
                Mathf.Clamp(subjectPosition.x + xOffset, viewport.Min.x - subjectHalfSize.x, viewport.Max.x - subjectHalfSize.x),
                Mathf.Clamp(subjectPosition.y + yOffset, viewport.Min.y - subjectHalfSize.y, viewport.Max.y - subjectHalfSize.y));
        }
        else
        {
            return new Vector2(subjectPosition.x + xOffset, subjectPosition.y + yOffset);
        }
    }
}
