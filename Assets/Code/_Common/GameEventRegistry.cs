using System;
using System.Collections.Generic;
using UnityEngine;


namespace PQ.Common
{
    /*
    Provides a way to subscribe/unsubscribe an event and their corresponding handler.

    Note that intentionally the only exposed manipulation of events is on an all or nothing basis.
    The idea is providing a mechanism to hook up existing events with existing handlers that
    can be periodically be turned on and off as a group.

    Assumptions
    - Events and callbacks are available for entire life time of the registry
    - Order registered (as of now) is not significant in any way
    */
    public class GameEventRegistry
    {
        private class Entry<T>
        {
            public Action<T> Event   { get; set; }
            public Action<T> Handler { get; set; }
            public Entry(Action<T> event_, Action<T> handler_)
            {
                Event = event_;
                Handler = handler_;
            }
        }

        private bool _active;
        private string _description;
        private List<Entry<IEventPayload>> _eventHandlers;

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
            for (int i = 0; i < _eventHandlers.Count; i++)
            {
                _eventHandlers[i].Event += _eventHandlers[i].Handler;
            }
        }

        public void UnsubscribeToAllRegisteredEvents()
        {
            for (int i = 0; i < _eventHandlers.Count; i++)
            {
                _eventHandlers[i].Event -= _eventHandlers[i].Handler;
            }
        }

        public void Add(GameEvent<IEventPayload> event_, Action<IEventPayload> handler_)
        {
            if (IsEventAlreadyInRegistry(event_))
            {
                throw new ArgumentException($"{event_.Name} is already in registry");
            }

            string eventName = event_.Name;
            string handlerName = handler_.Method.Name;
            Debug.Log($"{eventName}=>{handlerName}");

            _description += $"{eventName}=>{handlerName};";

            _eventHandlers.Add(new Entry<IEventPayload>(event_.AsAction, handler_));

            // explicitly enforce that any new event-handler pairs have a subscription state matching the
            // rest of the event-handler pairs in the registry
        }

        private bool IsEventAlreadyInRegistry(GameEvent<IEventPayload> event_)
        {
            var key = event_.AsAction;
            for (int i = 0; i < _eventHandlers.Count; i++)
            {
                if (_eventHandlers[i].Event == key)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
