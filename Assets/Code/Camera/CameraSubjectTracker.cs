using UnityEngine;


namespace PQ.Camera
{
    /*
    World positional tracking for given transform, intended for use by rendering/camera/etc scripts.

    Center and size are taken from attached active collider, otherwise transform.position with zero size is used.
    */
    internal class CameraSubjectTracker
    {
        private readonly Transform _subject;
        private readonly Collider2D _collider;

        public string  Name => _subject == null? "null" : _subject.name;
        public Vector2 Center  { get; private set; }
        public float   Depth   { get; private set; }
        public Vector2 Extents { get; private set; }

        public override string ToString() =>
            $"{GetType().Name}:{{" +
                $"name:{Name}," +
                $"center:{Center}," +
                $"size:{Extents * 2f}}}";

        public CameraSubjectTracker(Transform subject)
        {
            if (subject == null)
            {
                Debug.LogError($"CameraSubjectInfo : Received null subject");
            }

            _subject  = subject;
            _collider = subject.GetComponent<Collider2D>();
            Update();
        }

        public void Update()
        {
            Bounds bounds = _collider ?
                _collider.bounds : new Bounds(_subject.position, Vector3.zero);

            Center  = bounds.center;
            Depth   = bounds.center.z;
            Extents = bounds.extents;
        }
    }
}
