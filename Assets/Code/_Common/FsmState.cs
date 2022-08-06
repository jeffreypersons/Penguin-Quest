using System;


namespace PQ.Common
{
    /*
    Representation of a state in a finite state machine.

    Intended to fully encapsulate graphics, animation, and physics needed for any specific state.
    State is entered and exited without any transitional checks - that is, it is entirely up to the call site to
    handle when transition is/is-not allowed to occur. Instead, it's up to the state to determine what the
    per-frame behavior is (or isn't) as callbacks are provided for regular, fixed, and late updates.
    */
    public abstract class FsmState<Fields> : IEquatable<FsmState<Fields>>
        where Fields : Enum
    {
        public readonly string Name;
        public override string ToString() => Name;

        public FsmState(string name) => Name = name;
        public abstract void Enter();
        public abstract void Exit();

        public virtual void Update()      { }
        public virtual void FixedUpdate() { }
        public virtual void LateUpdate()  { }

        public bool Equals(FsmState<Fields> other) => other is not null && Name == other.Name;
        public override bool Equals(object obj) => Equals(obj as FsmState<Fields>);
        public override int GetHashCode() => HashCode.Combine(Name);
        public static bool operator ==(FsmState<Fields> left, FsmState<Fields> right) =>  Equals(left, right);
        public static bool operator !=(FsmState<Fields> left, FsmState<Fields> right) => !Equals(left, right);
    }
}
