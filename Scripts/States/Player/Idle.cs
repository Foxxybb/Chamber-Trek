using Godot;
using System;

// this state is intended to be used while the player is grounded and is recieving no input

public partial class Idle : State
{

    public override void _Ready()
    {
        base._Ready();
        player = (Player)GetParent().GetParent();

    }

    public override void Enter()
    {
        base.Enter();
        player.playerStateDebug = "idle";
    }

    public override void UpdateLogic(double delta)
    {
        base.UpdateLogic(delta);

        // transition to turnaround state if speed is certain threshold and player changes direction
        if (Math.Abs(player.Velocity.X) > player.turnThreshold){
            // these if statements are to prevent player "turning around" into the same direction currently moving
            if ((player.facingRight) && (dpad.X < 0) && (player.Velocity.X > 0) && (!Input.IsActionPressed("jump_action"))){
                TurnStart();
            } else if ((!player.facingRight) && (dpad.X > 0) && (player.Velocity.X < 0) && (!Input.IsActionPressed("jump_action"))){
                TurnStart();
            } else {
                // flip character and initiate walk otherwise
                if ((dpad.X < 0)){
                    player.facingRight = false;
                    RunStart();
                } else if ((dpad.X > 0)) {
                    player.facingRight = true;
                    RunStart();
                }
            }
        } else {
            // flip character and initiate walk otherwise
            if ((dpad.X < 0)){
                player.facingRight = false;
                RunStart();
            } else if ((dpad.X > 0)) {
                player.facingRight = true;
                RunStart();
            }
        }

    //     // Jump
    //     if (jumpAction.triggered){
    //         player.Jump();
    //     }

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

            // enter door
        if ((dpad.Y < 0) && player.atDoor){
            player.EnterDoor();
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

    // function to transition to walking state and handle animation
    void RunStart(){
        stateMachine.ChangeState(player.run);
        if (player.anState != "landrun"){
            player.ChangeAnimationState("runstart");
        }
    }

    void TurnStart(){
        // if (jumpAction.triggered){
        //     player.Flip();
        // } else {
        //     stateMachine.ChangeState(player.turnaround);
        //     player.ChangeAnimationState(player.TURN);
        // }
        stateMachine.ChangeState(player.turn);
        player.ChangeAnimationState("turn");
    
    }
}
