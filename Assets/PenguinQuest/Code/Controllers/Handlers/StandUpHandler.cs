using UnityEngine;
using PenguinQuest.Data;


namespace PenguinQuest.Controllers.Handlers
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(PenguinSkeleton))]
    public class StandUpHandler : MonoBehaviour
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
            GameEventCenter.standupCommand.AddListener(OnStandUpInput);
            animationComponent.OnStandupStart += OnStandUpAnimationEventStart;
            animationComponent.OnStandupEnd   += OnStandUpAnimationEventEnd;
        }
        void OnDisable()
        {
            GameEventCenter.standupCommand.RemoveListener(OnStandUpInput);
            animationComponent.OnStandupStart -= OnStandUpAnimationEventStart;
            animationComponent.OnStandupEnd   -= OnStandUpAnimationEventEnd;
        }
        

        void OnStandUpInput(string _)
        {
            penguinAnimator.SetTrigger("StandUp");
        }

        void OnStandUpAnimationEventStart()
        {
            penguinSkeleton.ColliderConstraints = PenguinColliderConstraints.None;

            // enable the feet again, allowing the penguin to 'fall' down to alignment
            penguinSkeleton.ColliderConstraints &=
                ~PenguinColliderConstraints.DisableFeet;
        }

        void OnStandUpAnimationEventEnd()
        {
            // todo: this stuff needs to go in the state machine
            penguinAnimator.SetBool("IsUpright", true);
            transform.GetComponent<GroundHandler>().MaintainPerpendicularityToSurface = false;

            // enable the bounding box again, allowing the penguin to 'fall' down to alignment
            penguinSkeleton.ColliderConstraints &=
                ~PenguinColliderConstraints.DisableBoundingBox;
        }
    }
}
