using UnityEngine;


namespace PQ.Common.Physics
{
    /*
    Wrapper over transform with convenience methods for 2D.
    */
    public class Transform2D
    {
        private Transform _transform;

        public Vector2   Forward     => _transform.right.normalized;
        public Vector2   Up          => _transform.up.normalized;
        public Vector2   Position    => _transform.position;
        public float     Rotation    => _transform.eulerAngles.z;
        public float     Depth       => _transform.position.z;
        public Transform Transform3D => _transform;

        public Transform2D(Transform transform)
        {
            _transform = transform;
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
