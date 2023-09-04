#if tools
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// this state is intended to be used as a base state for all attacks
// attack states will be states that last for a duration or until cancelled (if possible)
// attack states will spawn hitboxes
// upon entering state, startup begins, ends > active begins, ends > recovery begins, ends > exit state

public class PlayerAction : State
{
    float duration; // total frames until attack ends
    float startup; // frames before hitbox is active
    float active; // active is used to determine how many frames the generated hitbox is active before being destroyed
    int forceDelay; // countdown in frames to apply force for attack

    bool chain; // used for attack buffering
    bool attackHeld; // used to track if attack button is held to transition to holdAction
    Action holdAction;
    Action releaseAction;
    Action autoAction;
    Action nextAction; // next attack in sequence
    
    Vector2 offset; // used to make adjustments to hitbox (such as flipping)
    GameObject currentHitbox;

    int xDir = 0;
    
    public PlayerAction(Player _player, StateMachine _stateMachine) : base(_player, _stateMachine)
    {
        player = _player;
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        base.Enter();
        chain = false;
        if (player.facingRight){ xDir = 1; } else {xDir = -1;}
        forceDelay = player.currentAction.forceDelay;
        player.hh.hitstop = 0;
        player.hh.currentAction = player.currentAction;
        player.jumpCancel = false;
        attackHeld = true;
        if (player.currentAction.GCROS) { player.GCROffset(player.currentAction.forceDelay+5); }
        if (player.currentAction.shakeOnStart) { Oracle.Instance.ShakeCamera(player.currentAction.shakeDuration, player.currentAction.shakeMagnitude); }

        // set and play sound for attack
        player.au.clip = player.currentAction.attackSound;
        player.au.Play();
        
        // upon entering attack state, set frame countdown values from player.currentAction
        startup = player.currentAction.startup;
        active = player.currentAction.active;
        duration = player.currentAction.duration;
        
        // next attack in sequence
        nextAction = player.currentAction.nextAction;
        holdAction = player.currentAction.holdAction;
        releaseAction = player.currentAction.releaseAction;
        autoAction = player.currentAction.autoAction;
        
        // set drag
        if (player.currentAction.drag != 0){
            player.rb.drag = player.currentAction.drag;
        } else {
            player.rb.drag = player.baseDrag;
        }

        // set gravity
        player.rb.gravityScale = player.baseGravityScale;

        player.hh.backupVel = Vector2.zero;
        
    }

