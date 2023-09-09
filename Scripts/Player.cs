using Godot;
using System;

public partial class Player : CharacterBody2D
{   
	public Vector2 dpad;

	public float JumpVelocity = -525.0f;
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	public float gravityScale = 1;
	// used for controlling falling speed based on player input
	public float baseGravityScale = 2f; // default gravity scale
	public float jumpGravityScale = 1.2f; // increases jump height when jump action is held
	public float stunGravityScale = 3f;

	public float drag = 20;
	public float airDrag = 5;
	public float stunDrag = 10f;

	public float runSpeed = 425;
	public float accel = 15;
	

	public float airSpeed = 400;
	public float airAccel = 7.5f;
	

	public float baseActionDrag = 10;
	
	public float turnThreshold = 350; // threshold of speed to trigger

	public Vector2 dashSpeed = new Vector2(400,0); // velocity Vector2 of dash
	public int dashTime = 16;
	Random rand = new Random();

	//[Header("Attack Info")]
	public string playerStateDebug = "";
	public Action currentAction;
	public bool hitboxActive;
	public bool jumpCancel; // true if an attack connects, allows jump cancel from attacks
	public bool hasJumpKick; // used to limit air actions
	public bool hasAirdash; // used to limit air actions

	public int dashStock; // resource player uses for dashes and dash attacks
	public int dashStockCap = 3;
	public int dashRestock; // timer for regaining dash stocks
	public int dashRestockCap = 100; // when dashMeter reaches cap, stock is gained

	//[Header("Other Stuffs")]
	public bool launched;
	public bool animating; // if player animations are currently playing
	[Export] public bool facingRight = true; // used to flip sprite renderer
	public bool dashingRight; // used for dash state
	public int fallTimer; // tracks how long player has been airborne (in frames)
	public bool atDoor;
	public bool spawning; // used for initiating spawn sequence on level start

	int altInt = 1; // alternating int for hitshake
	public float hitshakeIntensity = 0.8f;
	
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

	private bool inHitstun;
	public bool InHitstun{
		get {return inHitstun;}
		set{
			if (value == inHitstun){
				return;
			} else {
				inHitstun = value;
				
				if(inHitstun){
					EnterHitstun();
				} else {
					ExitHitstun();
				}
			}
		}
	}

	// Statemachine and States
	public StateMachine playerSM;
	public Spawn spawn;
	public Idle idle;
	public Run run;
	public Air air;
	public Turn turn;
	public PlayerAction action;
	public Dash dash;
	public Stun stun;
	
	public AnimatedSprite2D an; // public Animator an;
	public Label la; // used to display state
	public HitHandler hh; // handles hitstop/hitstun

	#region // animation names
	// Animation States
	public string anState;
	const string IDLE = "idle";
	const string RUN = "run";
	const string RUNSTART = "runstart";
	const string RUNSTOP = "runstop";
	const string JUMP = "jump";
	const string FALL = "fall";
	const string LAND = "land";
	const string LANDRUN = "landrun";
	const string TURN = "turn";
	const string DASHF = "dashf";
	const string DASHB = "dashb";
	const string AIRDASHF = "airdashf";
	const string AIRDASHB = "airdashb";
	const string STUN = "stun";
	
	#endregion

	public override void _Ready()
	{
		// Add animated sprite component from database as child node, hide sprite placeholder
		this.AddChild(Database.Instance.playerAn.Instantiate());
		an = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		GetNode<Sprite2D>("Sprite2D").Visible = false;

		hh = GetNode<HitHandler>("HitHandler");
		la = GetNode<Label>("StateText");
		
		// state machine
		playerSM = (StateMachine)this.GetNode("StateMachine");
		// each state needs to be a node
		// states
		spawn = (Spawn)this.GetNode("StateMachine/Spawn");
		idle = (Idle)this.GetNode("StateMachine/Idle");
		run = (Run)this.GetNode("StateMachine/Run");
		air = (Air)this.GetNode("StateMachine/Air");
		turn = (Turn)this.GetNode("StateMachine/Turn");
		action = (PlayerAction)this.GetNode("StateMachine/PlayerAction");
		dash = (Dash)this.GetNode("StateMachine/Dash");
		stun = (Stun)this.GetNode("StateMachine/Stun");

		playerSM.Initialize(spawn); 

		la.Visible = Oracle.Instance.myDebug;
	}

