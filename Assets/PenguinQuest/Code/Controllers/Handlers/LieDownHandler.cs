using UnityEngine;
using PenguinQuest.Data;
using PenguinQuest.Controllers.AlwaysOnComponents;


namespace PenguinQuest.Controllers.Handlers
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(PenguinSkeleton))]
    [RequireComponent(typeof(GroundChecker))]
    public class LieDownHandler : MonoBehaviour
    {
        private Animator        penguinAnimator;
        private Rigidbody2D     penguinRigidbody;
        private PenguinSkeleton penguinSkeleton;
        private GroundChecker   groundChecker;

        void Awake()
        {
            penguinAnimator  = gameObject.GetComponent<Animator>();
            penguinRigidbody = gameObject.GetComponent<Rigidbody2D>();
            penguinSkeleton  = gameObject.GetComponent<PenguinSkeleton>();
            groundChecker    = gameObject.GetComponent<GroundChecker>();
        }

        void OnEnable()
        {
            // note that for animation events the registration is done implicitly
            GameEventCenter.lieDownCommand.AddListener(OnLieDownInput);
        }

        void OnDisable()
        {
            GameEventCenter.lieDownCommand.RemoveListener(OnLieDownInput);
        }

        void OnLieDownInput(string _)
        {
            penguinAnimator.SetTrigger("LieDown");
        }

        void OnLieDownAnimationEventStart()
        {
            penguinAnimator.SetBool("IsUpright", false);
            groundChecker.enabled = false;
            penguinRigidbody.constraints &= ~RigidbodyConstraints2D.FreezePosition;
        }

        void OnLieDownAnimationEventMid()
        {
            penguinSkeleton.ColliderConstraints |=
                PenguinColliderConstraints.DisableFeet;
        }

        void OnLieDownAnimationEventEnd()
        {
            groundChecker.enabled = true;
            penguinSkeleton.ColliderConstraints |=
                PenguinColliderConstraints.DisableFeet |
                PenguinColliderConstraints.DisableFlippers;
        }
    }
}
