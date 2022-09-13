/*
Lightweight event primitives for game wide usage that's used for triggering and receiving event payloads.


Intended to provide a unified 'single source of truth' for any game event, rather than an inconsistent mix
of C# delegates, animation events, ui events, input actions, and so forth.

Instead, by triggering a GameEvent when those specific external events occur - eg from an adapter class for
processing player input commands - we have a consistent interface for events throughout the rest of the game.


Features

- Equality: Equality and hashing are supported out of the box, so unlike actions they can be freely used as dictionary keys

- Stateless forwarding: Event data is forwarded to subscribers on trigger rather than stored within the instance

- Cache friendly: Since data is directly forwarded to subscribers, events only need to be newed up once, relieving the GC

- Intentionally restricted to a single param data payload made available to any subscribing actions

- Lightweight: Built on top of native C# event actions, so far more performant than eg UnityEngine.Event as there is
  no need for reflection, serializing, and constant polling in its implementation
*/
using System;


namespace PQ.Common.Events
{
    //
    // Yes, there is duplication here, but it's kept to a minimum and is intentional as a solution
    // for supporting parameter-less events as well as events with args - eg an instance of a struct.
    // Since these events are called heavily, they implement the interfaces directly rather than through
    // a polymorphic base class to avoid the overhead of virtual calls
    // 

    /* Lightweight event primitive for game wide usage used for triggering parameter-less events. */
    public sealed class PqEvent : IEventRaiser, IEventHandler, IEquatable<PqEvent>
    {
        private readonly string _name;
        private event Action _action = delegate { };

        public string Name => _name;
        public PqEvent(string name) => _name = name;

        public void Raise()                            => _action.Invoke();
        public void AddHandler(Action onTrigger)       => _action += onTrigger;
        public void RemoveHandler(Action onTrigger)    => _action -= onTrigger;
        bool IEquatable<PqEvent>.Equals(PqEvent other) => other is not null && Name == other.Name;

        public override string ToString()       => $"Event({_name})";
        public override bool Equals(object obj) => ((IEquatable<PqEvent>)this).Equals(obj as PqEvent);
        public override int GetHashCode()       => HashCode.Combine(base.GetHashCode(), _action.GetHashCode(), Name);

        public static bool operator ==(PqEvent left, PqEvent right) =>  Equals(left, right);
        public static bool operator !=(PqEvent left, PqEvent right) => !Equals(left, right);
    }


    /* Lightweight event primitive for game wide usage used for triggering parameter inclusive events. */
    public sealed class PqEvent<T> : IEventRaiser<T>, IEventHandler<T>, IEquatable<PqEvent<T>>
    {
        private readonly string _name;
        private event Action<T> _action = delegate { };

        public string Name => _name;
        public PqEvent(string name) => _name = name;

        public void Raise(T args)                            => _action.Invoke(args);
        public void AddHandler(Action<T> onTrigger)          => _action += onTrigger;
        public void RemoveHandler(Action<T> onTrigger)       => _action -= onTrigger;
        bool IEquatable<PqEvent<T>>.Equals(PqEvent<T> other) => other is not null && Name == other.Name;

        public override string ToString()       => $"Event<{typeof(T).FullName}>({_name})";
        public override bool Equals(object obj) => ((IEquatable<PqEvent<T>>)this).Equals(obj as PqEvent<T>);
        public override int GetHashCode()       => HashCode.Combine(base.GetHashCode(), _action.GetHashCode(), Name);

        public static bool operator ==(PqEvent<T> left, PqEvent<T> right) =>  Equals(left, right);
        public static bool operator !=(PqEvent<T> left, PqEvent<T> right) => !Equals(left, right);
    }
}
