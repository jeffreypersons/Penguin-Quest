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
    [Tooltip("Mode for camera movement relative to to subject")]
    [SerializeField] private FollowMode followMode = FollowMode.MoveWithSubject;
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
    private Vector3 normalizedOffsets;

    private bool IsFullyInitialized =>
        cam          != null &&
        viewportInfo != null &&
        subject      != null &&
        subjectInfo  != null;

    private void Init()
    {
        cam = gameObject.GetComponent<Camera>();
        cam.nearClipPlane = 0.30f;
        cam.rect          = new Rect(0.00f, 0.00f, 1.00f, 1.00f);
        cam.orthographic  = true;

        viewportInfo = new CameraViewportInfo(cam);
        subjectInfo  = new CameraSubjectInfo(subject);

        zoomVelocity      = 0.00f;
        moveVelocity      = Vector2.zero;
        normalizedOffsets = Vector2.zero;
    }
    private void ForceUpdate()
    {
        // force reinitialization since sometimes the references are modified after recompilation
        if (!IsFullyInitialized)
        {
            Init();
        }

        viewportInfo.Update();
        subjectInfo.Update();
        AdjustZoom_forced();
        AdjustOffsets_forced();
        AdjustPosition_forced();
    }

    void Awake()
    {
        if (!subject)
        {
            Debug.LogError($"No subject assigned to follow, `{GetType().Name}` - no object assigned");
        }
        ForceUpdate();
    }

    #if UNITY_EDITOR
    void OnValidate()
    {
        ForceUpdate();
        if (followMode != FollowMode.MoveWithSubject)
        {
            Debug.LogWarning("Only `FollowMode.MoveWithSubject` mode is currently supported");
            followMode = FollowMode.MoveWithSubject;
        }
    }
    #endif

    void LateUpdate()
    {
        #if UNITY_EDITOR
        if (!Application.IsPlaying(this))
        {
            ForceUpdate();
            return;
        }
        #endif

        if (!isActivelyRunning)
        {
            return;
        }

        viewportInfo.Update();
        subjectInfo.Update();
        AdjustZoom();
        AdjustOffsets();
        AdjustPosition();
    }

    private void AdjustZoom_forced()
    {
        cam.orthographicSize = orthographicSize;
    }

    private void AdjustZoom()
    {
        float current = cam.orthographicSize;
        float target  = orthographicSize;
        if (Mathf.Abs(target - current) > TARGET_DISTANCE_TOLERANCE)
        {
            cam.orthographicSize = Mathf.SmoothDamp(current, target, ref zoomVelocity, Time.deltaTime, maxZoomSpeed);
        }
    }

    private void AdjustOffsets_forced()
    {
        if (keepSubjectInView)
        {
            Vector2 limit = viewportInfo.Extents - subjectInfo.Extents;
            normalizedOffsets = new Vector3(
                x: Mathf.Clamp(xOffset, -limit.x, limit.x),
                y: Mathf.Clamp(yOffset, -limit.y, limit.y),
                z: zOffset
            );
        }
        else
        {
            normalizedOffsets = new Vector3(xOffset, yOffset, zOffset);
        }
    }
    private void AdjustOffsets()
    {
        if (keepSubjectInView &&
            (viewportInfo.HasSizeChangedSinceLastUpdate || subjectInfo.HasSizeChangedSinceLastUpdate))
        {
            Vector2 limit = viewportInfo.Extents - subjectInfo.Extents;
            normalizedOffsets = new Vector3(
                x: Mathf.Clamp(xOffset, -limit.x, limit.x),
                y: Mathf.Clamp(yOffset, -limit.y, limit.y),
                z: zOffset
            );
        }
        else if (xOffset != normalizedOffsets.x || yOffset != normalizedOffsets.y)
        {
            normalizedOffsets = new Vector3(xOffset, yOffset, zOffset);
        }
    }

    private void AdjustPosition_forced()
    {
        cam.transform.position = new Vector3(
            x: subjectInfo.Center.x + normalizedOffsets.x,
            y: subjectInfo.Center.y + normalizedOffsets.y,
            z: normalizedOffsets.z
        );
    }

    private void AdjustPosition()
    {
        Vector3 current = transform.TransformVector(cam.transform.position);
        Vector3 target = new Vector3(
            x: subjectInfo.Center.x + normalizedOffsets.x,
            y: subjectInfo.Center.y + normalizedOffsets.y,
            z: normalizedOffsets.z
        );

        if (Mathf.Abs(target.x - current.x) > TARGET_DISTANCE_TOLERANCE ||
            Mathf.Abs(target.y - current.y) > TARGET_DISTANCE_TOLERANCE)
        {
            Vector2 position = Vector2.SmoothDamp(current, target, ref moveVelocity, Time.deltaTime, maxMoveSpeed);
            cam.transform.position = new Vector3(position.x, position.y, target.z);
        }
    }
}
