using Godot;
using System;

public partial class Box : CharacterBody2D
{
	public float drag = 10;
	public float airDrag = 5;
	public float gravScale = 1.5f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	// components
	public HitHandler hh; // handles hitstop/hitstun
	public AnimatedSprite2D an;

	int altInt = 1; // alternating int for hitshake
	public float hitshakeIntensity = 0.8f;
	Random rand = new Random();

	private bool inHitstop; // used to pause animator and extend attack duration
	public bool InHitstop{
		get {return inHitstop;}
		set{
			if (value == inHitstop){
				return;
			} else {
				inHitstop = value;
				// if inHitstop becomes true: EnterHitstop, else ExitHitstop
				if(inHitstop){
					EnterHitstop();
				} else {
					ExitHitstop();
				}
			}
		}
	}

	#region // animations
	public string anState;
	const string STILL = "still";
	const string ROTATER = "rotater";
	const string ROTATEL = "rotatel";
	#endregion

	public override void _Ready()
	{
		hh = GetNode<HitHandler>("HitHandler");

		// Add animated sprite component from database as child node, hide sprite placeholder
		this.AddChild(Database.Instance.boxAn.Instantiate());
		an = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		GetNode<Sprite2D>("Sprite2D").Visible = false;

		ChangeAnimationState(STILL);
	}

	public override void _Process(double delta)
	{
		HandleAnimation();
		HandleHitstop();
	}

	public override void _PhysicsProcess(double delta)
	{
		HandlePhysics(delta);
	}

	public void EnterHitstop(){
		//GD.Print("box enter hitstop");
		an.Pause();
	}

	public void ExitHitstop(){
		an.Play();
		// if there is stored vel, release it and set it back to zero
		if (hh.storedVel != Vector2.Zero){
			Velocity = hh.storedVel;
			hh.storedVel = Vector2.Zero;
		}
		// hitshake return to original position
		an.Offset = new Vector2(0, 0);
	}

	// polls HitHandler (hh) for ticks on hitstop and hitstun, and stored velocity
	void HandleHitstop(){

		InHitstop = (hh.inHitstop) ? true : false;
		
		// hitstop logic, if player is inHitstop, pause animator and attack tickdown
		// when entering hitstop, player velocity is stored, then reapplied when hitstop ends
		// (attack state handles tickdown pause)
		if (InHitstop){
			// hitshake
			if (hh.inHitstun){
				if (IsOnFloor()){
					// grounded hitshake
					an.Offset = new Vector2((hh.hitstop/hitshakeIntensity)*altInt, 0);
					altInt = altInt*(-1);
				} else {
					an.Offset = new Vector2((hh.hitstop/hitshakeIntensity)*altInt, (hh.hitstop/hitshakeIntensity)*rand.Next(-1,1));
					altInt = altInt*(-1);
				}
				
			}

			Velocity = Vector2.Zero;
		} else {

			// SPECIAL CASE WHEN ENEMY IS HIT WHILE IN HITSTOP FROM ANOTHER ENEMY
			// this ALWAYS fires?
			// if ((rb.gravityScale == 0) && (playerSM.currentState == action)){
			//     //Debug.Log("fire");
			//     rb.gravityScale = baseGravityScale;
			//     rb.velocity = hh.backupVel;
			//     hh.backupVel = Vector2.zero;
			// }
			
		}
	}

	// function to manually switch animation states
	public void ChangeAnimationState(string newState){

		//stop self interruption
		if (anState == newState){
			return;
		}

		// play animation
		an.Play(newState);

		// assign state
		anState = newState;
	}

	// function to automatically transition animations
	void HandleAnimation(){
		
		// auto transitions (on animation end)
		if (!an.IsPlaying())
		{	
			switch (anState){
				case STILL:
					if (!IsOnFloor()){
						if (Velocity.X > 0){
							ChangeAnimationState(ROTATER);
						} else if (Velocity.X < 0){
							ChangeAnimationState(ROTATEL);
						}
					}
					break;
				default:
					break;
			}
		} else {
			// state interrupts
			switch (anState){
				case STILL:
					if (!IsOnFloor()){
						if (Velocity.X > 0){
							ChangeAnimationState(ROTATER);
						} else if (Velocity.X < 0){
							ChangeAnimationState(ROTATEL);
						}
					}
					break;
				case ROTATEL:
					if (IsOnFloor()){
						// speed up animation if landed
						an.SpeedScale = 2f;
						if (an.Frame > 24){
							ChangeAnimationState(STILL);
							an.SpeedScale = 1;
						}
						
					}
					break;
				case ROTATER:
					if (IsOnFloor()){
						// speed up animation if landed
						an.SpeedScale = 2f;
						if (an.Frame > 24){
							ChangeAnimationState(STILL);
							an.SpeedScale = 1;
						}
					}
					break;
				default:
					break;
			}
		}
	}

	void HandlePhysics(double delta){
		if (!InHitstop){
			Vector2 velocity = Velocity;

			// Add the gravity.
			if (!IsOnFloor()){
				velocity.Y += gravity * gravScale * (float)delta;

				velocity.X = Mathf.MoveToward(Velocity.X, 0, airDrag);
			} else {
				velocity.X = Mathf.MoveToward(Velocity.X, 0, drag);
			}

			Velocity = velocity;
			MoveAndSlide();
		}
	}
}
