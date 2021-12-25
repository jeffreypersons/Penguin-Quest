﻿using UnityEngine;


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
[RequireComponent(typeof(Camera))]
public class FollowCameraController : MonoBehaviour
{
    public enum FollowMode { MoveWithSubject, MoveAfterLeavingView };

    private const float TARGET_DISTANCE_TOLERANCE = 0.20f;
    private const float ORTHO_SIZE_DEFAULT =     50.00f;
    private const float ORTHO_SIZE_MIN     =     15.00f;
    private const float ORTHO_SIZE_MAX     =    500.00f;
    private const float OFFSET_DEFAULT     =      0.00f;
    private const float OFFSET_MIN         =  -1000.00f;
    private const float OFFSET_MAX         =   1000.00f;
    private const float ZOOM_SPEED_DEFAULT =     10.00f;
    private const float ZOOM_SPEED_MIN     =      0.01f;
    private const float ZOOM_SPEED_MAX     =     50.00f;
    private const float MOVE_SPEED_DEFAULT =   1000.00f;
    private const float MOVE_SPEED_MIN     =     10.00f;
    private const float MOVE_SPEED_MAX     = 100000.00f;

    [Header("Subject to Follow")]
    [Tooltip("Transform of (any) subject for camera to follow (does not have to be 'visible')")]
    [SerializeField] private Transform subject;

    [Header("Follow Position Relative to Subject")]
    [Tooltip("x offset from subject (subject is on left of camera if positive, right if negative)")]
    [Range(OFFSET_MIN, OFFSET_MAX)] [SerializeField] private float xOffset = OFFSET_DEFAULT;
    [Tooltip("y offset from subject (subject is bellow camera if positive, above if negative)")]
    [Range(OFFSET_MIN, OFFSET_MAX)] [SerializeField] private float yOffset = OFFSET_DEFAULT;
    [Tooltip("z offset from subject (subject is 'into' screen camera if positive, 'out' of screen if negative)")]
    [Range(OFFSET_MIN, OFFSET_MAX)] [SerializeField] private float zOffset = OFFSET_DEFAULT;

    [Header("Follow Behavior")]
    [Tooltip("Clamp offsets to prevent subject's collider from leaving camera viewport")]
    [SerializeField] private bool keepSubjectInView = true;
    [Tooltip("Toggle for actively following")]
    [SerializeField] private bool isActivelyRunning = true;

    [Tooltip("Adjust move speed (how fast the camera follows the subject)")]
    [Range(MOVE_SPEED_MIN, MOVE_SPEED_MAX)] [SerializeField] private float maxMoveSpeed = MOVE_SPEED_DEFAULT;

    [Header("Zoom Behavior")]
    [Tooltip("Adjust orthographic size (how 'zoomed in' the camera is, by changing the viewport's half height)")]
    [Range(ORTHO_SIZE_MIN, ORTHO_SIZE_MAX)] [SerializeField] private float orthographicSize = ORTHO_SIZE_DEFAULT;
    [Tooltip("Adjust zoom speed (how fast the camera FOV is adjusted)")]
    [Range(ZOOM_SPEED_MIN, ZOOM_SPEED_MAX)] [SerializeField] private float maxZoomSpeed = ZOOM_SPEED_DEFAULT;


    private Camera             cam;
    private CameraViewportInfo viewportInfo;
    private CameraSubjectInfo  subjectInfo;

    private float   zoomVelocity;
    private Vector2 moveVelocity;

    private bool IsFullyInitialized =>
        cam          != null &&
        viewportInfo != null &&
        subject      != null &&
        subjectInfo  != null;

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

    private void Init()
    {
        cam = gameObject.GetComponent<Camera>();
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
        // warn just once on update if subject is null, to avoid logging the error each frame
        if (!subject)
        {
            Debug.LogError($"No subject assigned to follow, `{GetType().Name}` - no object assigned");
        }
        ForcedUpdate();
    }

    #if UNITY_EDITOR
    void OnValidate()
    {
        ForcedUpdate();
    }
    #endif

    void LateUpdate()
    {
        #if UNITY_EDITOR
        if (!Application.IsPlaying(this))
        {
            ForcedUpdate();
            return;
        }
        #endif

        if (isActivelyRunning)
        {
            SmoothedUpdate();
        }
    }

    private void ForcedUpdate()
    {
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
        if (!MathUtils.IsWithinTolerance(current, targetOrthoSize, TARGET_DISTANCE_TOLERANCE))
        {
            cam.orthographicSize = Mathf.SmoothDamp(current, targetOrthoSize, ref zoomVelocity, Time.deltaTime, maxZoomSpeed);
        }
    }
    private void MoveCameraTowards(Vector3 target)
    {
        Vector3 current = cam.transform.position;
        if (!MathUtils.IsWithinTolerance(current, target, TARGET_DISTANCE_TOLERANCE))
        {
            Vector2 position = Vector2.SmoothDamp(current, target, ref moveVelocity, Time.deltaTime, maxMoveSpeed);
            cam.transform.position = new Vector3(position.x, position.y, target.z);
        }
    }
}
