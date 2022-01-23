using UnityEngine;
using PenguinQuest.Utils;


namespace PenguinQuest.Data
{
    /*
    Positional tracking for given transform, intended for use by rendering/camera/etc scripts.

    Center and size is synced from collider if attached,
    otherwise we use transform.position and assume size to be zero.
    */
    public class CameraSubjectInfo
    {
        private readonly Transform  subject;
        private readonly Collider2D collider;

        public Vector2 Center  { get; private set; }
        public Vector2 Size    { get; private set; }
        public Vector2 Extents { get; private set; }
        public Vector2 Min     { get; private set; }
        public Vector2 Max     { get; private set; }

        public bool HasSizeChangedSinceLastUpdate     { get; private set; }
        public bool HasPositionChangedSinceLastUpdate { get; private set; }
        public bool HasCollider { get => collider != null && collider.enabled; }

        
        public CameraSubjectInfo(Transform subject)
        {
            if (subject == null)
            {
                Debug.LogError($"CameraSubjectInfo : Received null subject");
            }

            this.subject  = subject;
            this.collider = ExtractColliderFromTransform(subject);

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
            Vector2 newCenter = HasCollider ? collider.bounds.center : subject.position;
            if (!MathUtils.AreComponentsEqual(newCenter, Center))
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
            Vector2 newSize = HasCollider ? collider.bounds.size : Vector3.zero;
            if (!MathUtils.AreComponentsEqual(newSize, Size))
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

        private static Collider2D ExtractColliderFromTransform(Transform transform)
        {
            return transform.TryGetComponent(out Collider2D collider) ? collider : null;
        }
    }
}
