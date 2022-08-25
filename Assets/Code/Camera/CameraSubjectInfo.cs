using UnityEngine;
using System.Diagnostics.Contracts;


namespace PQ.Camera
{
    /*
    Positional tracking for given transform, intended for use by rendering/camera/etc scripts.

    Center and size is synced from collider if attached,
    otherwise we use transform.position and assume size to be zero.
    */
    internal class CameraSubjectInfo
    {
        public Vector2 Center  { get; private set; }
        public Vector2 Size    { get; private set; }
        public Vector2 Extents { get; private set; }
        public Vector2 Min     { get; private set; }
        public Vector2 Max     { get; private set; }

        public bool HasSizeChangedSinceLastUpdate     { get; private set; }
        public bool HasPositionChangedSinceLastUpdate { get; private set; }
        public bool HasCollider => _collider != null && _collider.enabled;


        private readonly Transform _subject;
        private readonly Collider2D _collider;

        public CameraSubjectInfo(Transform subject)
        {
            if (subject == null)
            {
                Debug.LogError($"CameraSubjectInfo : Received null _subject");
            }

            _subject  = subject;
            _collider = ExtractColliderFromTransform(subject);

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
            Vector2 newCenter = HasCollider ? _collider.bounds.center : _subject.position;
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
            Vector2 newSize = HasCollider ? _collider.bounds.size : Vector3.zero;
            if (!AreComponentsEqual(newSize, Size))
            {
                Size    = newSize;
                Extents = newSize * 0.50f;
                HasSizeChangedSinceLastUpdate = true;
            }
            else
            {
                HasSizeChangedSinceLastUpdate = false;
            }
        }


        [Pure] private static Collider2D ExtractColliderFromTransform(Transform transform) =>
            transform.TryGetComponent(out Collider2D collider) ? collider : null;

        [Pure] private static bool AreComponentsEqual(Vector2 a, Vector2 b) =>
            Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y);
    }
}
