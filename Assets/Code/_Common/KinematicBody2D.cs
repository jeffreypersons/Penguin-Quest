using UnityEngine;


namespace PQ.Common
{
    // todo: make this the game's core physics component source of truth, adding mass, etc
    //       this would probably entail having an info pane in the inspector that displays the same sort of info
    //       that rigidbody does, for contacts/positions/velocity/etc. If that route is taken,
    //       also consider changing this to MoveableBody with a collisions component (eg plus maybe box caster?)
    /*
    Component for any kinematic body.
    */
    [ExecuteAlways]
    [System.Serializable]
    [AddComponentMenu("KinematicBody2D")]
    public class KinematicBody2D : MonoBehaviour
    {
        private Rigidbody2D   _rigidBody;
        private BoxCollider2D _boundingBox;

        public BoxCollider2D BoundingBox => _boundingBox;

        public Vector2 Forward  => _rigidBody.transform.right.normalized;
        public Vector2 Up       => _rigidBody.transform.up.normalized;
        public Vector2 Position => _rigidBody.position;
        public float   Rotation => _rigidBody.rotation;
        public float   Depth    => _rigidBody.transform.position.z;

        public override string ToString() =>
            $"KinematicBody2D@({Position.x},{Position.y},{Depth}), " +
                $"Rotation: {Rotation}, " +
                $"Forward: {Forward}, " +
                $"Up: {Up}";


        void Awake()
        {
            _rigidBody   = gameObject.GetComponent<Rigidbody2D>();
            _boundingBox = gameObject.GetComponent<BoxCollider2D>();

            if (_rigidBody == null)
            {
                throw new MissingComponentException("Expected attached rigidbody2D - not found");
            }

            _rigidBody.isKinematic = true;
        }

        public void MoveTo(Vector2 position)    => _rigidBody.MovePosition(position);
        public void RotateTo(float degrees)     => _rigidBody.MoveRotation(degrees);
        public void MoveBy(Vector2 delta)       => _rigidBody.MovePosition(_rigidBody.position + delta);
        public void RotateBy(float degrees)     => _rigidBody.MoveRotation(_rigidBody.rotation + degrees);
        public void MoveForward(float distance) => _rigidBody.MovePosition(_rigidBody.position + (distance * Forward));

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
