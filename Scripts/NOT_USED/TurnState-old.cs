#if tools
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// State used when player changes direction when moving at a certain speed
// Allows access to unique actions

public class Turnaround : State
{
    int turnDuration = 16; // number of frames that turnaround lasts
    int turnTick;
    float backFoot;
    Quaternion backFootRot;
    
    public Turnaround(Player _player, StateMachine _stateMachine) : base(_player, _stateMachine)
    {
        player = _player;
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        base.Enter();
        //player.an.SetTrigger("turn");
        player.an.StopPlayback();
        player.ChangeAnimationState("pawnch_turn");

        // flip player (turn them around, duh)
        player.facingRight = !player.facingRight;

        // set tick duration
        turnTick = turnDuration;

        // the player's intention is to slow down, so increase the drag
        player.rb.drag = player.baseDrag;

        // play skidding sound
        SoundManager.Instance.PlayPlayerSound(Database.Instance.skid);

        // spawn particle at back foot
        if (player.facingRight) {
            backFoot = -0.8f;
            backFootRot = Quaternion.Euler(0,0,-40);
        } 
        else {
            backFoot = 0.8f;
            backFootRot = Quaternion.Euler(0,0,220);
        }
        Vector3 footSparkOffset = new Vector3(backFoot,-1,0);

        GameObject.Instantiate(Database.Instance.groundSpark,
        player.transform.position + footSparkOffset, // position
        backFootRot); // rotation
    }

    public override void HandleInput()
    {
        base.HandleInput();
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        if(turnTick > 0){
            turnTick--;
        } else {
            if (Math.Abs(dpad.x) > 0){
                stateMachine.ChangeState(player.walking);
                // transition to runstart
                //player.ChangeAnimationState(player.RUNSTART);

            } else {
                stateMachine.ChangeState(player.idle);
                // transition to idle animation
                //player.ChangeAnimationState(player.RUNSTOP);
            }

        }

        // if player attempts to change direction again while in turnaround, cancel turnaround state
        if ((player.facingRight) && (dpad.x < 0)) {
            player.facingRight = !player.facingRight;
            stateMachine.ChangeState(player.walking);
            //player.ChangeAnimationState(player.RUNSTART);
        } else if ((!player.facingRight) && (dpad.x > 0)) {
            player.facingRight = !player.facingRight;
            stateMachine.ChangeState(player.walking);
            //player.ChangeAnimationState(player.RUNSTART);
        }

        // read groundcheck for airborne state change
        if (!player.grounded){
            stateMachine.ChangeState(player.airborne);
        }

        // sidejump
        if (jumpAction.triggered){
            player.Flip();
        }

        // turnpunch
        if (attackAction.triggered){
            player.TurnPunch();
        }

        // Dash
        if (dashRAction.triggered && dashLAction.triggered){
            Debug.Log("frame perfect");
            player.LowFlip();
        }
        else if (dashRAction.triggered){
            player.Dash(true);
        }
        else if (dashLAction.triggered){
            player.Dash(false);
        }
    }

    public override void UpdatePhysics()
    {
        base.UpdatePhysics();

        // give the player SOME control over slowing down to add weight to the movement
        player.rb.AddForce(new Vector2(MathF.Round(dpad.x)*player.walkSpeed*(0.5f), 0), ForceMode2D.Force);
    }

    public override void Exit()
    {
        base.Exit();

        
    }
}
#endif
