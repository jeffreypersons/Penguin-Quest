using UnityEngine;


namespace PQ.Camera
{
    /*
    Camera viewport tracking for given camera.
    
    Assumptions
    - assumes orthographic camera
    - assumes a single display and eye
    - all external functionality is in world space coords unless otherwise stated
    */
    internal class CameraViewportInfo
    {
        private readonly UnityEngine.Camera _cam;

        public Vector2 Center         { get; private set; }
        public Vector2 Extents        { get; private set; }
        public float   NearClipOffset { get; private set; }

        public override string ToString() =>
            $"{GetType().Name}:{{" +
                $"center:{Center}," +
                $"nearClip:{NearClipOffset}," +
                $"size:{Extents * 2f}," +
            $"}}";

        public CameraViewportInfo(UnityEngine.Camera cam)
        {
            if (cam == null)
            {
                Debug.LogError($"CameraViewportInfo : Received null camera");
            }

            _cam = cam;
            Update();
        }

        public void Update()
        {
            Bounds bounds = ExtractViewportBounds(_cam);

            Center         = bounds.center;
            NearClipOffset = bounds.center.z;
            Extents        = bounds.extents;
        }


        private static Bounds ExtractViewportBounds(UnityEngine.Camera cam)
        {
            float nearClipPlaneDistance = cam.nearClipPlane;
            Vector3 position = cam.ViewportToWorldPoint(new Vector3(0.50f, 0.50f, nearClipPlaneDistance));
            Vector3 min      = cam.ViewportToWorldPoint(new Vector3(0.00f, 0.00f, nearClipPlaneDistance));
            Vector3 max      = cam.ViewportToWorldPoint(new Vector3(1.00f, 1.00f, nearClipPlaneDistance));

            return new Bounds(center: position, size: max - min);
        }
    }
}
