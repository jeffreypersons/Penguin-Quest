using System;
using PQ.Common.Events;


namespace PQ.Common.States
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
        private PqEventRegistry _eventRegistry;

        public string Name => _name;
        public bool IsActive => _active;
        public bool IsInitialized => _initialized;

        public override string ToString() =>
            $"{GetType().Name}:{{" +
                $"name:{_name}," +
                $"eventRegistry:{_eventRegistry}}}";


        /*** External Facing Methods for Driving State Logic ***/

        // External entry point for initializing the state
        // Note that any sub class dependencies should be hooked up via an override of this base constructor
        public FsmState(string name)
        {
            _name = name;
            _active = false;
            _eventRegistry = new();
            _initialized = false;
        }

        // Entry point for client code initializing state instances
        // Any 'startup' code such as hooking up handlers to events is done here
        public void Initialize()
        {
            OnIntialize();
            _initialized = true;
            _eventRegistry.UnsubscribeToAllRegisteredEvents();
        }

        // Entry point for client code utilizing state instances
        public void Enter()
        {
            OnEnter();
            _active = true;
            _eventRegistry.SubscribeToAllRegisteredEvents();
        }

        // Exit point for client code utilizing state instances
        public void Exit()
        {
            OnExit();
            _active = false;
            _eventRegistry.UnsubscribeToAllRegisteredEvents();
        }

        // Execute logic intended for early in a frame such as processing input
        public void Update()      => OnUpdate();

        // Execute logic intended for mid way through a frame such as fixed duration physics calculations
        public void FixedUpdate() => OnFixedUpdate();

        // Execute logic intended for later on in a frame such as programmatic visual effects
        public void LateUpdate()  => OnLateUpdate();

        
        public bool Equals(FsmState other) => other is not null && Name == other.Name;
        public override bool Equals(object obj) => Equals(obj as FsmState);
        public override int GetHashCode() => HashCode.Combine(Name);
        public static bool operator ==(FsmState left, FsmState right) =>  Equals(left, right);
        public static bool operator !=(FsmState left, FsmState right) => !Equals(left, right);



        /*** Internal Hooks for Defining State Specific Logic ***/

        // Mechanism for hooking up events to handlers such that they can automatically be subscribed on state enter
        // and unsubscribed on state exit.
        // Can only be invoked in OnInitialize.
        protected void RegisterEvent(IPqEventReceiver event_, Action handler_) => _eventRegistry.Add(event_, handler_);
        protected void RegisterEvent<T>(IPqEventReceiver<T> event_, Action<T> handler_) => _eventRegistry.Add(event_, handler_);

        // Required one time callback where long living data can be hooked up (eg events/handlers)
        protected abstract void OnIntialize();

        // Required entry/exit point callbacks
        protected abstract void OnEnter();
        protected abstract void OnExit();

        // Optional recurring callbacks
        protected virtual void OnUpdate()      { }
        protected virtual void OnFixedUpdate() { }
        protected virtual void OnLateUpdate()  { }
    }
}
