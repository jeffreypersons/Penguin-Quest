using UnityEngine;
using PenguinQuest.Data;
using PenguinQuest.Controllers.AlwaysOnComponents;


namespace PenguinQuest.Controllers.Handlers
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(PenguinSkeleton))]
    [RequireComponent(typeof(GroundChecker))]
    public class StandUpHandler : MonoBehaviour
    {
        private Animator        penguinAnimator;
        private PenguinSkeleton penguinSkeleton;
        private GroundChecker   groundChecker;

        void Awake()
        {
            penguinAnimator = gameObject.GetComponent<Animator>();
            penguinSkeleton = gameObject.GetComponent<PenguinSkeleton>();
            groundChecker   = gameObject.GetComponent<GroundChecker>();
        }

        void OnEnable()
        {
            // note that for animation events the registration is done implicitly
            GameEventCenter.standupCommand.AddListener(OnStandUpInput);
        }
        void OnDisable()
        {
            GameEventCenter.standupCommand.RemoveListener(OnStandUpInput);
        }
        

        void OnStandUpInput(string _)
        {
            penguinAnimator.SetTrigger("StandUp");
        }

        void OnStandUpAnimationEventStart()
        {
            groundChecker.enabled = false;
            penguinSkeleton.ColliderConstraints = PenguinColliderConstraints.None;
        }

        void OnStandUpAnimationEventEnd()
        {
            groundChecker.enabled = true;
            penguinAnimator.SetBool("IsUpright", true);
        }
    }
}
