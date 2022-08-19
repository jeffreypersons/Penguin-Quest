using UnityEngine;


namespace PQ.Common
{
    /*
    Convenience wrapper with a transform, optional rigidbody and bounding box.
    Note that all adjustments are done via transform.
    */
    public class GameObject2D
    {
        private Transform     _transform;
        private Rigidbody2D   _rigidBody;
        private BoxCollider2D _boundingBox;

        public Vector2   Forward     => _transform.right.normalized;
        public Vector2   Up          => _transform.up.normalized;
        public Vector2   Position    => _transform.position;
        public float     Rotation    => _transform.eulerAngles.z;
        public float     Depth       => _transform.position.z;

        public Transform     Transform   => _transform;
        public Rigidbody2D   RigidBody   => _rigidBody;
        public BoxCollider2D BoundingBox => _boundingBox;


        public GameObject2D(Transform transform)
        {
            _transform   = transform;
            _rigidBody   = transform.GetComponent<Rigidbody2D>();
            _boundingBox = transform.GetComponent<BoxCollider2D>();
        }

        public void MoveTo(Vector2 position)    => _transform.position = position;
        public void RotateTo(float degrees)     => _transform.rotation = Quaternion.Euler(0, 0, degrees);

        public void MoveBy(Vector2 delta)       => _transform.Translate(delta.x, delta.y, 0);
        public void RotateBy(float degrees)     => _transform.Rotate(0, 0, degrees);

        public void MoveForward(float distance) => _transform.Translate(distance * Forward);
        
        public void SetPosition3D(float x, float y, float z) =>
            _transform.localPosition = new Vector3(x, y, z);
        public void SetOrientation3D(float xDegrees, float yDegrees, float zDegrees) =>
            _transform.localRotation = Quaternion.Euler(xDegrees, yDegrees, zDegrees);

        public void AlignTo(Vector2 forward)
        {
            Vector2 currentDirection = Forward;
            Vector2 targetDirection  = forward.normalized;

            float degreesUnaligned = Vector2.SignedAngle(from: currentDirection, to: targetDirection);
            _transform.Rotate(0, 0, degreesUnaligned);
        }
    }
}
