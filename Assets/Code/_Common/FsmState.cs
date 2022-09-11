using System;
using System.Collections.Generic;


namespace PQ.Common
{
    /*
    Representation of a state in a finite state machine.

    Note that active, initialized etc are not checked everytime - it's up to the machinery
    of the module that handles the correct ordering of states. If it was done here, there would be tons
    of unnecessary and slow validation littered throughout the template hooks (eg Enter()).

    Intended to fully encapsulate graphics, animation, and physics needed for any specific state.
    State is entered and exited without any transitional checks - that is, it is entirely up to the call site to
    handle when transition is/is-not allowed to occur. Instead, it's up to the state to determine what the
    per-frame behavior is (or isn't) as callbacks are provided for regular, fixed, and late updates.
    */
    public abstract class FsmState : IEquatable<FsmState>
    {
        private class EventRegistry<T> where T : new()
        {
            public Dictionary<GameEvent<T>, Action<T>> Data = new();
        }


        private readonly string _name;
        private bool _active;
        private EventRegistry<object> _events;

        public string Name     => _name;
        public bool   IsActive => _active;

        public override string ToString() =>
            $"{GetType().Name}:{{" +
                $"name:{_name}," +
                $"eventRegistry:{_events.Data}}}";


        // Entry point for initializing the state - any event registration or external
        // class dependencies should be hooked up via an override of this base constructor
        //
        // this determines what events should automatically be
        // registered/unregistered on event enter/exit on state construction
        public FsmState(string name)
        {
            _name   = name;
            _active = false;
            _events = new();
            OnIntialize();
        }


        /*** Internal Hooks to MonoBehavior ***/

        // Required one time callbacks
        protected abstract void OnIntialize();
        protected abstract void OnEnter();
        protected abstract void OnExit();

        // Optional recurring callbacks
        protected virtual void OnUpdate()      { }
        protected virtual void OnFixedUpdate() { }
        protected virtual void OnLateUpdate()  { }


        // Entry point for client code utilizing state instances
        public void Enter()
        {
            _active = true;
            OnEnter();
            foreach (var (event_, callback_) in _events.Data)
            {
                event_.AddListener(callback_);
            }
        }

        public void Update()      => OnUpdate();
        public void FixedUpdate() => OnFixedUpdate();
        public void LateUpdate()  => OnLateUpdate();


        // Exit point for client code utilizing state instances
        public void Exit()
        {
            _active = false;
            OnExit();
            foreach (var (event_, callback_) in _events.Data)
            {
                event_.RemoveListener(callback_);
            }
        }


        public bool Equals(FsmState other) => other is not null && Name == other.Name;
        public override bool Equals(object obj) => Equals(obj as FsmState);
        public override int GetHashCode() => HashCode.Combine(Name);
        public static bool operator ==(FsmState left, FsmState right) =>  Equals(left, right);
        public static bool operator !=(FsmState left, FsmState right) => !Equals(left, right);
    }
}
