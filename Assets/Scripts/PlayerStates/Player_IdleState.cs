using UnityEngine;

// Responsible for the player's idle state behavior
public class Player_IdleState : EntityState
{
    public Player_IdleState(Player player, StateMachine stateMachine, string stateName) : base(player, stateMachine, stateName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        player.SetVelocity(0, rb.linearVelocity.y);
    }


    public override void Update()
    {
        base.Update();

        player.SetVelocity(0, 0);
        player.UpdateAnimation(player.moveInput);

        if (player.moveInput.x != 0 || player.moveInput.y != 0)
        {
            stateMachine.ChangeState(player.moveState);
        }
    }

}