	public override void _Process(double delta)
	{
		// pause check
		// if (!Oracle.Instance.paused)
		if (true){
			dpad = Input.GetVector("dpad_left", "dpad_right", "dpad_up", "dpad_down");
			HandleAnimation(); // HandleAnimation needs to be BEFORE state logic to prevent auto transitions overriding current action
			
			playerSM.currentState.UpdateLogic(delta);

			if (la.Text != playerStateDebug){
				la.Text = playerStateDebug;
				// emit state change signal
				Events.Instance.EmitSignal("PlayerStateChange");
			}
			
			// sprite flipping
			an.FlipH = !facingRight;
			//hh.facingRight = facingRight;
			
			HandleHitstop();
			HandleHitstun();
			// GetAirTime();
			// HandleGround();

			if (IsOnFloor()){
				hasAirdash = true;
				hasJumpKick = true;
			}
			UpdateMeter();

			// PHYSICS
			// if (!InHitstop){
			// 	playerSM.currentState.UpdatePhysics(delta);
			// 	MoveAndSlide();
			// }
			//GD.Print(GetPlatformVelocity());
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		// ALL PHYSICS UPDATES ARE TIED TO FRAMERATE _Process (60 fps)
		//player.currentState();
		if (!InHitstop){
			playerSM.currentState.UpdatePhysics(delta);
			MoveAndSlide();
		}
	}

	void UpdateMeter(){
		// update restock every frame
		if (dashStock == dashStockCap){
			dashRestock = 0;
		} else {
			dashRestock++;
		}
		
		// if stock exceeds cap, reset to 1
		if (dashRestock > dashRestockCap){
			dashRestock = 1;
			
			if (dashStock < dashStockCap){
				dashStock++;
			}
		}
	}

	private void _on_area_2d_body_entered(Node2D body)
	{
		//GD.Print("player entered: " + body.GetType().ToString());

		if (body.GetType().ToString() == "Door"){
			Door thisDoor = (Door)body; // get door script
			if (thisDoor.isOpen) atDoor = true;
		}
	}

	private void _on_area_2d_body_exited(Node2D body)
	{
		//GD.Print("player exited: " + body.GetType().ToString());

		if (body.GetType().ToString() == "Door"){
			atDoor = false;
		}
	}

	public void Jump(){
		//SoundManager.Instance.PlayPlayerSound(Database.Instance.jump);
		SoundManager.Instance.PlaySoundAtNode(SoundManager.Instance.player_jump, this, 0);

		// rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
		// rb.velocity = new Vector2(rb.velocity.x,jumpForce);
		
		// playerSM.ChangeState(airborne);
		// grounded = false; // removes edge cases where update is not in sync with unity physics
		// ChangeAnimationState(JUMP);
		// GCROffset(5);

		// //SpawnDustCloud();
	}

	public void Land(){
		SoundManager.Instance.PlaySoundAtNode(SoundManager.Instance.player_land, this, 0);
	}

	//grounded punch combo
	public void Punch(){
		currentAction = Database.Instance.punch1;
		playerSM.ChangeState(action);
	}

	public void JumpKick(){
		if (hasJumpKick){
			currentAction = Database.Instance.jumpkick;
			playerSM.ChangeState(action);
			hasJumpKick = false;
		}
	}

	public void Flip(){
		currentAction = Database.Instance.flip;
		playerSM.ChangeState(action);
		facingRight = !facingRight;
	}

	public void TurnPunch(){
		currentAction = Database.Instance.turnpunch;
		playerSM.ChangeState(action);
	}

	public void LowFlip(){
		if (playerSM.currentState == dash){
			dashStock++;
		}

		if (dashStock > 0){
			dashStock--;
			currentAction = Database.Instance.lowflip;
			playerSM.ChangeState(action);
		} else {
			// play backfire sound
			//SoundManager.Instance.PlayPlayerSound(Database.Instance.blip);
		}
	}

	public void Stomp(){
		// refund stock if stomping from dash
		if (playerSM.currentState == dash){
			dashStock++;
		}

		if (dashStock > 0){
			dashStock--;
			currentAction = Database.Instance.stomp;
			playerSM.ChangeState(action);
		} else {
			// play backfire sound
			//SoundManager.Instance.PlayPlayerSound(Database.Instance.blip);
		}
	}

	public void Dash(bool right){
		// check if there are dash stocks remaining
		if ((dashStock > 0) && hasAirdash){
			hasAirdash = false;
			SpawnAfterImage();

			dashingRight = right;
			playerSM.ChangeState(dash);
			// forward dash
			if((dashingRight && facingRight) || (!dashingRight && !facingRight)){
				if (IsOnFloor()){ ChangeAnimationState(DASHF); }
				else { ChangeAnimationState(AIRDASHF); }
			} else // backward dash 
			{
				if (IsOnFloor()){ ChangeAnimationState(DASHB); }
				else { ChangeAnimationState(AIRDASHB); }
			}

			dashStock--;

			//au.Stream = SoundManager.Instance.dash;
			//au.Play();
			SoundManager.Instance.PlaySoundAtNode(SoundManager.Instance.player_dash, this, 0);
		} else {
			// play backfire sound
			//SoundManager.Instance.PlayPlayerSound(Database.Instance.blip);
		}
	}

	void SpawnAfterImage(){
		// instantiate afterImage
		AfterImage afterImage = (AfterImage)Database.Instance.afterImage.Instantiate();
		// add afterimage to scene tree
		this.GetParent().AddChild(afterImage);
		// set location of afterImage
		afterImage.Position = this.Position;
		// set direction
		if (!facingRight) afterImage.GetNode<AnimatedSprite2D>("AnimatedSprite2D").FlipH = true;
		// set animation and frame of afterImage (should match current player animation)
		afterImage.an.Animation = an.Animation;
		afterImage.an.Frame = an.Frame;
		// slow down and reverse animation (why not?)
		afterImage.an.SpeedScale = 0.5f;
	}

	public void EnterDoor(){
		// enter spawn state
		playerSM.ChangeState(spawn);
		// start scene transition
		Oracle.Instance.ResetScene();
		// StartCoroutine(Oracle.Instance.ResetSceneWithDelay(1f));
		// SoundManager.Instance.PlayPlayerSound(Database.Instance.exit);
	}

	public void EnterHitstop(){
		//GD.Print("player enter hitstop");
		an.Pause();
	}

	public void ExitHitstop(){
		//GD.Print("player exit hitstop");
		//GD.Print(hh.storedVel);
		an.Play();
		// if there is stored vel, release it and set it back to zero
		if (hh.storedVel != Vector2.Zero){
			Velocity = hh.storedVel;
			hh.storedVel = Vector2.Zero;
		}
	}

	public void EnterHitstun(){

	}

	public void ExitHitstun(){
		
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
			
			// hitshake return to original position
			//sprite.transform.localPosition = new Vector3(0,sprite.transform.localPosition.y);
		}
	}

