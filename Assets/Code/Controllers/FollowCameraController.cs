﻿using UnityEngine;
using UnityEngine.SocialPlatforms;


// features:
// * option to restrict subject to camera viewport
// * applies smoothing when following subject
// * reduces jitter by avoids moving if super close to object
// * forces immediate updates in editor so that offsets are always kept in sync prior to game start
//
// notes:
// * assumes camera position is center of viewport
//
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class FollowCameraController : MonoBehaviour
{
    private const float FOV_DEFAULT    =   50.00f;
    private const float FOV_MIN        =   10.00f;
    private const float FOV_MAX        =  100.00f;
    private const float OFFSET_DEFAULT =    0.00f;
    private const float OFFSET_MIN     = -1000.00f;
    private const float OFFSET_MAX     =  1000.00f;
    private const float ZOOM_SPEED_DEFAULT =   10.00f;
    private const float ZOOM_SPEED_MIN     =    0.01f;
    private const float ZOOM_SPEED_MAX     =   50.00f;
    private const float MOVE_SPEED_DEFAULT =  100.00f;
    private const float MOVE_SPEED_MIN     =   10.00f;
    private const float MOVE_SPEED_MAX     =  500.00f;
    private const float TARGET_DISTANCE_TOLERANCE = 0.15f;

    private Camera cam;
    private CameraViewportInfo viewportInfo;
    private CameraSubjectInfo subjectInfo;
    private float zoomVelocity;
    private Vector2 moveVelocity;
    private Vector3 normalizedOffsets;
    public enum FollowMode { MoveWithSubject, MoveAfterLeavingView };

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
    [Tooltip("Adjust field of view (how 'zoomed in' the camera is)")]
    [Range(FOV_MIN, FOV_MAX)] [SerializeField] private float fieldOfView = FOV_DEFAULT;
    [Tooltip("Adjust zoom speed (how fast the camera FOV is adjusted)")]
    [Range(ZOOM_SPEED_MIN, ZOOM_SPEED_MAX)] [SerializeField] private float maxZoomSpeed = ZOOM_SPEED_DEFAULT;

    private bool IsFullyInitialized
    {
        get => cam != null && viewportInfo != null && subject != null && subjectInfo != null;
    }
    private void Init()
    {
        cam = gameObject.GetComponent<Camera>();
        cam.nearClipPlane = 0.30f;
        cam.rect = new Rect(0.00f, 0.00f, 1.00f, 1.00f);

        viewportInfo = new CameraViewportInfo(cam);
        subjectInfo  = new CameraSubjectInfo(subject);

        zoomVelocity = 0.00f;
        moveVelocity = Vector2.zero;
    }
    private void ForceUpdate()
    {
        // force reinitialization since sometimes the references are modified after recompilation
        if (!IsFullyInitialized)
        {
            Init();
        }
        AdjustViewBounds(forceChange: true);
        AdjustZoom(forceChange: true);
        AdjustPosition(forceChange: true);
    }

    void Awake()
    {
        if (!subject)
        {
            Debug.LogError($"No subject assigned to follow, `{GetType().Name}` - no object assigned");
        }
        if (!cam.orthographic)
        {
            Debug.LogError($"Only orthographic camera mode is supported");
        }
        ForceUpdate();
    }

    #if UNITY_EDITOR
    void OnValidate()
    {
        ForceUpdate();
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
        AdjustZoom();
        AdjustViewBounds();
        AdjustPosition();
    }

    private void AdjustZoom(bool forceChange=false)
    {
        if (forceChange)
        {
            cam.fieldOfView = fieldOfView;
            return;
        }

        float current = cam.fieldOfView;
        float target  = fieldOfView;
        if (!IsTooClose(current, target))
        {
            cam.fieldOfView = Mathf.SmoothDamp(current, target, ref zoomVelocity, Time.deltaTime, maxZoomSpeed);
        }
    }
    private void AdjustViewBounds(bool forceChange=false)
    {
        viewportInfo.Update();
        subjectInfo.Update();

        if (keepSubjectInView &&
            (forceChange || viewportInfo.HasSizeChangedSinceLastUpdate || subjectInfo.HasSizeChangedSinceLastUpdate))
        {
            Vector2 limit = viewportInfo.Extents - subjectInfo.Extents;
            normalizedOffsets = new Vector3(x: Mathf.Clamp(xOffset, -limit.x, limit.x),
                                            y: Mathf.Clamp(yOffset, -limit.y, limit.y),
                                            z: zOffset);
        }
        else if (forceChange ||
                 xOffset != normalizedOffsets.x || yOffset != normalizedOffsets.y || yOffset != normalizedOffsets.x)
        {
            normalizedOffsets = new Vector3(xOffset, yOffset, zOffset);
        }
    }
    private void AdjustPosition(bool forceChange=false)
    {
        Vector3 target = ComputeTarget();
        if (forceChange)
        {
            cam.transform.position = ComputeTarget();
            return;
        }

        Vector3 current = transform.TransformVector(cam.transform.position);
        if (!IsTooClose(current, target) ||
            ((keepSubjectInView && viewportInfo.HasSizeChangedSinceLastUpdate || subjectInfo.HasPositionChangedSinceLastUpdate)))
        {
            Vector2 position = Vector2.SmoothDamp(current, target, ref moveVelocity, Time.deltaTime, maxMoveSpeed);
            cam.transform.position = new Vector3(position.x, position.y, target.z);
        }
    }

    // compute displacements needed to constrain the subject bounds to the viewport
    // assumes subject bounds are smaller than viewport
    // note that if offset is zero, then no adjustments can be done since its already centered
    private Vector3 ComputeTarget()
    {
        return new Vector3(x: subjectInfo.Center.x + normalizedOffsets.x,
                           y: subjectInfo.Center.y + normalizedOffsets.y,
                           z: normalizedOffsets.z);
    }

    private static bool IsTooClose(float current, float target)
    {
        return Mathf.Abs(target - current) <= TARGET_DISTANCE_TOLERANCE;
    }
    private static bool IsTooClose(Vector2 current, Vector2 target)
    {
        return Mathf.Abs(target.x - current.x) <= TARGET_DISTANCE_TOLERANCE &&
               Mathf.Abs(target.y - current.y) <= TARGET_DISTANCE_TOLERANCE;
    }
}
