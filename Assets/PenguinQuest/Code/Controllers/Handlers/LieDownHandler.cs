using UnityEngine;
using PenguinQuest.Data;


namespace PenguinQuest.Controllers.Handlers
{
    [RequireComponent(typeof(PenguinEntity))]
    public class LieDownHandler : MonoBehaviour
    {
        private PenguinEntity penguinEntity;

        void Awake()
        {
            penguinEntity = gameObject.GetComponent<PenguinEntity>();
        }
        
        void OnEnable()
        {
            // note that for animation events the registration is done implicitly
            GameEventCenter.lieDownCommand.AddListener(OnLieDownInput);
            penguinEntity.Animation.OnLieDownStart += OnLieDownAnimationEventStart;
            penguinEntity.Animation.OnLieDownMid   += OnLieDownAnimationEventMid;
            penguinEntity.Animation.OnLieDownEnd   += OnLieDownAnimationEventEnd;
        }
        void OnDisable()
        {
            GameEventCenter.lieDownCommand.RemoveListener(OnLieDownInput);
            penguinEntity.Animation.OnLieDownStart -= OnLieDownAnimationEventStart;
            penguinEntity.Animation.OnLieDownMid   -= OnLieDownAnimationEventMid;
            penguinEntity.Animation.OnLieDownEnd   -= OnLieDownAnimationEventEnd;
        }

        
        void OnLieDownInput(string _)
        {
            penguinEntity.Animation.TriggerParamLieDownParameter();
        }

        void OnLieDownAnimationEventStart()
        {
            penguinEntity.Animation.SetParamIsUpright(false);
            penguinEntity.ColliderConstraints |=
                PenguinColliderConstraints.DisableBoundingBox |
                PenguinColliderConstraints.DisableFeet;
        }

        void OnLieDownAnimationEventMid()
        {
            penguinEntity.ColliderConstraints |=
                PenguinColliderConstraints.DisableFeet;
        }

        void OnLieDownAnimationEventEnd()
        {
            // todo: this stuff needs to go in the state machine
            transform.GetComponent<GroundHandler>().MaintainPerpendicularityToSurface = true;
            penguinEntity.ColliderConstraints |=
                PenguinColliderConstraints.DisableFeet |
                PenguinColliderConstraints.DisableFlippers;
        }
    }
}
