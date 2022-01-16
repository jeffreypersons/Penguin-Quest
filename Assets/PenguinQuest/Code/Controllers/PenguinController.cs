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

        #if UNITY_EDITOR
        [Header("Test Settings")]
        [Tooltip("Use the a placeholder sprite instead of the actual penguin?")]
        [SerializeField] private bool usePlaceholderSprite = false;

        void OnValidate()
        {
            FetchComponentsIfNotAlready();
            if (usePlaceholderSprite)
            {
                penguinSkeleton.ColliderConstraints = PenguinColliderConstraints.DisableBoundingBox;
                this.enabled = true;
                transform.GetComponent<PenguinController>().enabled = false;
                Debug.Log("OnValidate - Using this script over other - enabled PenguinController_Old and disabled PenguinController");
            }
            else
            {
                penguinSkeleton.ColliderConstraints = PenguinColliderConstraints.DisableBoundingBox;
                this.enabled = false;
                transform.GetComponent<PenguinController>().enabled = true;
                Debug.Log("OnValidate - Using other script over this - enabled PenguinController and disabled PenguinController_Old");
            }
        }
        #endif


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
                penguinAnimator = gameObject.GetComponent<Animator>();
                penguinSkeleton = gameObject.GetComponent<PenguinSkeleton>();
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
