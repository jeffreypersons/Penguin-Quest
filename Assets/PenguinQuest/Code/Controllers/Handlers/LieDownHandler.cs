using UnityEngine;
using PenguinQuest.Data;


namespace PenguinQuest.Controllers.Handlers
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(PenguinBody))]
    public class LieDownHandler : MonoBehaviour
    {
        private Animator        penguinAnimator;
        private PenguinBody penguinSkeleton;

        void Awake()
        {
            penguinAnimator = gameObject.GetComponent<Animator>();
            penguinSkeleton = gameObject.GetComponent<PenguinBody>();
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
        }

        void OnLieDownAnimationEventMid()
        {
            penguinSkeleton.ColliderConstraints |=
                PenguinColliderConstraints.DisableFeet;
        }

        void OnLieDownAnimationEventEnd()
        {
            penguinSkeleton.ColliderConstraints |=
                PenguinColliderConstraints.DisableFeet |
                PenguinColliderConstraints.DisableFlippers;
        }
    }
}
