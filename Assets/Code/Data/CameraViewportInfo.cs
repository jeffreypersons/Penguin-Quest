using UnityEngine;


// provides useful helper methods for given camera object
// * assumes orthographic camera
// * assumes a single display and eye
// * all external functionality is in world space coords unless otherwise stated
public class CameraViewportInfo
{
    private readonly Camera cam;

    public bool  HasPositionChangedSinceLastUpdate { get; private set; }
    public bool  HasSizeChangedSinceLastUpdate     { get; private set; }
    public float NearClipOffset { get; private set; }
    public Vector2 Center       { get; private set; }
    public Vector2 Size         { get; private set; }
    public Vector2 Extents      { get; private set; }
    public Vector2 Min          { get; private set; }
    public Vector2 Max          { get; private set; }

    public CameraViewportInfo(Camera cam)
    {
        this.cam = cam;
        Update();
        HasPositionChangedSinceLastUpdate = false;
        HasSizeChangedSinceLastUpdate     = false;
    }

    public void Update()
    {
        UpdatePosition();
        UpdateSize();
    }

    private void UpdatePosition()
    {
        Vector3 center = cam.ViewportToWorldPoint(new Vector3(0.50f, 0.50f, cam.nearClipPlane));
        if (center.z != NearClipOffset)
        {
            NearClipOffset = center.z;
        }

        if (center.x != Center.x || center.y != Center.y)
        {
            Center = center;
            HasPositionChangedSinceLastUpdate = true;
        }
        else
        {
            HasPositionChangedSinceLastUpdate = false;
        }
    }
    private void UpdateSize()
    {
        Vector2 min = cam.ViewportToWorldPoint(new Vector3(0.00f, 0.00f, cam.nearClipPlane));
        Vector2 max = cam.ViewportToWorldPoint(new Vector3(1.00f, 1.00f, cam.nearClipPlane));
        if (min.x != Min.x || min.y != Min.y || max.x != Max.x || max.y != Max.y)
        {
            Min = min;
            Max = max;
            Size = max - min;
            Extents = Size * 0.50f;
            HasSizeChangedSinceLastUpdate = true;
        }
        else
        {
            HasSizeChangedSinceLastUpdate = false;
        }
    }
}
