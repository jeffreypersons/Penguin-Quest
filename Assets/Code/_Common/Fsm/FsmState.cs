using System;
using System.Collections.Generic;
using PQ.Common.Events;


namespace PQ.Common.Fsm
{
    /*
    Representation of a state in a finite state machine.

    Intended to fully encapsulate graphics, animation, and physics needed for any specific state.
    State is entered and exited without any transitional checks - that is, it is entirely up to the call site to
    handle when transition is/is-not allowed to occur. Instead, it's up to the state to determine what the
    per-frame behavior is (or isn't) as callbacks are provided for regular, fixed, and late updates.

    
    Note that active, initialized etc are not checked everytime - it's up to the machinery
    of the module that handles the correct ordering of states. If it was done here, there would be tons
    of unnecessary and slow validation littered throughout the template hooks (eg Enter()).
    */
    public abstract class FsmState<StateId, SharedData>
        : IEquatable <FsmState<StateId, SharedData>>,
          IComparable<FsmState<StateId, SharedData>>
        where SharedData : FsmSharedData
        where StateId    : struct, Enum
    {
        private StateId         _id;
        private string          _name;
        private SharedData      _data;
        private bool            _active;
        private PqEventRegistry _eventRegistry;

        private PqEvent          _moveToLastStateSignal = new("fsm.state.move.last");
        private PqEvent<StateId> _moveToNextStateSignal = new("fsm.state.move.next");

        // Note that since enums are a value type, we can't use ==, so this is the best we can do (no boxing!) for id comparisons
        private static readonly EqualityComparer<StateId> IdEqualityComparer = EqualityComparer<StateId>.Default;

        public    StateId    Id     => _id;
        public    string     Name   => _name;
        protected SharedData Blob   => _data;
        public    bool       Active => _active;
        public IPqEventReceiver          OnMoveToLastStateSignaled => _moveToLastStateSignal;
        public IPqEventReceiver<StateId> OnMoveToNextStateSignaled => _moveToNextStateSignal;

        public override string ToString() =>
            $"FsmState(" +
                $"id:{_id}, " +
                $"active:{_active}, " +
                $"blob:{_data}, " +
                $"eventRegistry:[{_eventRegistry}]" +
            $")";
        

        int IComparable<FsmState<StateId, SharedData>>.CompareTo(FsmState<StateId, SharedData> other) =>
            Id.CompareTo(other.Id);
        bool IEquatable<FsmState<StateId, SharedData>>.Equals(FsmState<StateId, SharedData> other) =>
            other is not null && IdEqualityComparer.Equals(Id, other.Id);

        public override bool Equals(object obj) =>
            ((IEquatable<FsmState<StateId, SharedData>>)this).Equals(obj as FsmState<StateId, SharedData>);
        public override int GetHashCode() =>
            HashCode.Combine(IdEqualityComparer.GetHashCode());

        public static bool operator ==(FsmState<StateId, SharedData> left, FsmState<StateId, SharedData> right) =>
            ReferenceEquals(left, right) ||
            (left is not null && ((IEquatable<FsmState<StateId, SharedData>>)left).Equals(right));
        public static bool operator !=(FsmState<StateId, SharedData> left, FsmState<StateId, SharedData> right) =>
            !(left == right);


        /*** External Facing Methods for Driving State Logic ***/

        // Public dummy state constructor so that we can constrain generics to new(), for use in factories
        public FsmState() { }

        // Note that since enums are a value type, we can't use ==, so this is the best we can do (no boxing!) for id comparisons
        public bool        HasSameId(StateId id)                  => IdEqualityComparer.Equals(_id, id);
        public static bool HasSameId(StateId left, StateId right) => IdEqualityComparer.Equals(left, right);

        // External entry point factory for constructing the state
        // Note that this is our uniform single access point for creating the state, no public constructors
        public static StateSubclassInstance Create<StateSubclassInstance>(StateId id, SharedData blob)
            where StateSubclassInstance : FsmState<StateId, SharedData>, new()
        {
            new UnityEngine.Vector3();
            return new()
            {
                _id = id,
                _name = Enum.GetName(typeof(StateId), id),
                _data = blob,
                _active = false,
                _eventRegistry = new(),
            };
        }

        // Entry point for client code initializing state instances
        // Any 'startup' code such as hooking up handlers to events is done here
        public void Initialize()
        {
            OnIntialize();
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



        /*** Internal Hooks for Defining State Specific Logic ***/

        // Mechanism for hooking up events to handlers such that they can automatically be subscribed on state enter
        // and unsubscribed on state exit.
        // Can only be invoked in OnInitialize.
        protected void SignalMoveToLastState()             => _moveToLastStateSignal.Raise();
        protected void SignalMoveToNextState(StateId dest) => _moveToNextStateSignal.Raise(dest);
        protected void RegisterEvent(IPqEventReceiver event_, Action handler_)          => _eventRegistry.Add(event_, handler_);
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