	void HandleHitstun(){
		InHitstun = (hh.inHitstun) ? true : false;

		if (InHitstun && (playerSM.currentState != stun)){
			playerSM.ChangeState(stun);
		}
	}

	// function to count frames when player is falling (changes volume of landing sound)
	void GetAirTime(){
		// // if falling, count frames
		// if (rb.velocity.y < -1){
		//     fallTimer++;
		// } else if ((rb.velocity.y > 0) && (fallTimer > 0)) {
		//     fallTimer--;
		// }
	}

	void HandleGround(){
		// refresh air actions when grounded
		// if (grounded){
		//     hasAirdash = true;
		//     hasJumpKick = true;

		//     // for fall sound to be dynamic, needs a seperate audio source
		//     // play landing sound if fallTimer >= 0, reset
		//     // landing sound is louder if fall is greater
		//     if (fallTimer > 1){
		//         //print(fallTimer + " / " + 60);
		//         //float landVolume = (fallTimer/20f);
		//         // (fallTimer/60f)
		//         SoundManager.Instance.PlayPlayerSound(Database.Instance.land);
		//         fallTimer = 0;
		//         //an.SetTrigger("land");
		//         SpawnDustCloud();
		//     }
		//     //an.SetBool("airborne", false);
		// } else {
		//     //an.SetBool("airborne", true);
		// }
	}

