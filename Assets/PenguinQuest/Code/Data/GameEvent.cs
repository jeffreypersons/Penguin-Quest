using System;


namespace PenguinQuest.Data
{
    /*
    Lightweight event, as a wrapper over the native c# events, and as an alternative to the much slower UnityEvent.

    Notes
    - Unsupported features: serialization (since delegate body is null), threading, or assignment via Unity inspector
    - Currently only allows single parameter events, which is a non-issue as it's better to use custom objects are preferable
    */
    public class GameEvent<EventData>
    {
        Action<EventData> action;

        public int NumListeners { get { return action.GetInvocationList().Length; } }

        public GameEvent()
        {
            action = delegate { };
        }

        public void Trigger(in EventData eventData)
        {
            action.Invoke(eventData);
        }

        public void AddListener(Action<EventData> listener)
        {
            action += listener;
        }

        /* Add a listener that automatically removes itself just after executing. */
        public void AddAutoUnsubscribeListener(Action<EventData> oneTimeUseListener)
        {
            // note null initialization is required to force nonlocal scope of the handler, see https://stackoverflow.com/a/1362244
            Action<EventData> handler = null;
            handler = (data) =>
            {
                action -= handler;
                oneTimeUseListener.Invoke(data);
            };
            action += handler;
        }

        public void RemoveListener(Action<EventData> listener)
        {
            action -= listener;
        }

        public void StopAllListeners()
        {
            if (action != null)
            {
                foreach (Delegate d in action.GetInvocationList())
                {
                    action -= (Action<EventData>)d;
                }
            }
        }
    }
}
