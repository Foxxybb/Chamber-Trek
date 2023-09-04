using Godot;
using System;

// this state is intended to be used while the player is grounded and is recieving no input

public partial class Dash : State
{
	int dashTimeTick;
    bool groundedDash; // used to determine if grounded or air dash
    int dashDirection;

    public override void _Ready()
    {
        base._Ready();
        player = (Player)GetParent().GetParent();

    }

    public override void Enter()
    {
        base.Enter();
        player.playerStateDebug = "dash";
		groundedDash = player.IsOnFloor();
        dashDirection = (player.dashingRight) ? 1 : -1;
        dashTimeTick = player.dashTime;
        player.an.SelfModulate = Color.Color8(255, 255, 0, 255);
    }

    public override void UpdateLogic(double delta)
    {
        base.UpdateLogic(delta);

		if (dashTimeTick > 0){
            dashTimeTick--;
        }

    //     // Jump
    //     if (jumpAction.triggered){
    //         player.Jump();
    //     }
        
        // ATTACK FROM DASH
        // grounded punch combo
        if (Input.IsActionJustPressed("attack_action")){
            if (player.IsOnFloor()){
                player.Punch();
            } else {
                player.JumpKick();
            }
        }

        // flip or stomp from dash
        if (player.dashingRight && Input.IsActionJustPressed("dashL_action")){
            if (player.IsOnFloor()){
                player.LowFlip();
            } else {
                player.Stomp();
            }
        }
        if (!player.dashingRight && Input.IsActionJustPressed("dashR_action")){
            if (player.IsOnFloor()){
                player.LowFlip();
            } else {
                player.Stomp();
            }
        }

		// dash duration end
        if ((dashTimeTick <= 0) && player.IsOnFloor()){
            stateMachine.ChangeState(player.idle);
        } else if ((dashTimeTick <= 0) && (!player.IsOnFloor())){
            stateMachine.ChangeState(player.air);
        }
    
    }

    public override void UpdatePhysics(double delta)
    {
        base.UpdatePhysics(delta);
        // player velocity in dash state is flat
        player.Velocity = player.dashSpeed*dashDirection;
        
		// dash duration end
        if ((dashTimeTick <= 0) && player.IsOnFloor()){
            stateMachine.ChangeState(player.idle);
        } else if ((dashTimeTick <= 0) && (!player.IsOnFloor())){
            stateMachine.ChangeState(player.air);
        }
			
		// Handle Jump.
		if (Input.IsActionJustPressed("jump_action") && player.IsOnFloor())
		{
            player.Jump();
			player.Velocity = new Vector2(player.Velocity.X, player.JumpVelocity); 
            stateMachine.ChangeState(player.air);
            player.ChangeAnimationState("jump");
		}
        
		
    }

    public override void Exit()
    {
        base.Exit();
        player.an.SelfModulate = Color.Color8(255, 255, 255, 255);
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
