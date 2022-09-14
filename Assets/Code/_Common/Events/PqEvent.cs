

namespace PQ.Common.Events
{
    /*
    Contract for parameter-less events used throughout our game.
    
    Example Usage:
    - public event PqEvent myEvent; // event keyword restricts invoke/assignment to enclosing scope
    - myEvent?.Invoke();            // triggers if any listeners are registered
    */
    public delegate void PqEvent();

    /*
    Contract for single parameter events used throughout our game.

    Example Usage:
    - public event PqEvent<myData> myEvent; // event keyword restricts invoke/assignment to enclosing scope
    - myEvent?.Invoke(myDataInstance);      // triggers if any listeners are registered
    */
    public delegate void PqEvent<in T>(T args);
}
