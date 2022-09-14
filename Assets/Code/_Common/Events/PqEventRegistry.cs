﻿using System;
using System.Collections.Generic;


namespace PQ.Common.Events
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
    public sealed class PqEventRegistry : IDisposable
    {
        // note that interface is required for contraviant storage of event/handler pairs
        private interface IEntry
        {
            public string EventName { get; }
            public string HandlerName { get; }
            public void Subscribe();
            public void Unsubscribe();
        }

        private class Entry : IEntry
        {
            private IPqEventReceiver _event;
            private Action _handler;

            string IEntry.EventName     => _event.Name;
            string IEntry.HandlerName   => _handler.Method.Name;
            void   IEntry.Subscribe()   => _event.AddHandler(_handler);
            void   IEntry.Unsubscribe() => _event.RemoveHandler(_handler);

            public Entry(IPqEventReceiver event_, Action handler_)
            {
                _event = event_;
                _handler = handler_;
            }
        }

        private class Entry<T> : IEntry
        {
            private IPqEventReceiver<T> _event;
            private Action<T> _handler;

            string IEntry.EventName     => _event.Name;
            string IEntry.HandlerName   => _handler.Method.Name;
            void   IEntry.Subscribe()   => _event.AddHandler(_handler);
            void   IEntry.Unsubscribe() => _event.RemoveHandler(_handler);

            public Entry(IPqEventReceiver<T> event_, Action<T> handler_)
            {
                _event = event_;
                _handler = handler_;
            }
        }


        private bool _active;
        private string _description;
        private List<IEntry> _eventActionEntries;

        public PqEventRegistry()
        {
            _active = false;
            _description = "";
            _eventActionEntries = new();
        }

        public bool IsActive => _active;
        public override string ToString() => _description == "" ? "<empty>" : _description;

        public void Add(IPqEventReceiver event_, Action handler_)          => AppendEntry(new Entry(event_, handler_));
        public void Add<T>(IPqEventReceiver<T> event_, Action<T> handler_) => AppendEntry(new Entry<T>(event_, handler_));

        public void SubscribeToAllRegisteredEvents()   => SetSubscriptionState(true);
        public void UnsubscribeToAllRegisteredEvents() => SetSubscriptionState(false);
        void IDisposable.Dispose() => SetSubscriptionState(false);


        private void SetSubscriptionState(bool state)
        {
            // just an fyi if you ever return here looking to optimize - list enumerators use
            // value types contrary to most the rest of C#,
            // so that for each creates zero garbage collection pressure! :)
            if (state)
            {
                _active = true;
                _eventActionEntries.ForEach(entry => entry.Subscribe());
            }
            else
            {
                _active = true;
                _eventActionEntries.ForEach(entry => entry.Unsubscribe());
            }
        }

        private void AppendEntry(IEntry newEntry)
        {
            if (_eventActionEntries.Exists(entry => entry.EventName == newEntry.EventName))
            {
                throw new ArgumentException($"{newEntry.EventName} is already in registry");
            }

            // explicitly enforce that any new event-handler pairs have a subscription state matching the
            // rest of the event-handler pairs in the registry
            if (_active)
            {
                newEntry.Subscribe();
            }
            else
            {
                newEntry.Unsubscribe();
            }

            _description += $"{newEntry.EventName}=>{newEntry.HandlerName};";
            _eventActionEntries.Add(newEntry);
        }
    }
}
