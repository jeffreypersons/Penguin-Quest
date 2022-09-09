using System;


namespace PQ.Common
{
    /*
    Lightweight event primitive for game wide usage that's used for triggering and receiving event payloads.
    

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
    public sealed class GameEvent<EventData> : IEquatable<GameEvent<EventData>>
    {
        private readonly string _name;
        private event Action<EventData> _action;

        public string Name => _name;
        public Type EventDataType => typeof(EventData);

        public int ListenerCount => _action.GetInvocationList().Length;
        public override string ToString() =>
            $"{GetType().Name}:{{" +
                $"name:{Name}," +
                $"arg:{EventDataType.FullName}}}";

        public GameEvent(string name)
        {
            _name = name;
            _action = delegate { };
        }

        public void Trigger(in EventData eventData)
        {
            _action.Invoke(eventData);
        }

        public void AddListener(Action<EventData> listener)
        {
            // to guarantee we never hook a listener up to the same event more than once,
            // we ensure that we always unsubscribe in the case it already is before adding it
            _action -= listener;
            _action += listener;
        }

        public void RemoveListener(Action<EventData> listener)
        {
            _action -= listener;
        }


        /* Add a listener that automatically removes itself just after executing. */
        public void AddOneShotListener(Action<EventData> oneTimeUseListener)
        {
            // note null initialization is required to force nonlocal scope of the handler, see https://stackoverflow.com/a/1362244
            Action<EventData> handler = null;
            handler = (data) =>
            {
                _action -= handler;
                oneTimeUseListener.Invoke(data);
            };
            _action += handler;
        }

        public void StopAllListeners()
        {
            foreach (Delegate delegate_ in _action.GetInvocationList())
            {
                _action -= (Action<EventData>)delegate_;
            }
        }


        bool IEquatable<GameEvent<EventData>>.Equals(GameEvent<EventData> other) => other is not null && Name == other.Name;
        public override bool Equals(object obj) => ((IEquatable<GameEvent<EventData>>)this).Equals(obj as GameEvent<EventData>);
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), _action.GetHashCode(), _name);
        public static bool operator ==(GameEvent<EventData> left, GameEvent<EventData> right) =>  Equals(left, right);
        public static bool operator !=(GameEvent<EventData> left, GameEvent<EventData> right) => !Equals(left, right);
    }
}
