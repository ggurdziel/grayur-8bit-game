using UnityEngine;

// Responsible for the player's move state behavior
public class Player_MoveState : EntityState
{
    public Player_MoveState(Player player, StateMachine stateMachine, string stateName) : base(player, stateMachine, stateName)
    {
    }


    public override void Update()
    {
        base.Update();

        Vector2 move = player.moveInput;
        
        // If player is not moving at all, switch to idle
        if (move.x == 0 && move.y == 0)
        {
            player.SetVelocity(0, 0);
            stateMachine.ChangeState(player.idleState);
            return;
        }

        player.UpdateAnimation(move);
        
        move = move.normalized;

        bool sprintingNow = player.isSprintHeld && player.canSprint;
        float currentSpeed = sprintingNow ? player.sprintSpeed : player.moveSpeed;

        player.SetVelocity(move.x * currentSpeed, move.y * currentSpeed);
    }

}