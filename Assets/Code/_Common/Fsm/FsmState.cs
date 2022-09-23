using System;
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
    public abstract class FsmState<DataBlob> : IEquatable<FsmState<DataBlob>>, IComparable<FsmState<DataBlob>>
        where DataBlob : FsmBlackboardData
    {
        public enum Status
        {
            Unintialized,
            Inactive,
            Active
        }

        private string          _id;
        private DataBlob        _blob;
        private Status          _status;
        private PqEventRegistry _eventRegistry;
        private PqEvent         _moveToLastStateRequest = new("fsm.state.move.last");
        private PqEvent<string> _moveToNextStateRequest = new("fsm.state.move.next");

        public    string   Id            => _id;
        protected DataBlob Blob          => _blob;
        public    Status   CurrentStatus => _status;

        public IPqEventReceiver         OnMoveToLastStateSignaled => _moveToLastStateRequest;
        public IPqEventReceiver<string> OnMoveToNextStateSignaled => _moveToNextStateRequest;

        public override string ToString() =>
            $"FsmState(" +
                $"id:{_id}," +
                $"status:{_status}" +
                $"blob:{_blob}" +
                $"eventRegistry:[{_eventRegistry}]" +
            $")";



        /*** External Facing Methods for Driving State Logic ***/

        // Public dummy state constructor so that we can constrain generics to new(), for use in factories
        public FsmState() { }


        // External entry point factory for constructing the state
        // Note that this is our uniform single access point for creating the state, no public constructors
        public static StateSubclass Create<StateSubclass>(string id, DataBlob blob)
            where StateSubclass : FsmState<DataBlob>, new()
        {
            return new()
            {
                _id            = id,
                _status        = Status.Unintialized,
                _eventRegistry = new(),
                _blob          = blob,
            };
        }

        // Entry point for client code initializing state instances
        // Any 'startup' code such as hooking up handlers to events is done here
        public void Initialize()
        {
            OnIntialize();
            _status = Status.Inactive;
            _eventRegistry.UnsubscribeToAllRegisteredEvents();
        }

        // Entry point for client code utilizing state instances
        public void Enter()
        {
            OnEnter();
            _status = Status.Active;
            _eventRegistry.SubscribeToAllRegisteredEvents();
        }

        // Exit point for client code utilizing state instances
        public void Exit()
        {
            OnExit();
            _status = Status.Inactive;
            _eventRegistry.UnsubscribeToAllRegisteredEvents();
        }

        // Execute logic intended for early in a frame such as processing input
        public void Update()      => OnUpdate();

        // Execute logic intended for mid way through a frame such as fixed duration physics calculations
        public void FixedUpdate() => OnFixedUpdate();

        // Execute logic intended for later on in a frame such as programmatic visual effects
        public void LateUpdate()  => OnLateUpdate();


        int IComparable<FsmState<DataBlob>>.CompareTo(FsmState<DataBlob> other) => Id.CompareTo(other.Id);
        bool IEquatable<FsmState<DataBlob>>.Equals(FsmState<DataBlob> other) => other is not null && Id == other.Id;
        public override bool Equals(object obj) => ((IEquatable<FsmState<DataBlob>>)this).Equals(obj as FsmState<DataBlob>);
        public override int GetHashCode() => HashCode.Combine(Id);
        public static bool operator ==(FsmState<DataBlob> left, FsmState<DataBlob> right) =>  Equals(left, right);
        public static bool operator !=(FsmState<DataBlob> left, FsmState<DataBlob> right) => !Equals(left, right);



        /*** Internal Hooks for Defining State Specific Logic ***/

        // Mechanism for hooking up events to handlers such that they can automatically be subscribed on state enter
        // and unsubscribed on state exit.
        // Can only be invoked in OnInitialize.
        protected void SignalMoveToLastState()            => _moveToLastStateRequest.Raise();
        protected void SignalMoveToNextState(string dest) => _moveToNextStateRequest.Raise(dest);
        protected void RegisterEvent(IPqEventReceiver event_, Action handler_)          => _eventRegistry.Add(event_, handler_);
        protected void RegisterEvent<D>(IPqEventReceiver<D> event_, Action<D> handler_) => _eventRegistry.Add(event_, handler_);


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
