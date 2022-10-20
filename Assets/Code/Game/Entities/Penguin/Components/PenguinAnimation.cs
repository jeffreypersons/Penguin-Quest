using UnityEngine;
using PQ.Common.Animation;


namespace PQ.Game.Entities.Penguin
{
    public class PenguinAnimation : AnimationDriver<PenguinAnimationEventId, PenguinAnimationParamId>
    {
        protected override void OnInitialize()
        {
            Debug.Log("Initialized " + this);
        }

        protected override void OnEventRaised(string eventName)
        {
            Debug.Log($"PenguinAnimation[{Time.frameCount - 1}]: Event {eventName} received from animator");
        }

        // Optional overridable callback for when data was supplied to this instance
        protected override void OnParamChanged<T>(string paramName, T paramValue)
        {
            // todo: replace the 'comment toggle' with proper on/off logging elsewhere
            //Debug.Log($"PenguinAnimation[{Time.frameCount - 1}]: Param {paramName} with {paramValue} sent to animator");
        }
    }
}
