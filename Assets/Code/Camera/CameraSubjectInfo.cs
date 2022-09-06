using UnityEngine;


namespace PQ.Camera
{
    /*
    Positional tracking for given transform, intended for use by rendering/camera/etc scripts.

    Center and size is synced from collider if attached,
    otherwise we use transform.position and assume size to be zero.

    Note that we do not take rotation into account.

    Note that we only check for collider once, so dynamically adding colliders won't automatically
    update it.
    */
    internal class CameraSubjectInfo
    {
        private readonly Transform _subject;
        private readonly Collider2D _collider;

        public string  Name => _subject.name;
        public Vector2 Center  { get; private set; }
        public float   Depth   { get; private set; }
        public Vector2 Extents { get; private set; }

        public override string ToString() =>
            $"{GetType().Name}:{{" +
                $"name:{Name}," +
                $"center:{Center}," +
                $"size:{Extents * 2f}," +
            $"}}";

        public CameraSubjectInfo(Transform subject)
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
