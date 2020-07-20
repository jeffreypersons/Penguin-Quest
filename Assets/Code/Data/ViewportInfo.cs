﻿using UnityEngine;


// provides useful helper methods for given camera object
// * assumes orthographic camera
// * assumes a single display and eye
// * all external functionality is in world space coords unless otherwise stated
public class ViewportInfo
{
    private readonly Camera cam;

    private Vector2 _screenSize;
    private Vector3 _viewportOrigin;

    public Vector2 Min     { get; private set; }
    public Vector2 Max     { get; private set; }
    public Vector2 Size    { get; private set; }
    public Vector2 Extents { get; private set; }
    public float NearClipOffset { get => _viewportOrigin.z; }

    public ViewportInfo(Camera cam)
    {
        this.cam = cam;
        Update();
    }

    public void Update()
    {
        _screenSize = new Vector3(Screen.width, Screen.height, 0.00f);
        _viewportOrigin = cam.ViewportToWorldPoint(new Vector3(0.50f, 0.50f, cam.nearClipPlane));
        Min = cam.ViewportToWorldPoint(new Vector3(0.00f, 0.00f, cam.nearClipPlane));
        Max = cam.ViewportToWorldPoint(new Vector3(1.00f, 1.00f, cam.nearClipPlane));
        Size = Max - Min;
        Extents = Size * 0.50f;
    }
}
