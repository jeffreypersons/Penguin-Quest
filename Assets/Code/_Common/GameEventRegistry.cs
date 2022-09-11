using System;
using System.Collections.Generic;


namespace PQ.Common
{
    /*
    Provides a way to manage the lifetime of events and their corresponding handler.

    Note that intentionally the only exposed manipulation of events is on an all or nothing basis.
    The idea is providing a mechanism to hook up existing events with existing handlers that
    can be periodically be turned on and off as a group.

    Assumptions
    - Events and callbacks are available for entire life time of the registry
    - Order registered (as of now) is not significant in any way
    */
    public class GameEventRegistry
    {
        private bool _active;
        private string _description;
        public Dictionary<GameEvent<IEventPayload>, Action<IEventPayload>> _eventHandlers;

        public bool IsActive => _active;
        public override string ToString() => _description == ""? "<empty>" : _description;

        public GameEventRegistry()
        {
            _active = false;
            _description = "";
            _eventHandlers = new();
        }

        public void SubscribeToAllRegisteredEvents()
        {
            foreach (var (event_, callback_) in _eventHandlers)
            {
                event_.AddListener(callback_);
            }
        }

        public void UnsubscribeToAllRegisteredEvents()
        {
            foreach (var (event_, callback_) in _eventHandlers)
            {
                event_.RemoveListener(callback_);
            }
        }

        public void Add<T>(GameEvent<T> event_, Action<T> handler_) where T : struct, IEventPayload
        {
            string eventName  = event_.Name;
            string handerName = handler_.Method.Name;
            if (!_eventHandlers.TryAdd(
                key:   event_    as GameEvent<IEventPayload>,
                value: handler_ as Action<IEventPayload>))
            {
                throw new ArgumentException($"{eventName} is already in registry can only be added once - skipping");
            }

            _description += $"{eventName}=>{handerName};";

            // explicitly enforce that any new event-handler pairs have a subscription state matching the
            // rest of the event-handler pairs in the registry
            if (_active)
            {
                event_.AddListener(handler_);
            }
            else
            {
                event_.RemoveListener(handler_);
            }
        }
    }
}
