using UnityEngine;


[ExecuteAlways]
public class FollowCamController : MonoBehaviour
{
    [Header("Game object to follow")]
    [Tooltip("Subject for the camera to move in unison with")]
    [SerializeField] private Transform subject;

    [Header("Camera offset from subject")]
    [Tooltip("ie offset(0, 10, 0) the subject will be centered 10 units above the subject")]
    [SerializeField] private Vector3 offset;

    private Vector3 PositionToFollow
    {
        get => new Vector3(subject.position.x, subject.position.y, 0);
    }

    void Awake()
    {
        if (!subject)
        {
            Debug.LogError($"Subject must be provided to camera script `{GetType().Name}` - no object assigned");
        }
    }
    void Update()
    {
        transform.position = PositionToFollow + offset;
    }
}
