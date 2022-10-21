using System;
using System.Linq;
using UnityEngine;
using PQ.Common.Events;
using PQ.Common.Containers;
using System.Collections.Generic;


//
// *** todo: look into advanced animator param support ***
// - use param class that is used as our source of truth for the animator params,
//   which will throw errors if anything is attempted to be added in the editor (or kept in sync),
//   and internally use hash ids (with collision validation) for performance/memory improvements over raw names
// - some relevant resources:
//   * https://docs.unity3d.com/ScriptReference/Animations.AnimatorController.html
//   * https://forum.unity.com/threads/animator-parameter-to-custom-editor.857440/#post-8369559
//   * https://forum.unity.com/threads/custom-editor-for-animation-controller.513564
//
//
namespace PQ.Common.Animation
{
    /*
    Generic component for listening to animation events, interfacing with animator, and setting parameters.


    Note that animation event IDs are used to abstract the animator event details away, such that we only need
    to lookup 'subscribers' to be notified of an event, and parameters for notifying the animator that a relevant
    change occurred (eg data related to blending, transitions, or animation masks).

    In other words, note that we only expose parameter updates (not querying!) and event subscription (not raising!),
    as parameters drive the animator, events and any related data are hooked up in the editor to notify clients of the
    current animation context (and any corresponding data).
    */
    [ExecuteAlways]
    public abstract class AnimationDriver<EventId, ParamId> : MonoBehaviour
        where EventId : struct, Enum
        where ParamId : struct, Enum
    {
        private Animator _animator;
        private EnumMap<EventId, PqEvent> _animationEvents;
        private EnumMap<ParamId, string>  _animationParams;

        public Vector2 SkeletalRootPosition => _animator.rootPosition;

        public override string ToString()
        {
            return $"{GetType()}" +
                   $"(animator:{_animator},{_animationEvents}";
        }


        /*** Internal Hooks for Setting up a Animation Component Instance ***/

        // external facing event queries such that callbacks to animator events can be hooked up where this is called
        public IPqEventReceiver LookupEvent(EventId eventId) => _animationEvents[eventId];


        // knobs for feeding the animator relevant data and context that it can then use to determine transitions/blends

        public bool SetInteger(ParamId paramId, int paramValue)
        {
            var paramName = _animationParams[paramId];
            if (_animator.GetInteger(paramName) == paramValue)
            {
                return false;
            }

            _animator.SetInteger(paramName, paramValue);
            OnParamChanged(paramName, paramValue);
            return true;
        }

        public bool SetBool(ParamId paramId, bool paramValue)
        {
            var paramName = _animationParams[paramId];
            if (_animator.GetBool(paramName) == paramValue)
            {
                return false;
            }

            _animator.SetBool(paramName, paramValue);
            OnParamChanged(paramName, paramValue);
            return true;
        }

        public bool SetFloat(ParamId paramId, float paramValue)
        {
            var paramName = _animationParams[paramId];
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
        public bool AddTriggerToQueue(ParamId paramId)
        {
            var paramName = _animationParams[paramId];
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
                throw new MissingComponentException($"Expected attached animator - not found on {gameObject}");
            }

            // note that since other monobehaviors may want to query events on Start(),
            // we populate events early here in Awake as opposed to Start
            _animationEvents = new EnumMap<EventId, PqEvent>();
            foreach (EventId eventId in _animationEvents.EnumFields)
            {
                _animationEvents.Add(eventId, new PqEvent(eventId.ToString()));
            }

            // eventually we will support parameter ids directly, OR map IDs to a custom param class,
            // but for now we explicitly maintain id to mecanim parameter names, and enforce exact match
            _animationParams = new EnumMap<ParamId, string>();
            foreach (ParamId id in _animationParams.EnumFields)
            {
                _animationParams.Add(id, id.ToString());
            }

            var expected = _animationParams.EnumFields as IReadOnlyList<ParamId>;
            var actual = _animator.parameters as IReadOnlyList<AnimatorControllerParameter>;
            for (int i = 0; i < expected.Count; i++)
            {
                if (i >= actual.Count || expected[i].ToString() != actual[i].name)
                {
                    throw new InvalidOperationException(
                        $"Animation parameter mismatch - expected [{string.Join(',', expected)}] " +
                        $"found parameter mismatch - actual [{string.Join(',', actual)}]"
                    );
                }

            }
            EnsureRegisteredAndEditorParamsAreAnExistMatch();
        }
        


        /*** Internal 'Machinery' ***/

        // is the ordering, count, and names of our paramIds an _exact_ match with the ones in the mecanim editor window?
        private void EnsureRegisteredAndEditorParamsAreAnExistMatch()
        {
            IReadOnlyList<string> expected = _animationParams.Values;
            IReadOnlyList<string> actual   = _animator.parameters.Select(param => param.name).ToArray();
            for (int i = 0; i < expected.Count; i++)
            {
                if (i >= actual.Count || expected[i] != actual[i])
                {
                    throw new InvalidOperationException(
                        $"Animation parameter mismatch - expected [{string.Join(',', expected)}] " +
                        $"found parameter mismatch - actual [{string.Join(',', actual)}]"
                    );
                }
            }
        }
    }
}
