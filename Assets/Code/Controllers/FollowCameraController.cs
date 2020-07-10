using UnityEngine;


// todo: look into adding some sort of smoothing when the camera moves
[ExecuteAlways]
public class FollowCameraController : MonoBehaviour
{
    private Camera cam;

    [Header("Game object to follow")]
    [Tooltip("Subject for the camera to move in unison with")]
    [SerializeField] private Transform subject;

    [Header("Camera offset from subject")]
    [Tooltip("x offset from subject")]
    [SerializeField] [Range(-30, 30)] private float xOffset = default;
    [Tooltip("y offset from subject")]
    [SerializeField] [Range(-30, 30)] private float yOffset = default;
    float zOffset;

    private Vector3 Offset
    {
        get
        {
            return new Vector3(xOffset, yOffset, zOffset);
        }
    }


    private Vector3 PositionToFollow
    {
        get => new Vector3(subject.position.x, subject.position.y, 0);
    }

    void Awake()
    {
        cam = gameObject.GetComponent<Camera>();
        zOffset = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane)).z;

        if (!subject)
        {
            Debug.LogWarning($"No subject assigned to follow, `{GetType().Name}` - no object assigned");
        }
    }
    void LateUpdate()
    {
        if (subject != null && subject.transform.hasChanged)
        {
            transform.position = PositionToFollow + Offset;
        }
    }
}
