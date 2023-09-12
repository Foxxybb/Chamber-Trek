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
        player.an.SelfModulate = Color.Color8(255,0,0,255);

        // cancel sounds
        SoundManager.Instance.CutSoundsOnNode(player);
    }

    public override void UpdateLogic(double delta)
    {
        base.UpdateLogic(delta);

        // exit hitstun
        if (!player.InHitstun){
            player.playerSM.ChangeState(player.idle);
            player.ChangeAnimationState("idle");
        }
        
    }

    public override void UpdatePhysics(double delta)
    {
        base.UpdatePhysics(delta);
        Vector2 velocity = player.Velocity; // stored velocity to apply changes to, before returning to player
        
		if (!player.IsOnFloor())
		{
			velocity.Y += player.gravity * player.stunGravityScale * (float)delta;
		}

		velocity.X = Mathf.MoveToward(player.Velocity.X, 0, player.stunDrag);
		

        player.Velocity = velocity;
    }

    public override void Exit()
    {
        base.Exit();
        player.an.SelfModulate = Color.Color8(255,255,255,255);
    }
    
}
