

namespace PenguinQuest.Controllers.Fsm
{
    /*
    Representation of a state in a finite state machine.

    Intended to fully encapsulate graphics, animation, and physics needed for any specific state.
    State is entered and exited without any transitional checks - that is, it is entirely up to the call site to
    handle when transition is/is-not allowed to occur. Instead, it's up to the state to determine what the
    per-frame behavior is (or isn't) as callbacks are provided for regular, fixed, and late updates.
    */
    public abstract class FsmState
    {
        public readonly string Name;

        public FsmState(string name)
        {
            Name = name;
        }

        public abstract void Enter();
        public abstract void Exit();
        public abstract void Update();
        public abstract void FixedUpdate();
        public abstract void LateUpdate();

        public override string ToString() => Name;
    }
}
