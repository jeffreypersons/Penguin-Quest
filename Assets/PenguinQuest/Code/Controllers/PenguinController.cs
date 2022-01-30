using UnityEngine;
using PenguinQuest.Controllers.Handlers;


namespace PenguinQuest.Controllers
{
    [RequireComponent(typeof(GroundHandler))]
    [RequireComponent(typeof(HorizontalMoveHandler))]
    [RequireComponent(typeof(JumpUpHandler))]
    [RequireComponent(typeof(StandUpHandler))]
    [RequireComponent(typeof(LieDownHandler))]

    [RequireComponent(typeof(PenguinEntity))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PenguinController : MonoBehaviour
    {
        private PenguinEntity penguinEntity;

        private Vector2 initialSpawnPosition;


        void Awake()
        {
            penguinEntity = gameObject.GetComponent<PenguinEntity>();

            // todo: this should be in state machine for upright and we should start in a blank state and then
            //       entered rather than assuming we start upright here...
            transform.GetComponent<GroundHandler>().MaintainPerpendicularityToSurface = false;

            initialSpawnPosition = penguinEntity.Rigidbody.position;
            ResetPositioning();
        }

        public void ResetPositioning()
        {
            penguinEntity.Rigidbody.velocity = Vector2.zero;
            penguinEntity.Rigidbody.position = initialSpawnPosition;
            penguinEntity.Rigidbody.transform.localEulerAngles = Vector3.zero;
        }
    }
}
