using Godot;
using System;

// Intended to be used when player is not colliding vertically

public partial class Air : State
{
	float strafeForce; // used to limit player movement when air strafing

	public override void _Ready()
    {
        base._Ready();
        player = (Player)GetParent().GetParent();
    }

	public override void Enter()
	{
		base.Enter();
		player.playerStateDebug = "air";
	}

	public override void UpdateLogic(double delta)
	{
		base.UpdateLogic(delta);

		// lock control if bounced
		if (!player.bounced){
			// gravity of rigidbody changes if jump is held
			// if button is held and moving UP, allow for more height (mario 1 style)
			if (Input.IsActionPressed("jump_action") && (player.Velocity.Y < 0)){
				player.gravityScale = player.jumpGravityScale;
			} else {
				player.gravityScale = player.baseGravityScale;
			}

			// Dash
			if (player.hasAirdash){
				if (Input.IsActionJustPressed("dashR_action") && Input.IsActionJustPressed("dashL_action")){
					player.Stomp();
				}
				else if (Input.IsActionJustPressed("dashR_action")){
					player.Dash(true);
				}
				else if (Input.IsActionJustPressed("dashL_action")){
					player.Dash(false);
				}
			}

			// Jump kick
			if (Input.IsActionJustPressed("attack_action")){
				player.JumpKick();
			}
		}
		
	}

	public override void UpdatePhysics(double delta)
	{
		base.UpdatePhysics(delta);
		Vector2 velocity = player.Velocity; // stored velocity to apply changes to, before returning to player

		if (!player.IsOnFloor())
		{
			velocity.Y += player.gravity * player.gravityScale * (float)delta;
		} 
		else 
		{
			// if player is bounced, only land if moving down
			if (!player.bounced){
				stateMachine.ChangeState(player.idle);
				player.Land();
			} else {
				if (player.Velocity.Y >= 0){
					stateMachine.ChangeState(player.idle);
					player.Land();
				}
			} 
			
		}

		// lock control if bounced
		if (!player.bounced){
			// Get the input direction and handle the movement/deceleration.
			if (dpad.X != 0)
			{
				velocity.X = Mathf.MoveToward(player.Velocity.X, player.airSpeed*((float)Math.Round(dpad.X)), player.airAccel);
			}
			else
			{
				velocity.X = Mathf.MoveToward(player.Velocity.X, 0, player.airDrag);
			}
		}
		
		player.Velocity = velocity;
	}

	public override void Exit()
	{
		base.Exit();
	}

}
