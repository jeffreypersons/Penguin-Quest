using UnityEngine;
using System.Diagnostics.Contracts;


namespace PQ.Game.Camera
{
    /*
    Camera behavior for following a subject.

    Features
    - Option to restrict subject to camera viewport
    - Applies smoothing when following subject
    - Reduces jitter by avoids moving if super close to object
    - forces immediate updates in editor so that offsets are always kept in sync prior to game start

    Notes
    - Only orthographic mode is supported
    - Assumes camera position is center of viewport
    - Assumes subject's dimensions are less than viewport
    - The above could possibly be handled in future by changing the orthographic size
    */
    [ExecuteAlways]
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class FollowCameraController : MonoBehaviour
    {        
        [Header("Subject to Follow")]
        [Tooltip("Transform of (any) subject for camera to follow (does not have to be 'visible')")]
        [SerializeField] private Transform _subject;

        
        [Header("Follow Position Relative to Subject")]
        [Tooltip("x offset from subject (subject is on left of camera if positive, right if negative)")]
        [Range(-1000.00f, 1000.00f)] [SerializeField] private float _xOffset = 0.00f;

        [Tooltip("y offset from subject (subject is bellow camera if positive, above if negative)")]
        [Range(-1000.00f, 1000.00f)] [SerializeField] private float _yOffset = 0.00f;

        [Tooltip("z offset from subject (subject is 'into' screen camera if positive, 'out' of screen if negative)")]
        [Range(-1000.00f, 1000.00f)] [SerializeField] private float _zOffset = 0.00f;
        

        [Header("Follow Behavior")]
        [Tooltip("Toggle for actively following")]
        [SerializeField] private bool _isActivelyRunning = true;

        [Tooltip("Should we clamp offsets to prevent subject's collider from leaving camera viewport")]
        [SerializeField] private bool _keepSubjectInView = true;
        
        [Tooltip("How fast can the camera follow the subject?")]
        [Range(10.00f, 10000.00f)] [SerializeField] private float _maxMoveSpeed = 1000.00f;

        [Tooltip("How far can the subject be from the camera before we update our position?")]
        [Range(0.01f, 100.00f)] [SerializeField] private float _distanceFromTargetPositionThreshold = 0.20f;


        [Header("Zoom Settings")]
        [Tooltip("Adjust orthographic size (how 'zoomed in' the camera is, by changing the viewport's half height)")]
        [Range(15.00f, 500.0f)] [SerializeField] private float _orthographicSize = 50.00f;

        [Tooltip("How fast can the camera's field of view be adjusted?")]
        [Range(0.10f, 50.00f)] [SerializeField] private float _maxZoomSpeed = 10.00f;

        [Tooltip("How sensitive to adjustments in zoom are we?")]
        [Range(0.01f, 100.00f)] [SerializeField] private float _differenceFromTargetOrthoSizeThreshold = 0.20f;
        
        public override string ToString() =>
            $"{GetType().Name}:{{" +
                $"name:{_cam.name}," +
                $"orthoSize:{_orthographicSize}," +
                $"maxMoveSpeed:{_maxMoveSpeed}," +
                $"maxZoomSpeed:{_maxZoomSpeed}," +
                $"keepSubjectInView:{_keepSubjectInView}, " +
                $"offsetFromSubject:{OffsetFromSubject}, " +
                $"viewport:{_viewportInfo}, " +
                $"subject:{_subjectInfo}}}";


        private UnityEngine.Camera _cam;
        private CameraViewportTracker _viewportInfo;
        private CameraSubjectTracker  _subjectInfo;

        private float   _zoomVelocity;
        private Vector2 _moveVelocity;
        
        private bool IsFullyInitialized =>
            _cam          != null &&
            _viewportInfo != null &&
            _subject      != null &&
            _subjectInfo  != null;

        private Vector3 SubjectPosition => _subjectInfo.Center;
        private Vector3 OffsetFromSubject
        {
            get
            {
                if (_keepSubjectInView)
                {
                    Vector2 maxOffsetInsideViewport = _viewportInfo.Extents - _subjectInfo.Extents;
                    return new Vector3(
                        x: Mathf.Clamp(_xOffset, -maxOffsetInsideViewport.x, maxOffsetInsideViewport.x),
                        y: Mathf.Clamp(_yOffset, -maxOffsetInsideViewport.y, maxOffsetInsideViewport.y),
                        z: _zOffset);
                }
                else
                {
                    return new Vector3(_xOffset, _yOffset, _zOffset);
                }
            }
        }

        private void Init()
        {
            _cam = gameObject.GetComponent<UnityEngine.Camera>();
            _cam.nearClipPlane = 0.30f;
            _cam.rect          = new Rect(0.00f, 0.00f, 1.00f, 1.00f);
            _cam.orthographic  = true;

            _viewportInfo = new CameraViewportTracker(_cam);
            _subjectInfo  = new CameraSubjectTracker(_subject);

            _zoomVelocity = 0.00f;
            _moveVelocity = Vector2.zero;
        }
    
        void Awake()
        {
            ForcedUpdate();
        }

        void LateUpdate()
        {
            #if UNITY_EDITOR
            if (!Application.IsPlaying(this) && _subject)
            {
                ForcedUpdate();
                return;
            }
            #endif

            if (_isActivelyRunning && _subject)
            {
                SmoothedUpdate();
            }
        }
        

        private void ForcedUpdate()
        {
            // warn just once on update if subject is null, to avoid logging the error each frame
            if (!_subject)
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

            _viewportInfo.Update();
            _subjectInfo.Update();
            _cam.orthographicSize   = _orthographicSize;
            _cam.transform.position = SubjectPosition + OffsetFromSubject;
        }

        private void SmoothedUpdate()
        {
            _viewportInfo.Update();
            _subjectInfo.Update();
            AdjustZoomTowards(_orthographicSize);
            MoveCameraTowards(SubjectPosition + OffsetFromSubject);
        }


        private void AdjustZoomTowards(float targetOrthoSize)
        {
            float current = _cam.orthographicSize;
            if (!IsWithinTolerance(current, targetOrthoSize, _differenceFromTargetOrthoSizeThreshold))
            {
                _cam.orthographicSize = Mathf.SmoothDamp(current, targetOrthoSize, ref _zoomVelocity,
                    Time.deltaTime, _maxZoomSpeed);
            }
        }

        private void MoveCameraTowards(Vector3 target)
        {
            Vector3 current = _cam.transform.position;
            if (!IsWithinTolerance(current, target, _distanceFromTargetPositionThreshold))
            {
                Vector2 position = Vector2.SmoothDamp(current, target, ref _moveVelocity, Time.deltaTime, _maxMoveSpeed);
                _cam.transform.position = new Vector3(position.x, position.y, target.z);
            }
        }


        [Pure] private static bool IsWithinTolerance(float a, float b, float tolerance) =>
            Mathf.Abs(b - a) <= tolerance;

        [Pure] private static bool IsWithinTolerance(Vector2 a, Vector2 b, float tolerance) =>
            Mathf.Abs(b.x - a.x) <= tolerance && Mathf.Abs(b.y - a.y) <= tolerance;
    }
}
