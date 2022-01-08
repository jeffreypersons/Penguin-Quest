using UnityEngine;
using PenguinQuest.Data;


namespace PenguinQuest.Controllers
{
    [RequireComponent(typeof(Animator))]
    public class LieDownHandler : MonoBehaviour
    {
        [Header("Collider References")]
        [SerializeField] private CapsuleCollider2D headCollider              = default;
        [SerializeField] private CapsuleCollider2D torsoCollider             = default;
        [SerializeField] private CapsuleCollider2D frontFlipperUpperCollider = default;
        [SerializeField] private CapsuleCollider2D frontFlipperLowerCollider = default;
        [SerializeField] private CapsuleCollider2D frontFootCollider         = default;
        [SerializeField] private CapsuleCollider2D backFootCollider          = default;

        private Animator penguinAnimator;

        void Awake()
        {
            penguinAnimator = gameObject.GetComponent<Animator>();
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
            frontFootCollider.enabled = false;
            backFootCollider .enabled = false;
        }
        void OnLieDownAnimationEventEnd()
        {
            frontFlipperUpperCollider.enabled = false;
            frontFlipperLowerCollider.enabled = false;
        }
    }
}
