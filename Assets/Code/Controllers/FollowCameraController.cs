using UnityEngine;


// todo: look into adding some sort of smoothing when the camera moves
[ExecuteAlways]
public class FollowCameraController : MonoBehaviour
{
    private Camera cam;

    [Header("Game object to follow")]
    [Tooltip("Subject for the camera to move in unison with")]
    [SerializeField] private Transform subject;
    private Renderer subjectRenderer;

    private const float MAX_FOLLOW_DISTANCE = 100;
    [Header("Follow Settings")]
    [Tooltip("x offset from subject")]
    [SerializeField] [Range(-MAX_FOLLOW_DISTANCE, MAX_FOLLOW_DISTANCE)] private float xOffset = default;
    [Tooltip("y offset from subject")]
    [SerializeField] [Range(-MAX_FOLLOW_DISTANCE, MAX_FOLLOW_DISTANCE)] private float yOffset = default;
    [Tooltip("Prevent subject's sprite renderer from leaving the screen by clamping offsets")]
    [SerializeField] private bool keepSubjectInScreen = true;
    [Tooltip("Toggle for actively following")]
    [SerializeField] private bool isActivelyFollowing = true;

    private Vector3 screenSize;
    private Vector3 screenMin;
    private Vector3 screenMax;

    private void Reset()
    {
        // forces screensize to update on first update pass
        screenSize = Vector3.zero;
        screenMin = Vector3.zero;
        screenMax = Vector3.zero;
    }
    void Awake()
    {
        cam = gameObject.GetComponent<Camera>();
        subjectRenderer = gameObject.GetComponent<Renderer>();
        if (!subject)
        {
            Debug.LogWarning($"No subject assigned to follow, `{GetType().Name}` - no object assigned");
        }
    }
    void LateUpdate()
    {
        if (!isActivelyFollowing)
        {
            return;
        }

        if (screenSize.x != Screen.width || screenSize.y != Screen.height)
        {
            screenSize = new Vector3(Screen.width, Screen.height, 0.00f);
            screenMin = cam.ViewportToWorldPoint(new Vector3(0.00f, 0.00f, cam.nearClipPlane));
            screenMax = cam.ViewportToWorldPoint(new Vector3(1.00f, 1.00f, cam.nearClipPlane));
        }

        if (subject != null)
        {
            cam.transform.position = ComputeCameraPosition();
        }
    }

    // we don't worry about the case that the subject overlaps multiple sides,
    // in other words, we assume its bounds are smaller than viewport
    private Vector3 ComputeCameraPosition()
    {
        Vector3 subjectPosition;
        Vector3 subjectHalfSize;
        if (subjectRenderer == null)
        {
            subjectPosition = subject.position;
            subjectHalfSize = Vector3.zero;
        }
        else
        {
            subjectPosition = subjectRenderer.bounds.center;
            subjectHalfSize = subjectRenderer.bounds.extents;
        }

        if (keepSubjectInScreen)
        {
            return new Vector3(
                Mathf.Clamp(subjectPosition.x + xOffset, screenMin.x - subjectHalfSize.x, screenMax.x - subjectHalfSize.x),
                Mathf.Clamp(subjectPosition.y + yOffset, screenMin.y - subjectHalfSize.y, screenMax.y - subjectHalfSize.y),
                screenMin.z);
        }
        else
        {
            return new Vector3(subjectPosition.x + xOffset, subjectPosition.y + yOffset, screenMin.z);
        }
    }
}