	// function to manually switch animation states
	public void ChangeAnimationState(string newState){

		//stop self interruption
		if (anState == newState){
			// switch statement contains all animations that can cancel into themselves
			switch (newState){
				case DASHF:
					an.Stop();
					break;
				case DASHB:
					an.Stop();
					break;
				case AIRDASHF:
					an.Stop();
					break;
				case AIRDASHB:
					an.Stop();
					break;
				case STUN:
					an.Stop();
					break;
				default:
					return;
			}
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
				case RUNSTART:
					ChangeAnimationState(RUN);
					break;
				case RUNSTOP:
					ChangeAnimationState(IDLE);
					break;
				case JUMP:
					if (IsOnFloor()){
						if (dpad.X != 0){
							ChangeAnimationState(LANDRUN);
						} else {
							ChangeAnimationState(LAND);
						}
					}
					break;
				case FALL:
					if (IsOnFloor()){
						if (dpad.X != 0){
							ChangeAnimationState(LANDRUN);
						} else {
							ChangeAnimationState(LAND);
						}
					}
					break;
				case LAND:
					ChangeAnimationState(IDLE);
					break;
				case LANDRUN:
					ChangeAnimationState(RUN);
					break;
				case TURN:
					if(dpad.X == 0){
						ChangeAnimationState(IDLE);
					} else {
						ChangeAnimationState(RUNSTART);
					}
					break;
				case DASHF:
					if (IsOnFloor()){
						ChangeAnimationState(IDLE);
					}
					break;
				case DASHB:
					if (IsOnFloor()){
						ChangeAnimationState(IDLE);
					}
					break;
				case AIRDASHF:
					if (IsOnFloor() && (dpad.X > 0)){
						ChangeAnimationState(LANDRUN);
					} else if (IsOnFloor()){
						ChangeAnimationState(LAND);
					}
					break;
				case AIRDASHB:
					if (IsOnFloor() && (dpad.X > 0)){
						ChangeAnimationState(LANDRUN);
					} else if (IsOnFloor()){
						ChangeAnimationState(LAND);
					}
					break;
				default:
					break;
			}
		} else {
			// state interrupts
			switch (anState){
				case RUN:
					if (an.Frame == 1){
						SoundManager.Instance.PlaySoundAtNode(SoundManager.Instance.player_step2, this, 4);
					} else if (an.Frame == 20){
						SoundManager.Instance.PlaySoundAtNode(SoundManager.Instance.player_step1, this, 4);
					}
					break;
				case RUNSTOP:
					if (an.Frame == 8){
						SoundManager.Instance.PlaySoundAtNode(SoundManager.Instance.player_step1, this, 4);
					}
					break;
				case JUMP:
					if (IsOnFloor()){
						if (dpad.X != 0){
							ChangeAnimationState(LANDRUN);
						} else {
							ChangeAnimationState(LAND);
						}
					}
					break;
				case FALL:
					if (IsOnFloor()){
						if (dpad.X != 0){
							ChangeAnimationState(LANDRUN);
						} else {
							ChangeAnimationState(LAND);
						}
					}
					break;
				default:
					break;
			}
		}
	}
}
