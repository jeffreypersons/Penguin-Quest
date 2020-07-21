using UnityEngine;
using UnityEngine.SocialPlatforms;


// todo: look into adding some sort of smoothing when the camera moves
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
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
    private ViewportInfo viewportInfo;
    private CameraSubjectInfo subjectInfo;
    public enum FollowMode { MoveWithSubject, MoveAfterLeavingView };

    [Header("Subject to Follow")]
    [Tooltip("Transform of subject for camera to follow (does not have to be 'visible')")]
    [SerializeField]  private Transform subject;

    [Header("Follow Position Relative to Subject")]
    [Tooltip("x offset from subject (subject is on left of camera if positive, right if negative)")]
    [Range(OFFSET_MIN, OFFSET_MAX)] [SerializeField] private float xOffset = OFFSET_DEFAULT;
    [Tooltip("y offset from subject (subject is bellow camera if positive, above if negative)")]
    [Range(OFFSET_MIN, OFFSET_MAX)] [SerializeField] private float yOffset = OFFSET_DEFAULT;
    [Tooltip("z offset from subject (subject is 'into' screen camera if positive, 'out' of screen if negative)")]
    [Range(OFFSET_MIN, OFFSET_MAX)] [SerializeField] private float zOffset = OFFSET_DEFAULT;

    [Header("Follow Behavior")]
    [Tooltip("Mode for camera movement relative to to subject")]
    [SerializeField] private FollowMode followMode = FollowMode.MoveWithSubject;
    [Tooltip("Clamp offsets to prevent subject's renderer from leaving camera viewport")]
    [SerializeField] private bool keepSubjectInView = true;
    [Tooltip("Toggle for actively following")]
    [SerializeField] private bool isActivelyRunning = true;

    [Tooltip("Adjust move speed (how fast the camera follows the subject)")]
    [Range(MOVE_SPEED_MIN, MOVE_SPEED_MAX)] [SerializeField] private float moveSpeed = MOVE_SPEED_DEFAULT;

    [Header("Zoom Behavior")]
    [Tooltip("Adjust field of view (how 'zoomed in' the camera is)")]
    [Range(FOV_MIN, FOV_MAX)] [SerializeField] private float fieldOfView = FOV_DEFAULT;
    [Tooltip("Adjust zoom speed (how fast the camera FOV is adjusted)")]
    [Range(ZOOM_SPEED_MIN, ZOOM_SPEED_MAX)] [SerializeField] private float zoomSpeed = ZOOM_SPEED_DEFAULT;

    private void Init()
    {
        cam = gameObject.GetComponent<Camera>();
        cam.nearClipPlane = 0.30f;
        cam.rect = new Rect(0.00f, 0.00f, 1.00f, 1.00f);

        viewportInfo = new ViewportInfo(cam);
        subjectInfo  = new CameraSubjectInfo(subject);
    }
    void Awake()
    {
        if (!subject)
        {
            Debug.LogError($"No subject assigned to follow, `{GetType().Name}` - no object assigned");
        }
        Init();
        if (!cam.orthographic)
        {
            Debug.LogError($"Only orthographic camera mode is supported");
        }
    }

    #if UNITY_EDITOR
    void OnValidate()
    {
        if (cam != null && viewportInfo != null && subject != null && subjectInfo != null)
        {
            AdjustZoom(forceChange: true);
            AdjustPosition(forceChange: true);
        }
    }
    #endif

    void LateUpdate()
    {
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            AdjustZoom(forceChange: true);
            AdjustPosition(forceChange: true);
        }
        #endif

        if (!isActivelyRunning)
        {
            return;
        }

        AdjustZoom();
        AdjustPosition();
    }

    void AdjustZoom(bool forceChange=false)
    {
        if (forceChange)
        {
            cam.fieldOfView = fieldOfView;
            return;
        }

        if (Mathf.Abs(fieldOfView - cam.fieldOfView) > TARGET_DISTANCE_TOLERANCE)
        {
            cam.fieldOfView = Mathf.MoveTowards(cam.fieldOfView, fieldOfView, Time.deltaTime * zoomSpeed);
        }
    }
    void AdjustPosition(bool forceChange=false)
    {
        if (forceChange)
        {
            cam.transform.position = ComputeTargetPosition();
            return;
        }

        if (Mathf.Abs(subjectInfo.Center.x - cam.transform.position.x) > TARGET_DISTANCE_TOLERANCE ||
            Mathf.Abs(subjectInfo.Center.y - cam.transform.position.y) > TARGET_DISTANCE_TOLERANCE ||
            viewportInfo.HasScreenSizeChangedLastUpdate)
        {
            Vector2 target = Vector2.MoveTowards(transform.TransformVector(cam.transform.position),
                ComputeTargetPosition(), Time.deltaTime * moveSpeed);
            cam.transform.position = new Vector3(target.x, target.y, zOffset);
        }

    }
    // we don't worry about the case that the subject overlaps multiple sides,
    // in other words, we assume its bounds are smaller than viewport
    private Vector2 ComputeTargetPosition()
    {
        if (keepSubjectInView)
        {
            float leftBound  = viewportInfo.Min.x + subjectInfo.Extents.x;
            float rightBound = viewportInfo.Max.x - subjectInfo.Extents.x;
            float lowBound = viewportInfo.Min.y + subjectInfo.Extents.y;
            float upBound  = viewportInfo.Max.y - subjectInfo.Extents.y;
            Debug.Log($"min=({leftBound}, {lowBound}), max=({rightBound}, {upBound})");
            return new Vector3(
                Mathf.Clamp(subjectInfo.Center.x + xOffset, leftBound, rightBound),
                Mathf.Clamp(subjectInfo.Center.y + yOffset, lowBound,  upBound),
                zOffset);
        }
        else
        {
            return new Vector3(subjectInfo.Center.x + xOffset, subjectInfo.Center.y + yOffset, zOffset);
        }
    }
}
