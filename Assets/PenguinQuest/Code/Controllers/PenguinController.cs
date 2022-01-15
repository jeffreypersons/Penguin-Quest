using UnityEngine;
using PenguinQuest.Controllers.Handlers;


namespace PenguinQuest.Controllers
{
    [RequireComponent(typeof(GroundHandler))]
    [RequireComponent(typeof(HorizontalMoveHandler))]
    [RequireComponent(typeof(JumpUpHandler))]
    [RequireComponent(typeof(StandUpHandler))]
    [RequireComponent(typeof(LieDownHandler))]

    [RequireComponent(typeof(PenguinSkeleton))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PenguinController : MonoBehaviour
    {
        private Vector2 initialSpawnPosition;
        private Rigidbody2D penguinRigidBody;
        private Animator    penguinAnimator;


        public void Reset()
        {
            penguinRigidBody.velocity = Vector2.zero;
            penguinRigidBody.position = initialSpawnPosition;
            penguinRigidBody.transform.localEulerAngles = Vector3.zero;

            penguinRigidBody.isKinematic = false;
            penguinAnimator.applyRootMotion = true;
            penguinAnimator.updateMode = AnimatorUpdateMode.Normal;
        }

        void Awake()
        {
            penguinRigidBody = gameObject.GetComponent<Rigidbody2D>();
            penguinAnimator  = gameObject.GetComponent<Animator>();
            initialSpawnPosition = penguinRigidBody.position;
            Reset();
        }
    }
}
