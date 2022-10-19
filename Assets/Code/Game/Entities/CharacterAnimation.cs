using System;
using UnityEngine;
using PQ.Common.Events;
using PQ.Common.Containers;


namespace PQ.Game.Entities
{
    // todo: write editor tools to provide hooks into animatorParamIds as well
    // todo: look into possibly integrating enumMap into event registry, and just using that here..

    /*
    Generic component for listening to animation events, interfacing with animator, and setting parameters.


    Note that animation event IDs are used to abstract the animator event details away, such that we only need
    to lookup 'subscribers' to be notified of an event, and parameters for notifying the animator that a relevant
    change occurred (eg data related to blending, transitions, or animation masks).

    In other words, note that we only expose parameter updates (not querying!) and event subscription (not raising!),
    as parameters drive the animator, events and any related data are hooked up in the editor to notify clients of the
    current animation context (and any corresponding data)
    */
    [Serializable]
    [ExecuteAlways]
    public abstract class CharacterAnimation<EventId> : MonoBehaviour
        where EventId : struct, Enum
    {
        private Animator _animator;
        private EnumMap<EventId, PqEvent> _animationEvents;

        public Vector2 SkeletalRootPosition => _animator.rootPosition;

        public override string ToString()
        {
            return $"{GetType()}" +
                   $"(animator:{_animator},{_animationEvents}";
        }


        /*** Internal Hooks for Setting up a Animation Component Instance ***/

        // external facing event queries such that callbacks to animator events can be hooked up where this is called
        public IPqEventReceiver LookupEvent(EventId id) => _animationEvents[id];


        // knobs for feeding the animator relevant data and context that it can then use to determine transitions/blends

        public bool SetInteger(string paramName, int paramValue)
        {
            if (_animator.GetInteger(paramName) == paramValue)
            {
                return false;
            }

            _animator.SetInteger(paramName, paramValue);
            OnParamChanged(paramName, paramValue);
            return true;
        }

        public bool SetBool(string paramName, bool paramValue)
        {
            if (_animator.GetBool(paramName) == paramValue)
            {
                return false;
            }

            _animator.SetBool(paramName, paramValue);
            OnParamChanged(paramName, paramValue);
            return true;
        }

        public bool SetFloat(string paramName, float paramValue)
        {
            if (_animator.GetFloat(paramName) == paramValue)
            {
                return false;
            }

            _animator.SetFloat(paramName, paramValue);
            OnParamChanged(paramName, paramValue);
            return true;
        }

        // add trigger to queue, regardless of it's already set or not.
        // if it is, then call ResetAllAnimatorTriggers() in one of the callbacks on parameter set
        // in the child class
        public bool AddTriggerToQueue(string paramName)
        {
            _animator.SetTrigger(paramName);
            OnParamChanged(paramName, "trigger");
            return true;
        }



        /*** Internal Hooks for Setting up a Animation Component Instance ***/

        // callback to hook up with Animator in animation clip window, for triggering our custom events via Unity
        protected void RaiseEvent(EventId id)
        {
            _animationEvents[id].Raise();
            OnEventRaised(id.ToString());
        }
        
        // Force Unity's animator to clear each trigger parameter's queue, whether they are currently firing or not.
        //
        // For example, if triggers [crouch, jump] are set while jumping, then when landing state is entered, then the
        // character undergoes jump->landing->crouch->jump->landing. Typically such behavior is nearly always undesired,
        // so by clearing all the triggers we avoid having to special-case trigger conditions.
        protected void ResetAllAnimatorTriggers(Animator animator)
        {
            foreach (var trigger in animator.parameters)
            {
                if (trigger.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.ResetTrigger(trigger.name);
                }
            }
        }


        // Optional overridable callback for when things are setup
        protected virtual void OnInitialize() { }

        // Optional overridable callback for when an event was raised via the Unity's animator/animation-clips
        protected virtual void OnEventRaised(string eventName) { }

        // Optional overridable callback for when data was supplied to this instance
        protected virtual void OnParamChanged<T>(string paramName, T paramValue) { }



        /*** Internal Hooks to MonoBehavior ***/

        private void Awake()
        {
            _animator = gameObject.GetComponent<Animator>();
            if (_animator == null)
            {
                throw new MissingComponentException("Expected attached animator - not found");
            }

            // note that since other monobehaviors may want to query events on Start(),
            // we populate events early here in Awake as opposed to Start
            _animationEvents = new EnumMap<EventId, PqEvent>();
            foreach (EventId id in _animationEvents.EnumFields)
            {
                _animationEvents.Add(id, new PqEvent(id.ToString()));
            }
        }
    }
}
