using UnityEngine;
using PenguinQuest.Controllers.Handlers;


namespace PenguinQuest.Controllers
{
    [RequireComponent(typeof(JumpUpHandler))]
    [RequireComponent(typeof(StandUpHandler))]
    [RequireComponent(typeof(LieDownHandler))]
    [RequireComponent(typeof(GroundHandler))]

    [RequireComponent(typeof(PenguinSkeleton))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PenguinController : MonoBehaviour
    {
        private enum Posture { UPRIGHT, ONBELLY, BENTOVER }

        private Vector2 initialSpawnPosition;
        private Rigidbody2D penguinRigidBody;

        public void Reset()
        {
            penguinRigidBody.velocity = Vector2.zero;
            penguinRigidBody.position = initialSpawnPosition;
            penguinRigidBody.transform.localEulerAngles = Vector3.zero;
        }

        void Awake()
        {
            penguinRigidBody = gameObject.GetComponent<Rigidbody2D>();

            initialSpawnPosition = penguinRigidBody.position;
            Reset();
        }
    }
}
