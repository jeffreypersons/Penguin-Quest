using UnityEngine;
using PQ.Common.Animation;


namespace PQ.Game.Entities.Penguin
{
    public sealed class PenguinAnimationDriver : AnimationDriver<PenguinAnimationEventId, PenguinAnimationParamId>
    {
        [SerializeField] private Animator _animator;

        protected override void OnInitialize()
        {
            Debug.Log("Initialized " + this);

            // until animator params configured, the param ids won't match, so until then silence the validation
            try { Initialize(_animator); } catch (System.InvalidOperationException) { }
        }

        protected override void OnEventRaised(string eventName)
        {
            Debug.Log($"PenguinAnimation[Frame={Time.frameCount - 1}]: Event {eventName} received from animator");
        }

        // Optional overridable callback for when data was supplied to this instance
        protected override void OnParamChanged<T>(string paramName, T paramValue)
        {
            // todo: replace the 'comment toggle' with proper on/off logging elsewhere
            //Debug.Log($"PenguinAnimation[{Time.frameCount - 1}]: Param {paramName} with {paramValue} sent to animator");
        }
    }
}