    public override void HandleInput()
    {
        base.HandleInput();
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        
        HandleHoldAction();

        // CHAIN ATTACKS
        // cancel into attack (with attack button)
        if (attackAction.triggered){
            chain = true;
            
        }
        // check if current attack has attack chain
        // if player pressed attack, there is a nextAction, and current attack has been active for at least one frame
        if ((nextAction != null) && (chain == true) && ((active+1) < player.currentAction.active)){
            player.currentAction = nextAction;
            player.ChangeAnimationState(player.currentAction.attackAnimationName);
            stateMachine.ChangeState(player.action);
            
        }

        // tick force delay
        if (!player.inHitstop){
                if (forceDelay > 0){
                forceDelay--;
            } else if (forceDelay == 0){
                ApplyActionForce();
                forceDelay--;
            }
            // update forcedelay for debugging
            player.attackForceDelay = forceDelay;
        }

        // instead of dividing attack into startup/active/recovery
        // give each attack a "duration"
        // startup and active are still used for hitbox

        // tick down startup and active frames
        if (startup > 0){
            startup--;
        } else if ((!player.hitboxActive) && ((active > 0) || active < 0)){
            // set hitbox active and tick down active
            if (!player.inHitstop) active--;
            player.hitboxActive = true;

            // flip hitbox if player is facing left
            offset = player.currentAction.offset;
            if (!player.facingRight){ offset.x = offset.x * -1; }

            // spawn hitbox
            currentHitbox = GameObject.Instantiate(Database.Instance.hitbox,
            player.transform.position + (Vector3)offset, // position
            Quaternion.identity, // rotation
            player.transform); // parent

            // set size of hitbox
            currentHitbox.GetComponent<BoxCollider2D>().size = player.currentAction.size;


            // set all other stuff of hitboxes (this is done in hitbox script now)
            //SetHitboxProperties(currentHitbox);
            
        } else if (active > 0) {
            if (!player.inHitstop) active--;
        } else if (active < 0){
            // hitbox does not expire if active == -1
        } else {
            player.hitboxActive = false;
            // delete hitbox
            GameObject.Destroy(currentHitbox);
        }

        // land cancel, cancel air attacks on landing
        if (player.currentAction.landCancel && player.grounded && (player.GCRdelay == 0) && (startup == 0) && (player.rb.velocity.y < 1) && !player.inHitstop){
            //Debug.Log("land cancel");
            if (autoAction != null){
                player.currentAction = autoAction;
                player.ChangeAnimationState(player.currentAction.attackAnimationName);
                stateMachine.ChangeState(player.action);
            } else {
                if (dpad.x != 0){
                    stateMachine.ChangeState(player.walking);
                    player.ChangeAnimationState("pawnch_landrun");
                } else {
                    stateMachine.ChangeState(player.idle);
                    player.ChangeAnimationState("pawnch_land");
                }
            }
            
        }
        //////////////////////////////////////////////

        // tick down duration frames
        if (duration > 0){
            if (!player.inHitstop) duration--;
        } else if (duration < 0){
            // attacks with negative duration continue until cancelled
        } else {
            // trigger autoattack
            if (autoAction != null){
                player.currentAction = autoAction;
                player.ChangeAnimationState(player.currentAction.attackAnimationName);
                stateMachine.ChangeState(player.action);
            } else {
                // leave attack when duration ends
                player.currentAction = null;
                // end idle if grounded or airborne if in air
                if (player.grounded) {
                    stateMachine.ChangeState(player.idle);
                    //player.an.SetTrigger("idle");
                    player.ChangeAnimationState("pawnch_idle1");
                } else {
                    stateMachine.ChangeState(player.airborne);
                }
            }
            
        }
        ///////////////////////////////////////////////

        // jump cancelling (does not require hit)
        // if (jumpAction.triggered && player.grounded && player.jumpCancel){
        //     // exit hitstop, otherwise jump does not come out
        //     player.ExitHitstop();
        //     player.Jump();

        //     player.storedVel = Vector2.zero;
        //     player.currentAction = null;

        //     //SoundManager.Instance.PlayPlayerSound(Database.Instance.jump);
        // }

        // dash cancelling (does not require hit)
        if (dashRAction.triggered && dashLAction.triggered){
            //Debug.Log("frame perfect");
                if (player.grounded){
                    player.LowFlip();
                } else {
                    player.Stomp();
                }
        } else if (dashLAction.triggered){
            player.ExitHitstop();
            player.Dash(false);

            player.hh.storedVel = Vector2.zero;
            //player.currentAction = null;
        } else if (dashRAction.triggered){
            player.ExitHitstop();
            player.Dash(true);

            player.hh.storedVel = Vector2.zero;
            //player.currentAction = null;
        }
    }

    public override void UpdatePhysics()
    {
        base.UpdatePhysics();

        // if attack is steerable, allow player influence
        if (player.currentAction.steerable){
            player.rb.AddForce(new Vector2(MathF.Round(dpad.x)*player.walkSpeed*(0.5f), 0), ForceMode2D.Force);
        }
        
    }

    public override void Exit()
    {
        base.Exit();
        
        // reset attack sequence
        player.attackSequence = 0;
        player.hitboxActive = false;

        // destroy hitbox in the case that player is interrupted
        if (currentHitbox != null){
            GameObject.Destroy(currentHitbox);
        }

        player.ExitHitstop();
    }


    void ApplyActionForce(){
        //Debug.Log("attackforce Applied");
        float newVelX;
        float newVelY;


        if (player.currentAction.resetX) { newVelX = player.currentAction.forceVector.x*xDir; }
        else { newVelX = (player.currentAction.forceVector.x*xDir + player.rb.velocity.x); }

        if (player.currentAction.resetY) { newVelY = player.currentAction.forceVector.y; }
        else { newVelY = (player.currentAction.forceVector.y + player.rb.velocity.y); }

        // if (startup == 1){
        //     player.storedVel = new Vector2(newVelX, newVelY);
        //     player.backupVel = new Vector2(newVelX, newVelY);
        // } else {
        //     player.rb.velocity = new Vector2(newVelX, newVelY);
        // }
        player.rb.velocity = new Vector2(newVelX, newVelY);
        
    }

    void HandleHoldAction(){
        // check if attack button is released early (cancels hold attack)
        if (attackButton == 0){
            attackHeld = false;
            // trigger releaseAction
            if (releaseAction != null){
                player.currentAction = releaseAction;
                player.ChangeAnimationState(player.currentAction.attackAnimationName);
                stateMachine.ChangeState(player.action);
            }
        }

        if (holdAction != null){
            // trigger holdAction
            if (attackHeld && (player.currentAction.holdFrame == (player.currentAction.duration - duration))){
                player.currentAction = holdAction;
                player.ChangeAnimationState(player.currentAction.attackAnimationName);
                stateMachine.ChangeState(player.action);
            }
        }
    }
}
#endif