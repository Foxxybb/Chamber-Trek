using Godot;
using System;

public partial class PlayerAction : State
{
    int currentActionFrame = 0;
	int duration; // total frames until attack ends
    int startup; // frames before hitbox is active
    int active; // active is used to determine how many frames the generated hitbox
    bool hitboxActive;

    int forceDelay = -1; // countdown in frames to apply force for attack
    float actionDrag; // X
    float actionGravScale; // Y

    bool chain; // used for attack buffering
    bool attackHeld; // used to track if attack button is held to transition to holdAction
    Action thisAction; // used as a short reference so I don't reference player too much
    Action holdAction;
    Action releaseAction;
    Action autoAction;
    Action nextAction; // next attack in sequence

    Hitbox hitbox;

	public override void _Ready()
    {
        base._Ready();
        player = (Player)GetParent().GetParent();
    }

	public override void Enter()
    {
        base.Enter();
        player.playerStateDebug = "action";
        thisAction = player.currentAction;
        if (thisAction.cancelSound){
            SoundManager.Instance.CutSoundsAtNode(player);
        }
        SoundManager.Instance.PlaySoundAtNode(thisAction.swingSound, player, 0);
        currentActionFrame = 0; // just counting how many frames the action has lasted
		duration = player.currentAction.duration;
        startup = player.currentAction.startup;
        active = player.currentAction.active;
        hitboxActive = false;
        attackHeld = true;

        // next attack in sequence
        nextAction = thisAction.nextAction;
        holdAction = thisAction.holdAction;
        releaseAction = thisAction.releaseAction;
        autoAction = thisAction.autoAction;

        player.ChangeAnimationState(thisAction.name);

        // apply action force if forceDelay==0
        if (thisAction.forceDelay==0){
            ApplyActionForce();
        } else {
            forceDelay = thisAction.forceDelay;
        }

        // set actionDrag
        if (thisAction.drag != 0){
            actionDrag = thisAction.drag;
        } else {
            actionDrag = player.baseActionDrag;
        }

        // set actionGrav
        if (player.currentAction.gravScale == 0){
            actionGravScale = player.baseGravityScale;
        } else {
            actionGravScale = thisAction.gravScale;
        }

    }

	public override void UpdateLogic(double delta)
    {
        base.UpdateLogic(delta);
        if (!player.InHitstop) currentActionFrame++; // for debugging

        HandleHoldAction();

        // CHAIN ATTACKS
        // cancel into attack (with attack button)
        if (Input.IsActionJustPressed("attack_action")){
            chain = true;
        }
        // check if current attack has attack chain
        // if player pressed attack, there is a nextAction, and current attack has been active for at least one frame
        if ((nextAction != null) && (chain == true) && ((active+1) < thisAction.active)){
            player.currentAction = nextAction;
            player.ChangeAnimationState(thisAction.name);
            stateMachine.ChangeState(player.action);
            // set hitstop to 0 (exit hitstop) for next action
            player.hh.hitstop = 0;
        }

        if (!player.InHitstop){
            TickDuration();
            TickStartActive();
            TickForce();
        }

        // dash cancelling (does not require hit)
        // if (dashRAction.triggered && dashLAction.triggered){
        //     //Debug.Log("frame perfect");
        //         if (player.grounded){
        //             player.LowFlip();
        //         } else {
        //             player.Stomp();
        //         }
        // } else if (dashLAction.triggered){
        //     player.ExitHitstop();
        //     player.Dash(false);

        //     player.hh.storedVel = Vector2.zero;
        //     //player.currentAction = null;
        // } else if (dashRAction.triggered){
        //     player.ExitHitstop();
        //     player.Dash(true);

        //     player.hh.storedVel = Vector2.zero;
        //     //player.currentAction = null;
        // }

        // Dash
        if (Input.IsActionJustPressed("dashR_action") && Input.IsActionJustPressed("dashL_action")){
            player.hh.hitstop = 0; // cancel hitstop
            if (player.IsOnFloor()){
                player.LowFlip();
            } else {
                player.Stomp();
            }
            
        }
        else if (Input.IsActionJustPressed("dashR_action")){
            player.hh.hitstop = 0; // cancel hitstop
            player.Dash(true);
        }
        else if (Input.IsActionJustPressed("dashL_action")){
            player.hh.hitstop = 0; // cancel hitstop
            player.Dash(false);
        }
	}

	public override void UpdatePhysics(double delta)
    {
        base.UpdatePhysics(delta);
        Vector2 velocity = player.Velocity; // stored velocity to apply changes to, before returning to player

        if (!player.IsOnFloor())
		{
			velocity.Y += player.gravity * actionGravScale * (float)delta;
		} else {
            // Land Cancel if (duration frame > landFrame)
            if (thisAction.landCancel && (currentActionFrame >= thisAction.landFrame))
            {
                if (autoAction != null){
                    player.currentAction = autoAction;
                    stateMachine.ChangeState(player.action);
                } else {
                    stateMachine.ChangeState(player.idle);
                    player.Land();
                    if (dpad.X != 0){
                        player.ChangeAnimationState("landrun");
                    } else {
                        player.ChangeAnimationState("land");
                    }
                }
                
            }
        }

        // action influence
        if ((dpad.X != 0) && thisAction.actionAccel > 0){
            velocity.X = Mathf.MoveToward(player.Velocity.X, thisAction.actionSpeed*((float)Math.Round(dpad.X)), thisAction.actionAccel);
        } else {
            velocity.X = Mathf.MoveToward(player.Velocity.X, 0, actionDrag);
            
        }

        player.Velocity = velocity;
	}

