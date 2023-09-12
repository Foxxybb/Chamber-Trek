using Godot;
using System;

public partial class BashAction : State
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
    Action thisAction; // used as a short reference so I don't reference bashbox too much
    Action holdAction;
    Action releaseAction;
    Action autoAction;
    Action nextAction; // next attack in sequence

    Hitbox hitbox;

    public override void _Ready()
    {
        base._Ready();
        bashbox = (Bashbox)GetParent().GetParent();
    }

    public override void Enter()
    {
        base.Enter();
        bashbox.stateDebug = "action";
        thisAction = bashbox.currentAction;

        currentActionFrame = 0; // just counting how many frames the action has lasted
		duration = bashbox.currentAction.duration;
        startup = bashbox.currentAction.startup;
        active = bashbox.currentAction.active;
        hitboxActive = false;

        bashbox.ChangeAnimationState(thisAction.name);

        SoundManager.Instance.PlaySoundOnNode(thisAction.swingSound, bashbox, 0);

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
            actionDrag = bashbox.baseActionDrag;
        }

        // set actionGrav
        if (bashbox.currentAction.gravScale == 0){
            actionGravScale = bashbox.baseGravityScale;
        } else {
            actionGravScale = thisAction.gravScale;
        }
    }

    public override void UpdateLogic(double delta)
    {
        base.UpdateLogic(delta);
        if (!bashbox.InHitstop) currentActionFrame++; // for debugging

        if (!bashbox.InHitstop){
            TickDuration();
            TickStartActive();
            TickForce();
        }
    }

    public override void UpdatePhysics(double delta)
    {
        base.UpdatePhysics(delta);
        Vector2 velocity = bashbox.Velocity;

        if (!bashbox.IsOnFloor())
		{
			velocity.Y += bashbox.gravity * actionGravScale * (float)delta;
		} else {
            // Land Cancel if (duration frame > landFrame)
            if (thisAction.landCancel && (currentActionFrame >= thisAction.landFrame))
            {
                stateMachine.ChangeState(bashbox.sleep);
                bashbox.ChangeAnimationState("sleep");
            }
        }

        velocity.X = Mathf.MoveToward(bashbox.Velocity.X, 0, actionDrag);

        bashbox.Velocity = velocity;
		bashbox.MoveAndSlide();
    }

    public override void Exit()
    {
        base.Exit();
        // destroy hitbox in the case that player is interrupted
        if (hitboxActive){
            hitbox.DestroyHitbox();
            hitboxActive = false;
            
        }
    }

    void ApplyActionForce(){
        // int used to determine direction 
        int leftRight = 1;
        if (!bashbox.facingRight){ leftRight = -1; }
        
        // these become 0 if resetX/resetY is true
        int resX = 1;
        int resY = 1;

        // add or reset physics
        if (thisAction.resetX){ resX = 0; }
        if (thisAction.resetY){ resY = 0; }

        bashbox.Velocity = new Vector2((resX*bashbox.Velocity.X) + (thisAction.force.X*(leftRight)), (resY*bashbox.Velocity.Y) + thisAction.force.Y);
        // EDGECASE: if (forceDelay == startup+2) of action, set storedVel = actionForce to prevent actionForce from being nullified by hitstop
        if (thisAction.forceDelay == thisAction.startup+2){
            //GD.Print("edgecase");
            bashbox.hh.storedVel = new Vector2((resX*bashbox.Velocity.X) + (thisAction.force.X*(leftRight)), (resY*bashbox.Velocity.Y) + thisAction.force.Y);
        }
    }

    void TickStartActive(){
        // tick down startup and active frames
        if (startup > 0){
            startup--;
        } else if ((!hitboxActive) && ((active > 0) || active < 0)){
            // set hitbox active and tick down active
            
            if (!bashbox.InHitstop) active--;
            hitboxActive = true;

            CreateHitbox();
            
        } else if (active > 0) {
            if (!bashbox.InHitstop) active--;
        } else if (active < 0){
            // hitbox does not expire if active == -1
            //if (!hitboxActive) hitboxActive = true;
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
            if (!bashbox.InHitstop) duration--;
        } else if (duration < 0){
            // attacks with negative duration continue until cancelled
        } else {
            // trigger autoattack
            if (autoAction != null){
                bashbox.currentAction = autoAction;
                stateMachine.ChangeState(bashbox.action);
            } else {
                // leave attack when duration ends
                // end idle if grounded or airborne if in air
                stateMachine.ChangeState(bashbox.sleep);
                bashbox.ChangeAnimationState("sleep");
                
                // if (bashbox.IsOnFloor()) {
                //     stateMachine.ChangeState(bashbox.idle);
                //     bashbox.ChangeAnimationState("idle");
                // } else {
                //     stateMachine.ChangeState(bashbox.air);
                //     bashbox.ChangeAnimationState("fall");
                // }
            }
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

    void CreateHitbox(){
        // flip hitbox if player is facing left
        var offset = thisAction.offset;
        if (!bashbox.facingRight){ offset.X = offset.X * -1; }

        // spawn hitbox
        hitbox = (Hitbox)Database.Instance.hitbox.Instantiate();
        bashbox.AddChild(hitbox);
        
        // set owner of hitbox
        hitbox.hitboxOwner = bashbox;

        // set thisAction of hitbox
        hitbox.hitboxAction = thisAction;

        // set shape of hitbox 
        var newHitboxShape = new RectangleShape2D();
        newHitboxShape.Size = new Vector2(thisAction.size.X, thisAction.size.Y);
        hitbox.GetChild<Area2D>(0).GetChild<CollisionShape2D>(0).Shape = newHitboxShape;

        // set position of hitbox
        hitbox.Position = new Vector2(offset.X, offset.Y);

        // set properties of hitbox
        hitbox.facingRight = bashbox.facingRight;
        if (!bashbox.facingRight){
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
}
