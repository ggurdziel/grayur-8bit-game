using UnityEngine;

public class Player_SprintState : EntityState
{
    public Player_SprintState(Player player, StateMachine stateMachine, string stateName) : base(player, stateMachine, stateName)
    {
        
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = 1;
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer <= 0)
        {
            stateMachine.ChangeState(player.moveState);
        }
    }

}