	public override void Exit()
    {
        base.Exit();
        chain = false;
        // destroy hitbox in the case that player is interrupted
        if (hitboxActive){
            hitboxActive = false;
            hitbox.DestroyHitbox();
        }
    }

    void TickStartActive(){
        // tick down startup and active frames
        if (startup > 0){
            startup--;
        } else if ((!hitboxActive) && ((active > 0) || active < 0)){
            // set hitbox active and tick down active
            
            if (!player.InHitstop) active--;
            hitboxActive = true;

            CreateHitbox();
            
        } else if (active > 0) {
            if (!player.InHitstop) active--;
        } else if (active < 0){
            // hitbox does not expire if active == -1
        } else {
            // delete hitbox
            if (hitboxActive){
                hitboxActive = false;
                hitbox.DestroyHitbox();
            }
        }
    }

    void TickDuration(){
        // tick down duration frames
        if (duration > 0){
            if (!player.InHitstop) duration--;
        } else if (duration < 0){
            // attacks with negative duration continue until cancelled
        } else {
            // trigger autoattack
            if (autoAction != null){
                player.currentAction = autoAction;
                stateMachine.ChangeState(player.action);
            } else {
                // leave attack when duration ends
                // end idle if grounded or airborne if in air
                if (player.IsOnFloor()) {
                    stateMachine.ChangeState(player.idle);
                    player.ChangeAnimationState("idle");
                } else {
                    stateMachine.ChangeState(player.air);
                    player.ChangeAnimationState("fall");
                }
            }
        }
    }

    void CreateHitbox(){
        // flip hitbox if player is facing left
        var offset = thisAction.offset;
        if (!player.facingRight){ offset.X = offset.X * -1; }

        // spawn hitbox
        hitbox = (Hitbox)Database.Instance.hitbox.Instantiate();
        player.AddChild(hitbox);
        
        // set owner of hitbox
        hitbox.hitboxOwner = player;

        // set thisAction of hitbox
        hitbox.hitboxAction = thisAction;

        // set shape of hitbox 
        var newHitboxShape = new RectangleShape2D();
        newHitboxShape.Size = new Vector2(thisAction.size.X, thisAction.size.Y);
        hitbox.GetChild<Area2D>(0).GetChild<CollisionShape2D>(0).Shape = newHitboxShape;

        // set position of hitbox
        hitbox.Position = new Vector2(offset.X, offset.Y);

        // set properties of hitbox
        hitbox.facingRight = player.facingRight;
        if (!player.facingRight){
            hitbox.knockback = new Vector2(thisAction.knockback.X*(-1), thisAction.knockback.Y);
            hitbox.selfKnockback = new Vector2(thisAction.selfKnockback.X*(-1), thisAction.selfKnockback.Y);
            hitbox.stageKnockback = new Vector2(thisAction.stageKnockback.X*(-1), thisAction.stageKnockback.Y);
        } else {
            hitbox.knockback = thisAction.knockback;
            hitbox.selfKnockback = thisAction.selfKnockback;
            hitbox.stageKnockback = thisAction.stageKnockback;
        }
        hitbox.hitstop = thisAction.hitstop;
        hitbox.hitstun = thisAction.hitstun;
        
    }

    void ApplyActionForce(){
        // int used to determine direction 
        int leftRight = 1;
        if (!player.facingRight){ leftRight = -1; }
        
        // these become 0 if resetX/resetY is true
        int resX = 1;
        int resY = 1;

        // add or reset physics
        if (thisAction.resetX){ resX = 0; }
        if (thisAction.resetY){ resY = 0; }

        player.Velocity = new Vector2((resX*player.Velocity.X) + (thisAction.force.X*(leftRight)), (resY*player.Velocity.Y) + thisAction.force.Y);
        // EDGECASE: if (forceDelay == startup+2) of action, set storedVel = actionForce to prevent actionForce from being nullified by hitstop
        if (thisAction.forceDelay == thisAction.startup+2){
            //GD.Print("edgecase");
            player.hh.storedVel = new Vector2((resX*player.Velocity.X) + (thisAction.force.X*(leftRight)), (resY*player.Velocity.Y) + thisAction.force.Y);
        }
    }

    void TickForce(){
        if (forceDelay > 0){
            forceDelay--;
        } else if (forceDelay == 0){
            ApplyActionForce();
            forceDelay--;
        } else {
            // if forceDelay < 0, do nothing
        }
    }

    void HandleHoldAction(){
        // release action
        if (!Input.IsActionPressed("attack_action")){
            attackHeld = false;
            if (releaseAction != null){
                player.currentAction = releaseAction;
                stateMachine.ChangeState(player.action);
            }
        }

        // hold action
        if (holdAction != null){
            // trigger holdAction
            if (attackHeld && (thisAction.holdFrame == currentActionFrame)){
                player.currentAction = holdAction;
                stateMachine.ChangeState(player.action);
            }
        }
        
    }
}
