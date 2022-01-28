using UnityEngine;
using PenguinQuest.Data;


namespace PenguinQuest.Controllers.Handlers
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(PenguinSkeleton))]
    public class LieDownHandler : MonoBehaviour
    {
        private Animator        penguinAnimator;
        private PenguinSkeleton penguinSkeleton;
        private PenguinAnimationEventReciever animationComponent;

        void Awake()
        {
            animationComponent = gameObject.GetComponentInChildren<PenguinAnimationEventReciever>();
            penguinAnimator    = gameObject.GetComponent<Animator>();
            penguinSkeleton    = gameObject.GetComponent<PenguinSkeleton>();
        }
        
        void OnEnable()
        {
            // note that for animation events the registration is done implicitly
            GameEventCenter.lieDownCommand.AddListener(OnLieDownInput);
            animationComponent.OnLiedownStart += OnLieDownAnimationEventStart;
            animationComponent.OnLiedownMid   += OnLieDownAnimationEventMid;
            animationComponent.OnLiedownEnd   += OnLieDownAnimationEventEnd;
        }
        void OnDisable()
        {
            GameEventCenter.lieDownCommand.RemoveListener(OnLieDownInput);
            animationComponent.OnLiedownStart -= OnLieDownAnimationEventStart;
            animationComponent.OnLiedownMid   -= OnLieDownAnimationEventMid;
            animationComponent.OnLiedownEnd   -= OnLieDownAnimationEventEnd;
        }

        
        void OnLieDownInput(string _)
        {
            penguinAnimator.SetTrigger("LieDown");
        }

        void OnLieDownAnimationEventStart()
        {
            penguinAnimator.SetBool("IsUpright", false);
            penguinSkeleton.ColliderConstraints |=
                PenguinColliderConstraints.DisableBoundingBox |
                PenguinColliderConstraints.DisableFeet;
        }

        void OnLieDownAnimationEventMid()
        {
            penguinSkeleton.ColliderConstraints |=
                PenguinColliderConstraints.DisableFeet;
        }

        void OnLieDownAnimationEventEnd()
        {
            // todo: this stuff needs to go in the state machine
            transform.GetComponent<GroundHandler>().MaintainPerpendicularityToSurface = true;
            penguinSkeleton.ColliderConstraints |=
                PenguinColliderConstraints.DisableFeet |
                PenguinColliderConstraints.DisableFlippers;
        }
    }
}
