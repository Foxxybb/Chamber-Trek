using Godot;
using System;

// Intended when player is grounded and recieving dpad XOR left/right

public partial class Run : State
{
    float walkForce; // used to limit player movement when walking

    public override void _Ready()
    {
        base._Ready();
        player = (Player)GetParent().GetParent();
    }

    public override void Enter()
    {
        base.Enter();
        player.playerStateDebug = "run";
    }

    public override void UpdateLogic(double delta)
    {
        base.UpdateLogic(delta);
        //Vector2 dpad = Input.GetVector("dpad_left", "dpad_right", "dpad_up", "dpad_down");

        // return player to idle state whenever changing direction
        if (dpad.X == 0){
            RunStop();
        }
        // if player changes direction R>L
        else if ((player.facingRight) && (dpad.X < 0)){
            RunStop();
        }
        // if player changes direction L>R
        else if ((!player.facingRight) && (dpad.X > 0)){
            RunStop();
        }

        // // Jump
        // if (jumpAction.triggered){
        //     player.Jump();
        // }

        // // enter door
        // if ((dpad.y > 0) && player.atDoor){
        //     player.EnterDoor();
        // }

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

        // ATTACK BRANCH
        // shorthop jump kick
        if ((Input.IsActionJustPressed("jump_action")) && Input.IsActionJustPressed("attack_action")){
            player.JumpKick();
        }
        // grounded punch combo
        else if (Input.IsActionJustPressed("attack_action")){
            player.Punch();
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

        // Handle Jump.
		if (Input.IsActionJustPressed("jump_action") && player.IsOnFloor())
		{
			player.Jump();
			velocity.Y = player.JumpVelocity;
            stateMachine.ChangeState(player.air);
            player.ChangeAnimationState("jump");
		}

        // Get the input direction and handle the movement/deceleration.
		if (dpad.X != 0)
		{
            velocity.X = Mathf.MoveToward(player.Velocity.X, player.runSpeed*((float)Math.Round(dpad.X)), player.accel);
			// change animation
			//player.an.Animation = "run";
            //player.ChangeAnimationState("run");
		}
		else
		{
			velocity.X = Mathf.MoveToward(player.Velocity.X, 0, player.drag);
			//player.an.Animation = "idle";
            //player.ChangeAnimationState("idle");
		}

        player.Velocity = velocity;

    }

    public override void Exit()
    {
        base.Exit();

    }

    void RunStop(){
        stateMachine.ChangeState(player.idle);
        player.ChangeAnimationState("runstop");
    }
}
