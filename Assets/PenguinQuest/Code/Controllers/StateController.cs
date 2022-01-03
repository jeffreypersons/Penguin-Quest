using UnityEditor;
using UnityEngine;
using PenguinQuest.Data;
using System;


public class StateController : MonoBehaviour
{
    private WalkingState walking;
    private SlidingState sliding;

    private GameplayInputReciever input;
    private State CurrentState { get; set; }
    private bool IsCurrently(State state)
    {
        return ReferenceEquals(CurrentState, state);
    }

    private bool CanEnterUprightState => input.Axes.y > 0.0f && !IsCurrently(walking);
    private bool CanEnterOnbellyState => input.Axes.y < 0.0f && !IsCurrently(sliding);


    private void TransitionToState(State newState)
    {
        State oldState = CurrentState;
        Debug.Log($"Transitioning from {oldState} to {newState}");
        oldState.Exit();
        newState.Enter();
        CurrentState = newState;
    }

    void Awake()
    {
        walking = new WalkingState("Walking_State");
        sliding = new SlidingState("Sliding_State");
        CurrentState = walking;
    }

    void Start()
    {

    }
    void Update()
    {
        CurrentState.Update();

        if (CanEnterUprightState)
        {
            TransitionToState(walking);
        }
        else if (CanEnterOnbellyState)
        {
            TransitionToState(sliding);
        }
    }
    void FixedUpdate()
    {
        CurrentState.FixedUpdate();
    }
    void LateUpdate()
    {
        CurrentState.LateUpdate();
    }
}

public abstract class State : IEquatable<State>
{
    public readonly string Name;

    public State(string name)
    {
        Name = name;
    }

    public abstract void Enter();
    public abstract void Exit();
    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void LateUpdate();

    public bool Equals(State other)
    {
        throw new NotImplementedException();
    }

    public override string ToString() => Name;
}

public class WalkingState : State
{
    public WalkingState(string name) : base(name) { }

    public override void Enter()
    {
    }

    public override void Exit()
    {
    }

    public override void FixedUpdate()
    {
    }

    public override void LateUpdate()
    {
    }

    public override void Update()
    {
    }
}

public class SlidingState : State
{
    public SlidingState(string name) : base(name) { }

    public override void Enter()
    {
    }

    public override void Exit()
    {
    }

    public override void FixedUpdate()
    {
    }

    public override void LateUpdate()
    {
    }

    public override void Update()
    {
    }
}
