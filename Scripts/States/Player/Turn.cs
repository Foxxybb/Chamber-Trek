using Godot;
using System;

// this state is intended to be used while the player is grounded and is recieving no input

public partial class Turn : State
{
    int turnDuration = 16; // number of frames that turnaround lasts
    int turnTick;

    public override void _Ready()
    {
        base._Ready();
        player = (Player)GetParent().GetParent();
    }

    public override void Enter()
    {
        base.Enter();
        player.playerStateDebug = "turn";
        // flip player (turn them around, duh)
        player.facingRight = !player.facingRight;

        // set tick duration
        turnTick = turnDuration;

        // play turn sound
        SoundManager.Instance.PlaySoundAtNode(SoundManager.Instance.player_turn, player, 0);
    }

    public override void UpdateLogic(double delta)
    {
        base.UpdateLogic(delta);

        if(turnTick > 0){
            turnTick--;
        } else {
            if (dpad.X != 0){
                stateMachine.ChangeState(player.run);
            } else {
                stateMachine.ChangeState(player.idle);
            }
        }

        // if player attempts to change direction again while in turn, cancel turn state
        if ((player.facingRight) && (dpad.X < 0)){
            player.facingRight = !player.facingRight;
            stateMachine.ChangeState(player.run);
        } else if ((!player.facingRight) && (dpad.X > 0)){
            player.facingRight = !player.facingRight;
            stateMachine.ChangeState(player.run);
        }

        // sidejump
        if (Input.IsActionJustPressed("jump_action")){
            player.Flip();
        }

        // turnpunch
        if (Input.IsActionJustPressed("attack_action")){
            player.TurnPunch();
        }

        // Dash
        if (Input.IsActionJustPressed("dashR_action") && Input.IsActionJustPressed("dashL_action")){
            player.LowFlip();
        }
        else if (Input.IsActionJustPressed("dashR_action")){
            player.Dash(true);
        }
        else if (Input.IsActionJustPressed("dashL_action")){
            player.Dash(false);
        }

    }

    public override void UpdatePhysics(double delta)
    {
        base.UpdatePhysics(delta);
        Vector2 velocity = player.Velocity; // stored velocity to apply changes to, before returning to player
        
		// check for ground
		if (!player.IsOnFloor())
        {
			stateMachine.ChangeState(player.air);
            player.ChangeAnimationState("fall");
		}

        // Get the input direction and handle the movement/deceleration.
		if (dpad.X != 0)
		{
            velocity.X = Mathf.MoveToward(player.Velocity.X, player.runSpeed*(dpad.X)*(0.5f), player.accel);
		}
		else
		{
			velocity.X = Mathf.MoveToward(player.Velocity.X, 0, player.drag);
		}

        player.Velocity = velocity;
    }

    public override void Exit()
    {
        base.Exit();
    }
    
}
