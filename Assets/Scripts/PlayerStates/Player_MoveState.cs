using UnityEngine;

// Responsible for the player's move state behavior
public class Player_MoveState : Player_GroundedState
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
            stateMachine.ChangeState(player.idleState);
            return;
        }

        move = move.normalized; // Normalize so diagonal movement is not faster
        player.SetVelocity(move.x * player.moveSpeed, move.y * player.moveSpeed);
    }

}