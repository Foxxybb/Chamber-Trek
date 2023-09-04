using Godot;
using System;

public partial class Stun : State
{

    public override void _Ready()
    {
        base._Ready();
        player = (Player)GetParent().GetParent();
    }

    public override void Enter()
    {
        base.Enter();
        player.playerStateDebug = "stun";
    }

    public override void UpdateLogic(double delta)
    {
        base.UpdateLogic(delta);
        
    }

    public override void UpdatePhysics(double delta)
    {
        base.UpdatePhysics(delta);
        Vector2 velocity = player.Velocity; // stored velocity to apply changes to, before returning to player
        
		if (!player.IsOnFloor())
		{
			velocity.Y += player.gravity * player.gravityScale * (float)delta;
		}

		velocity.X = Mathf.MoveToward(player.Velocity.X, 0, player.drag);
		

        player.Velocity = velocity;
    }

    public override void Exit()
    {
        base.Exit();
    }
    
}
