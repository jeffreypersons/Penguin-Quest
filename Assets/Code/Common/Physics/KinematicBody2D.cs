using UnityEngine;


namespace PQ.Common.Physics
{
    /*
    Wrapper over rigidbody, colliders, and transforms to form a single source of truth for 2D movement.

    Note that no caching is done - that is up to any client code.
    */
    [ExecuteAlways]
    [System.Serializable]
    [AddComponentMenu("KinematicBody2D")]
    public sealed class KinematicBody2D : MonoBehaviour
    {
        private Rigidbody2D _rigidBody;
        private Collider2D _collider;

        public Vector2 Forward       => _rigidBody.transform.right.normalized;
        public Vector2 Up            => _rigidBody.transform.up.normalized;
        public Vector2 Position      => _rigidBody.position;
        public float   Rotation      => _rigidBody.rotation;
        public float   Depth         => _rigidBody.transform.position.z;

        public Vector2 BoundsCenter  => _collider.bounds.center;
        public Vector2 BoundsExtents => _collider ? _collider.bounds.extents : Vector2.zero;

        public override string ToString() =>
            $"KinematicBody2D@({Position.x},{Position.y},{Depth}), " +
                $"Rotation: {Rotation}, " +
                $"Forward: {Forward}, " +
                $"Up: {Up}";


        void Awake()
        {
            _rigidBody = gameObject.GetComponent<Rigidbody2D>();
            _collider  = gameObject.GetComponent<Collider2D>();
            if (_rigidBody == null)
            {
                throw new MissingComponentException("Expected attached rigidbody2D - not found");
            }

            _rigidBody.isKinematic = true;
        }

        public bool IsTouching(ContactFilter2D contactFilter) => _rigidBody.IsTouching(contactFilter);
        public void MoveTo(Vector2 position) => _rigidBody.MovePosition(position);
        public void RotateTo(float degrees)  => _rigidBody.MoveRotation(degrees);
        public void MoveBy(Vector2 delta)    => _rigidBody.MovePosition(_rigidBody.position + delta);
        public void RotateBy(float degrees)  => _rigidBody.MoveRotation(_rigidBody.rotation + degrees);

        public void SetLocalOrientation3D(float xDegrees, float yDegrees, float zDegrees) =>
            _rigidBody.transform.localEulerAngles = new Vector3(xDegrees, yDegrees, zDegrees);

        public void AlignTo(Vector2 forward)
        {
            Vector2 currentDirection = Forward;
            Vector2 targetDirection  = forward.normalized;

            float degreesUnaligned = Vector2.SignedAngle(from: currentDirection, to: targetDirection);
            _rigidBody.MoveRotation(degreesUnaligned);
        }
    }
}
