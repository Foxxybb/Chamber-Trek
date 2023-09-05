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
		
		// if (player.grounded && player.rb.velocity.y < 1) {
		//     if ((dpad.x > 0)){
		//         stateMachine.ChangeState(player.walking);
		//         player.ChangeAnimationState("pawnch_landrun");
		//         if (!player.facingRight){ player.facingRight = !player.facingRight; }
		//     } else if ((dpad.x < 0)){
		//         stateMachine.ChangeState(player.walking);
		//         player.ChangeAnimationState("pawnch_landrun");
		//         if (player.facingRight){ player.facingRight = !player.facingRight; }
		//     } else if ((dpad.x == 0)){
		//         stateMachine.ChangeState(player.idle);
		//         player.ChangeAnimationState("pawnch_land");
		//     }

		// } else if ((player.rb.velocity.y < 0) && !player.animating){
		//     //player.ChangeAnimationState(player.FALL);
		// }

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
			stateMachine.ChangeState(player.idle);
			player.Land();
		}

		// Get the input direction and handle the movement/deceleration.
		if (dpad.X != 0)
		{
            velocity.X = Mathf.MoveToward(player.Velocity.X, player.airSpeed*((float)Math.Round(dpad.X)), player.airAccel);
			// change animation
			//player.an.Animation = "run";
            //player.ChangeAnimationState("run");
		}
		else
		{
			velocity.X = Mathf.MoveToward(player.Velocity.X, 0, player.airDrag);
			//player.an.Animation = "idle";
            //player.ChangeAnimationState("idle");
		}
		
		// air control
		// limit force applied when player reaches certain speed
		// if (Math.Abs(player.rb.velocity.x) > player.maxSpeed){
		//     strafeForce = MathF.Round(dpad.x)*player.airSpeed*(0.5f);
		// } else {
		//     strafeForce = MathF.Round(dpad.x)*player.airSpeed;
		// }
		// player.rb.AddForce(new Vector2(strafeForce, 0), ForceMode2D.Force);
		player.Velocity = velocity;
	}

	public override void Exit()
	{
		base.Exit();
	}

}
