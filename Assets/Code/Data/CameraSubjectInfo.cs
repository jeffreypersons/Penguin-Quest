using UnityEngine;


// provides useful helper methods for given game object, primarily for use by rendering/camera/etc scripts
// * if given game object has a collider, then the bounds are set to match that, otherwise it's size is zero
// * if collider:    only takes collider's center and size into account
// * if no collider: only takes transform position into account
public class CameraSubjectInfo
{
    private readonly Transform subject;
    private readonly Collider2D collider;

    public bool HasPositionChangedSinceLastUpdate { get; private set; }
    public bool HasSizeChangedSinceLastUpdate     { get; private set; }
    public bool HasCollider { get => collider; }

    public Vector2 Center  { get; private set; }
    public Vector2 Size    { get; private set; }
    public Vector2 Extents { get; private set; }
    public Vector2 Min     { get; private set; }
    public Vector2 Max     { get; private set; }

    public CameraSubjectInfo(Transform subject)
    {
        this.subject = subject;
        collider = subject.GetComponent<Collider2D>();
        Update();
        HasPositionChangedSinceLastUpdate = false;
        HasSizeChangedSinceLastUpdate     = false;
    }

    public void Update()
    {
        UpdatePosition();
        UpdateSize();

        if (HasPositionChangedSinceLastUpdate || HasSizeChangedSinceLastUpdate)
        {
            Min = Center - Extents;
            Max = Center + Extents;
        }
    }

    private void UpdatePosition()
    {
        Vector2 center = HasCollider ? collider.bounds.center : subject.position;
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
        Vector2 size = HasCollider ? collider.bounds.size : Vector3.zero;
        if (size.x != Size.x || size.y != Size.y)
        {
            Size    = size;
            Extents = size * 0.50f;
            HasSizeChangedSinceLastUpdate = true;
        }
        else
        {
            HasSizeChangedSinceLastUpdate = false;
        }
    }
}
