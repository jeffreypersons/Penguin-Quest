using System;


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
        private readonly string _name;
        private bool _active;
        private bool _initialized;
        private GameEventRegistry _eventRegistry;

        public string Name     => _name;
        public bool   IsActive => _active;

        public override string ToString() =>
            $"{GetType().Name}:{{" +
                $"name:{_name}," +
                $"eventRegistry:{_eventRegistry}}}";


        // Entry point for initializing the state - any event registration or external
        // class dependencies should be hooked up via an override of this base constructor
        //
        // this determines what events should automatically be
        // registered/unregistered on event enter/exit on state construction
        public FsmState(string name)
        {
            _name = name;
            _active = false;
            _eventRegistry = null;

            OnIntialize();
            _initialized = true;
            _eventRegistry = new();
        }


        /*** Internal Hooks for Defining State Specific Logic ***/

        // 
        protected void RegisterEvent<T>(GameEvent<T> event_, Action<T> handler_) where T : struct, IEventPayload
        {
            if (_initialized)
            {
                throw new InvalidOperationException("Cannot register any events outside of OnInitialize - skipping");
            }

            if (_eventRegistry == null)
            {
                _eventRegistry = new();
            }
            _eventRegistry.Add<T>(event_, handler_);
        }

        // Required one time callback where long living data can be hooked up (eg events/handlers)
        protected abstract void OnIntialize();

        // Required entry/exit point callbacks
        protected abstract void OnEnter();
        protected abstract void OnExit();

        // Optional recurring callbacks
        protected virtual void OnUpdate()      { }
        protected virtual void OnFixedUpdate() { }
        protected virtual void OnLateUpdate()  { }


        // Entry point for client code utilizing state instances
        public void Enter()
        {
            OnEnter();
            _active = true;
            _eventRegistry.SubscribeToAllRegisteredEvents();
        }

        public void Update()      => OnUpdate();
        public void FixedUpdate() => OnFixedUpdate();
        public void LateUpdate()  => OnLateUpdate();


        // Exit point for client code utilizing state instances
        public void Exit()
        {
            OnExit();
            _active = false;
            _eventRegistry.UnsubscribeToAllRegisteredEvents();
        }


        public bool Equals(FsmState other) => other is not null && Name == other.Name;
        public override bool Equals(object obj) => Equals(obj as FsmState);
        public override int GetHashCode() => HashCode.Combine(Name);
        public static bool operator ==(FsmState left, FsmState right) =>  Equals(left, right);
        public static bool operator !=(FsmState left, FsmState right) => !Equals(left, right);
    }
}
