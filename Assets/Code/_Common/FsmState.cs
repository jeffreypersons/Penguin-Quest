﻿using System;
using System.Collections.Generic;
using System.Text;


namespace PQ.Common
{
    /*
    Representation of a state in a finite state machine.

    Intended to fully encapsulate graphics, animation, and physics needed for any specific state.
    State is entered and exited without any transitional checks - that is, it is entirely up to the call site to
    handle when transition is/is-not allowed to occur. Instead, it's up to the state to determine what the
    per-frame behavior is (or isn't) as callbacks are provided for regular, fixed, and late updates.
    */
    public abstract class FsmState : IEquatable<FsmState>
    {
        // todo: look into extracting this out into some sort of bus, and replace GameEventCenter with it
        private class EventRegistry<EventData>
        {
            private string _description;
            private readonly Dictionary<GameEvent<EventData>, Action<EventData>> _eventToHandlerMapping;

            public override string ToString() => _description;


            public EventRegistry(params (GameEvent<EventData>, Action<EventData>)[] eventCallbacks)
            {
                var stringBuilder = new StringBuilder(eventCallbacks.Length);
                _eventToHandlerMapping = new Dictionary<GameEvent<EventData>, Action<EventData>>(eventCallbacks.Length);
                foreach (var (event_, callback_) in eventCallbacks)
                {
                    _eventToHandlerMapping[event_] = callback_;
                    stringBuilder.AppendFormat("{0}=>{1};", event_.Name, callback_.Method.Name);
                }

                _description = stringBuilder.ToString();
            }

            public void StartListening()
            {
                foreach (var (event_, callback_) in _eventToHandlerMapping)
                {
                    event_.AddListener(callback_);
                }
            }
            public void StopListening()
            {
                foreach (var (event_, callback_) in _eventToHandlerMapping)
                {
                    event_.RemoveListener(callback_);
                }
            }
        }


        private readonly string _name;
        private bool _isActive;
        private bool _isHookedupWithEvents;
        private EventRegistry<object> _eventRegistry = null;

        public string Name             => _name;
        public bool   IsActive         => _isActive;
        public bool   IsEventsHookedUp => _isHookedupWithEvents;

        public override string ToString() =>
            $"{GetType().Name}:{{" +
                $"name:{_name}," +
                $"active:{_isActive}," +
                $"eventRegistry:{_eventRegistry}}}";


        // Entry point for initializing the state - any event registration or external
        // class dependencies should be hooked up via an override of this base constructor
        //
        // this determines what events should automatically be
        // registered/unregistered on event enter/exit on state construction
        public FsmState(string name, params (GameEvent<object>, Action<object>)[] eventCallbacks)
        {
            _name          = name;
            _eventRegistry = null;
            _isActive      = false;
            _eventRegistry = new EventRegistry<object>(eventCallbacks);
        }

        // Required one time callbacks
        public abstract void OnEnter();
        public abstract void OnExit();

        // Optional recurring callbacks
        public virtual void OnUpdate()      { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnLateUpdate()  { }


        // Entry point for client code utilizing state instances
        public void Enter()
        {
            _isActive = true;
            OnEnter();
            _eventRegistry.StartListening();
        }

        // Exit point for client code utilizing state instances
        public void Exit()
        {
            _isActive = false;
            OnExit();
            _eventRegistry.StopListening();
        }

        public bool Equals(FsmState other) => other is not null && Name == other.Name;
        public override bool Equals(object obj) => Equals(obj as FsmState);
        public override int GetHashCode() => HashCode.Combine(Name);
        public static bool operator ==(FsmState left, FsmState right) =>  Equals(left, right);
        public static bool operator !=(FsmState left, FsmState right) => !Equals(left, right);
    }
}
