using UnityEngine;
using PenguinQuest.Data;


namespace PenguinQuest.Controllers
{
    [RequireComponent(typeof(Animator))]
    public class StandUpHandler : MonoBehaviour
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
            frontFlipperUpperCollider.enabled = true;
            frontFlipperLowerCollider.enabled = true;
            frontFootCollider        .enabled = true;
            backFootCollider         .enabled = true;
        }
        void OnStandUpAnimationEventEnd()
        {
            penguinAnimator.SetBool("IsUpright", true);
        }
    }
}
