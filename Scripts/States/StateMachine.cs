using Godot;
using System;

public partial class StateMachine : Node2D
{
    public State currentState;

    public void Initialize(State startingState)
    {
        currentState = startingState;
        startingState.Enter();
    }

    public void ChangeState(State newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }
}