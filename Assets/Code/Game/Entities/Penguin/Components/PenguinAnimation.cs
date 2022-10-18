using UnityEngine;


namespace PQ.Game.Entities.Penguin
{
    public class PenguinAnimation : CharacterAnimation<PenguinAnimationEventId>
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
            // todo: look into comparing current value in base component, such that we only log when it actually changes
            // comment out since very noisy...
            //Debug.Log($"PenguinAnimation[{Time.frameCount - 1}]: Param {paramName} with {paramValue} sent to animator");
        }
    }
}
