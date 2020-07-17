using UnityEngine;


// provides useful helper methods for given game object, primarily for use by rendering/camera/etc scripts
// * if given game object has a renderer, then the bounds are set to match that, otherwise it is zero
// * if render: only takes renderer center and size into account
// * if no render: only takes transform position into account
public class CameraSubjectInfo
{
    private readonly Transform subject;
    private readonly Renderer renderer;

    public bool HasRenderer { get => renderer != null; }
    public Vector2 Center   { get => HasRenderer? renderer.bounds.center  : subject.position; }
    public Vector2 Size     { get => HasRenderer? renderer.bounds.size    : Vector3.zero;     }
    public Vector2 Extents  { get => Size * 0.50f;     }
    public Vector2 Min      { get => Center - Extents; }
    public Vector2 Max      { get => Center + Extents; }

    public CameraSubjectInfo(Transform subject)
    {
        this.subject = subject;
        renderer = subject.GetComponent<Renderer>();
    }
}
