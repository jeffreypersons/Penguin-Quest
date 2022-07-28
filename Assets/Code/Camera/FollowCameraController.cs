using UnityEngine;
using PQ.Common.Extensions;


namespace PQ.Camera
{
    /*
    Camera behavior for following a subject.

    Features
    - option to restrict subject to camera viewport
    - applies smoothing when following subject
    - reduces jitter by avoids moving if super close to object
    - forces immediate updates in editor so that offsets are always kept in sync prior to game start

    Notes
    - only orthographic mode is supported
    - assumes camera position is center of viewport
    - assumes subject's dimensions are less than viewport
    - the above could possibly be handled in future by changing the orthographic size
    */
    [ExecuteAlways]
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class FollowCameraController : MonoBehaviour
    {        
        [Header("Subject to Follow")]
        [Tooltip("Transform of (any) subject for camera to follow (does not have to be 'visible')")]
        [SerializeField] private Transform subject;

        
        [Header("Follow Position Relative to Subject")]
        [Tooltip("x offset from subject (subject is on left of camera if positive, right if negative)")]
        [Range(-1000.00f, 1000.00f)] [SerializeField] private float xOffset = 0.00f;

        [Tooltip("y offset from subject (subject is bellow camera if positive, above if negative)")]
        [Range(-1000.00f, 1000.00f)] [SerializeField] private float yOffset = 0.00f;

        [Tooltip("z offset from subject (subject is 'into' screen camera if positive, 'out' of screen if negative)")]
        [Range(-1000.00f, 1000.00f)] [SerializeField] private float zOffset = 0.00f;
        

        [Header("Follow Behavior")]
        [Tooltip("Toggle for actively following")]
        [SerializeField] private bool isActivelyRunning = true;

        [Tooltip("Should we clamp offsets to prevent subject's collider from leaving camera viewport")]
        [SerializeField] private bool keepSubjectInView = true;
        
        [Tooltip("How fast can the camera follow the subject?")]
        [Range(10.00f, 10000.00f)] [SerializeField] private float maxMoveSpeed = 1000.00f;

        [Tooltip("How far can the subject be from the camera before we update our position?")]
        [Range(0.01f, 100.00f)] [SerializeField] private float distanceFromTargetPositionThreshold = 0.20f;


        [Header("Zoom Settings")]
        [Tooltip("Adjust orthographic size (how 'zoomed in' the camera is, by changing the viewport's half height)")]
        [Range(15.00f, 500.0f)] [SerializeField] private float orthographicSize = 50.00f;

        [Tooltip("How fast can the camera's field of view be adjusted?")]
        [Range(0.10f, 50.00f)] [SerializeField] private float maxZoomSpeed = 10.00f;

        [Tooltip("How sensitive to adjustments in zoom are we?")]
        [Range(0.01f, 100.00f)] [SerializeField] private float differenceFromTargetOrthoSizeThreshold = 0.20f;


        private UnityEngine.Camera cam;
        private CameraViewportInfo viewportInfo;
        private CameraSubjectInfo  subjectInfo;

        private float   zoomVelocity;
        private Vector2 moveVelocity;


        private Vector3 SubjectPosition
        {
            get
            {
                return subjectInfo.Center;
            }
        }
        private Vector3 OffsetFromSubject
        {
            get
            {
                if (keepSubjectInView)
                {
                    Vector2 maxOffsetInsideViewport = viewportInfo.Extents - subjectInfo.Extents;
                    return new Vector3(
                        x: Mathf.Clamp(xOffset, -maxOffsetInsideViewport.x, maxOffsetInsideViewport.x),
                        y: Mathf.Clamp(yOffset, -maxOffsetInsideViewport.y, maxOffsetInsideViewport.y),
                        z: zOffset);
                }
                else
                {
                    return new Vector3(xOffset, yOffset, zOffset);
                }
            }
        }
        private bool IsFullyInitialized =>
            cam          != null &&
            viewportInfo != null &&
            subject      != null &&
            subjectInfo  != null;


        private void Init()
        {
            cam = gameObject.GetComponent<UnityEngine.Camera>();
            cam.nearClipPlane = 0.30f;
            cam.rect          = new Rect(0.00f, 0.00f, 1.00f, 1.00f);
            cam.orthographic  = true;

            viewportInfo = new CameraViewportInfo(cam);
            subjectInfo  = new CameraSubjectInfo(subject);

            zoomVelocity = 0.00f;
            moveVelocity = Vector2.zero;
        }
    
        void Awake()
        {
            ForcedUpdate();
        }

        void LateUpdate()
        {
            #if UNITY_EDITOR
            if (!Application.IsPlaying(this) && subject)
            {
                ForcedUpdate();
                return;
            }
            #endif

            if (isActivelyRunning && subject)
            {
                SmoothedUpdate();
            }
        }
        

        private void ForcedUpdate()
        {
            // warn just once on update if subject is null, to avoid logging the error each frame
            if (!subject)
            {
                Debug.LogError($"FollowCameraController : No subject assigned to follow");
                return;
            }

            // force reinitialization if anything became null,
            // since sometimes the references are modified after script recompilation in editor
            if (!IsFullyInitialized)
            {
                Init();
            }

            viewportInfo.Update();
            subjectInfo.Update();
            cam.orthographicSize   = orthographicSize;
            cam.transform.position = SubjectPosition + OffsetFromSubject;
        }

        private void SmoothedUpdate()
        {
            viewportInfo.Update();
            subjectInfo.Update();
            AdjustZoomTowards(orthographicSize);
            MoveCameraTowards(SubjectPosition + OffsetFromSubject);
        }


        private void AdjustZoomTowards(float targetOrthoSize)
        {
            float current = cam.orthographicSize;
            if (!MathExtensions.IsWithinTolerance(current, targetOrthoSize, differenceFromTargetOrthoSizeThreshold))
            {
                cam.orthographicSize = Mathf.SmoothDamp(current, targetOrthoSize, ref zoomVelocity,
                    Time.deltaTime, maxZoomSpeed);
            }
        }

        private void MoveCameraTowards(Vector3 target)
        {
            Vector3 current = cam.transform.position;
            if (!MathExtensions.IsWithinTolerance(current, target, distanceFromTargetPositionThreshold))
            {
                Vector2 position = Vector2.SmoothDamp(current, target, ref moveVelocity, Time.deltaTime, maxMoveSpeed);
                cam.transform.position = new Vector3(position.x, position.y, target.z);
            }
        }
    }
}
