using Godot;
using System;

public partial class HitHandler : Node2D
{
    CharacterBody2D owner; // owner of this HitHandler
	
    public Vector2 storedVel;
    public Vector2 backupVel;

    public int hitstun;
    public bool inHitstun;

	public int hitstop;
    public bool inHitstop;

    public bool launched; // special state that prevents recovery of stun unless grounded

    public override void _Ready()
	{
        owner = (CharacterBody2D)GetParent();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        HandleHitstun();
		HandleHitstop();
	}

    public void ApplyHit(int newHitstun, int newHitstop, Vector2 newKnockback)
    {
        hitstun += newHitstun; // temp fix for trade hits recovering hitstun
        hitstop = newHitstop;

        // if knockback is 0, get current velocity of owner and store it
        if (newKnockback == Vector2.Zero){
            storedVel = owner.Velocity;
        } else {
            storedVel = newKnockback;
        }
        
    }

    void HandleHitstun(){

        if (hitstun > 0){
            inHitstun = true;
            if (!inHitstop){
                if (!owner.IsOnFloor()){
                    // hitstun will only decay if grounded
                } else {
                    hitstun--;
                }  
            }
        } else {
            inHitstun = false;
        }
    }
    
    void HandleHitstop(){
        
        if ((storedVel != Vector2.Zero)){
            backupVel = storedVel;
        }

        if (hitstop > 0){
            inHitstop = true;
            hitstop--;
        } else {
            inHitstop = false;
        }
    }
}
