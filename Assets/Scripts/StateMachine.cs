using UnityEngine;

public class StateMachine
{
    public EntityState currentState { get; private set;}


    // Do something (enter, exit) given the current state we are in
    public void Initialize(EntityState startState)
    {
        // assign the current state to the start state (using Enter() method)
        currentState = startState;
        currentState.Enter();
    }


    // Changing from current state to a given new state (newState)
    public void ChangeState(EntityState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }


    public void UpdateActiveState()
    {
        currentState.Update();
    }
}
