using UnityEngine;
using PQ.Common.Collisions;
using PQ.Entities.Penguin;


namespace PQ.Common
{
    public class CharacterController2D : MonoBehaviour
    {
        // todo: get rid of penguin entity and use dependency injection or something as this should be more generic
        private PenguinEntity _penguinEntity;
        private CollisionChecker _groundChecker;
        public CharacterController2DSettings Settings { get; set; }

        private void Reset()
        {
            _penguinEntity.Rigidbody.MoveRotation(ComputeOrientationForGivenUpAxis(_penguinEntity.Rigidbody, Vector2.up));
        }

        void Awake()
        {
            // todo: replace ground checker with a 2d character controller that reports surroundings,
            //       and will be a property of penguinEntity
            _groundChecker = gameObject.GetComponent<CollisionChecker>();
            _penguinEntity = gameObject.GetComponent<PenguinEntity>();
            Reset();
        }

        void Update()
        {
            _penguinEntity.Animation.SetParamIsGrounded(_groundChecker.IsGrounded);
        }


        void FixedUpdate()
        {
            if (!_groundChecker.IsGrounded)
            {
                _penguinEntity.Rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
                AlignPenguinWithGivenUpAxis(Vector2.up);
                return;
            }

            _penguinEntity.Rigidbody.constraints = RigidbodyConstraints2D.None;
            if (Settings.MaintainPerpendicularityToSurface)
            {
                // keep our penguin perpendicular to the surface at all times if option enabled
                AlignPenguinWithGivenUpAxis(_groundChecker.SurfaceNormal);
            }
            else
            {
                // keep our penguin onFeet at all times if main perpendicularity option is not enabled
                AlignPenguinWithGivenUpAxis(Vector2.up);
                _penguinEntity.Rigidbody.constraints |= RigidbodyConstraints2D.FreezeRotation;
            }

            // if movement is within thresholds, freeze all axes to prevent jitter
            if (Settings.EnableAutomaticAxisLockingForSmallVelocities &&
                Mathf.Abs(_penguinEntity.Rigidbody.velocity.x)      < Settings.LinearVelocityThreshold &&
                Mathf.Abs(_penguinEntity.Rigidbody.velocity.y)      < Settings.LinearVelocityThreshold &&
                Mathf.Abs(_penguinEntity.Rigidbody.angularVelocity) < Settings.AngularVelocityThreshold)
            {
                // todo: this will have to be covered in the state machine instead since we need
                //       to account for when there is no input...
                _penguinEntity.Rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }

        private void AlignPenguinWithGivenUpAxis(Vector2 targetUpAxis)
        {
            float degreesUnaligned = Vector2.SignedAngle(targetUpAxis, transform.up);
            if (Mathf.Abs(degreesUnaligned) >= Settings.DegreesFromSurfaceNormalThreshold)
            {
                Quaternion current = transform.rotation;
                Quaternion target  = ComputeOrientationForGivenUpAxis(_penguinEntity.Rigidbody, targetUpAxis);
                _penguinEntity.Rigidbody.MoveRotation(Quaternion.Lerp(current, target, Settings.SurfaceAlignmentRotationalStrength));
            }
        }


        private static Quaternion ComputeOrientationForGivenUpAxis(Rigidbody2D rigidbody, Vector3 targetUpAxis)
        {
            Vector3 currentForwardAxis = rigidbody.transform.forward;
            Vector3 targetLeftAxis    = Vector3.Cross(currentForwardAxis, targetUpAxis);
            Vector3 targetForwardAxis = Vector3.Cross(targetUpAxis,       targetLeftAxis);
            return Quaternion.LookRotation(targetForwardAxis, targetUpAxis);
        }
    }
}
