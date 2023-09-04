#if tools
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// this state is intended to be used while the player is grounded and is recieving no input

public class Dash : State
{
    //int dashTime = 20; // length (in frames) of dash
    int dashTimeTick;
    bool groundedDash; // used to determine if grounded or air dash
    //bool rightDash; // used to determine direction of dash

    public Dash(Player _player, StateMachine _stateMachine) : base(_player, _stateMachine)
    {
        player = _player;
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        base.Enter();
        //player.an.SetTrigger("dash");
        SoundManager.Instance.PlayPlayerSound(Database.Instance.dash);

        // when player is not attemping movement, reset drag
        player.rb.drag = player.baseDrag;

        // different times for air and ground dash
        if (groundedDash){
            dashTimeTick = player.dashTime;
        } else {
            dashTimeTick = player.dashTime;
        }
        
        groundedDash = player.grounded; // if the dash is grounded properties are different
        //rightDash = player.dashingRight; // insert dash direction
        player.rb.drag = 0;
        if (player.dashingRight){
            player.rb.velocity = new Vector2(player.dashSpeed,0);
        } else {
            player.rb.velocity = new Vector2(-player.dashSpeed,0);
        }
    }

    public override void HandleInput()
    {
        base.HandleInput();
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        if (dashTimeTick > 0){
            dashTimeTick--;
        }

        // if dash is grounded and player becomes airborne: cancel dash state
        // if (groundedDash && !player.grounded){
        //     stateMachine.ChangeState(player.airborne);
        // }
        // if dash is aerial and player becomes grounded: cancel dash state
        // if (!groundedDash && player.grounded){
        //     stateMachine.ChangeState(player.idle);
        // }

        // if dash is aerial, overwrite y velocity for the duration of the dash
        if (!groundedDash){
            player.rb.gravityScale = 0;
            //player.rb.drag = 0;
            player.rb.velocity = new Vector2(player.rb.velocity.x,0);
        }

        // jump and flip
        if (jumpAction.triggered && groundedDash){
            player.Jump();
        } else if (player.dashingRight && dashLAction.triggered){ // back Jump
            // check stock count
            if (player.grounded){
                player.LowFlip();
            } else {
                player.Stomp();
            }
        } else if (!player.dashingRight && dashRAction.triggered){
            if (player.grounded){
                player.LowFlip();
            } else {
                player.Stomp();
            }
        } // chain dash
        else if ((player.dashingRight && dashRAction.triggered && player.grounded)){
            player.Dash(true);
        } else if ((!player.dashingRight && dashLAction.triggered && player.grounded)){
            player.Dash(false);
        }

        // grounded punch combo
        if (attackAction.triggered && player.grounded){
            player.Punch();
        }

        // Jump kick
        if (attackAction.triggered && player.hasJumpKick && !player.grounded) {
            player.hasJumpKick = false;
            player.JumpKick();
        }

        // dash duration end
        if ((dashTimeTick <= 0) && player.grounded){
            stateMachine.ChangeState(player.idle);
        } else if ((dashTimeTick <= 0) && (!player.grounded)){
            stateMachine.ChangeState(player.airborne);
        }
    }

    public override void UpdatePhysics()
    {
        base.UpdatePhysics();
    }

    public override void Exit()
    {
        base.Exit();

        // if air dash recovers in air, slow momentum
        if (!groundedDash){
            player.rb.velocity = new Vector2(player.rb.velocity.x/2, player.rb.velocity.y);
        }
    }
}
#endif