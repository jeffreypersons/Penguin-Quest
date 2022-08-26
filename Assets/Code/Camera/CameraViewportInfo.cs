using UnityEngine;
using System.Diagnostics.Contracts;


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
        public float   NearClipOffset { get; private set; }
        public Vector2 Center         { get; private set; }
        public Vector2 Size           { get; private set; }
        public Vector2 Extents        { get; private set; }
        public Vector2 Min            { get; private set; }
        public Vector2 Max            { get; private set; }


        public bool HasSizeChangedSinceLastUpdate     { get; private set; }
        public bool HasPositionChangedSinceLastUpdate { get; private set; }


        private readonly UnityEngine.Camera _cam;

        public CameraViewportInfo(UnityEngine.Camera cam)
        {
            if (cam == null)
            {
                Debug.LogError($"CameraViewportInfo : Received null camera");
            }

            _cam = cam;
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
            Vector3 newCenter = _cam.ViewportToWorldPoint(new Vector3(0.50f, 0.50f, _cam.nearClipPlane));
            if (!Mathf.Approximately(newCenter.z, NearClipOffset))
            {
                NearClipOffset = newCenter.z;
            }
        
            if (!AreComponentsEqual(newCenter, Center))
            {
                Center = newCenter;
                HasPositionChangedSinceLastUpdate = true;
            }
            else
            {
                HasPositionChangedSinceLastUpdate = false;
            }
        }

        private void UpdateSize()
        {
            Vector2 newMin = _cam.ViewportToWorldPoint(new Vector3(0.00f, 0.00f, _cam.nearClipPlane));
            Vector2 newMax = _cam.ViewportToWorldPoint(new Vector3(1.00f, 1.00f, _cam.nearClipPlane));
        
            if (!AreComponentsEqual(newMin, Min) || !AreComponentsEqual(newMax, Max))
            {
                Min     = newMin;
                Max     = newMax;
                Size    = newMax - newMin;
                Extents = Size * 0.50f;
                HasSizeChangedSinceLastUpdate = true;
            }
            else
            {
                HasSizeChangedSinceLastUpdate = false;
            }
        }


        [Pure] private static bool AreComponentsEqual(Vector2 a, Vector2 b) =>
            Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y);
    }
}
