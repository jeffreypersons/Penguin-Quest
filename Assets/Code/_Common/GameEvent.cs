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
    public class GameEvent<EventData>
    {
        private event Action<EventData> _action;

        public int NumListeners { get { return _action.GetInvocationList().Length; } }

        public GameEvent()
        {
            _action = delegate { };
        }

        public void Trigger(in EventData eventData)
        {
            _action.Invoke(eventData);
        }

        public void AddListener(Action<EventData> listener)
        {
            _action += listener;
        }

        /* Add a listener that automatically removes itself just after executing. */
        public void AddAutoUnsubscribeListener(Action<EventData> oneTimeUseListener)
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

        public void RemoveListener(Action<EventData> listener)
        {
            _action -= listener;
        }

        public void StopAllListeners()
        {
            if (_action != null)
            {
                foreach (Delegate d in _action.GetInvocationList())
                {
                    _action -= (Action<EventData>)d;
                }
            }
        }
    }
}
