using System;


namespace PQ.Common
{
    /*
    Lightweight event, as a wrapper over the native c# events, and as an alternative to the much slower UnityEvent.

    Intended for events that are larger in scope (hence game prefix) that can be triggered outside the defining class,
    unlike event Action (which still have a place for when that is desired, like say in an input class).

    Notes
    - Currently only allows single parameter events, which is a non-issue as it's better to use custom objects
    - Can be passed around and invoked listened to outside of the defining scope, unlike event Action
    - For cases where events are not so 'global', ie only an animation component can trigger it, event Action is preferable
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
