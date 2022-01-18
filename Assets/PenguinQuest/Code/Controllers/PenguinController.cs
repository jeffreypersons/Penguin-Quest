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
        private Rigidbody2D     penguinRigidBody = default;
        private Animator        penguinAnimator  = default;
        private PenguinSkeleton penguinSkeleton  = default;

        void Awake()
        {
            FetchComponentsIfNotAlready();
            initialSpawnPosition = penguinRigidBody.position;
            ResetPositioning();
        }

        public void FetchComponentsIfNotAlready()
        {
            if (penguinRigidBody == default || penguinAnimator == default || penguinSkeleton == default)
            {
                penguinRigidBody = gameObject.GetComponent<Rigidbody2D>();
                penguinAnimator  = gameObject.GetComponent<Animator>();
                penguinSkeleton  = gameObject.GetComponent<PenguinSkeleton>();
            }
        }
        
        public void ResetPositioning()
        {
            penguinRigidBody.velocity = Vector2.zero;
            penguinRigidBody.position = initialSpawnPosition;
            penguinRigidBody.transform.localEulerAngles = Vector3.zero;
        }
    }
}
