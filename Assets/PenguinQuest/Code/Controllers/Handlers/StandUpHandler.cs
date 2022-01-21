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

        void Awake()
        {
            penguinAnimator = gameObject.GetComponent<Animator>();
            penguinSkeleton = gameObject.GetComponent<PenguinSkeleton>();
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
            penguinSkeleton.ColliderConstraints = PenguinColliderConstraints.None;
        }

        void OnStandUpAnimationEventEnd()
        {
            penguinAnimator.SetBool("IsUpright", true);
        }
    }
}
