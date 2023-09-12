using Godot;
using System;

public partial class Bashbox : CharacterBody2D
{
	[Export] public bool facingRight = true;
	public bool waking;

	public float drag = 10;
	public float airDrag = 5;
	public float gravScale = 1.5f;

	public float baseActionDrag = 10;
	public float baseGravityScale = 2f; // default gravity scale

	public Action currentAction;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	// components
	public HitHandler hh; // handles hitstop/hitstun
	public AnimatedSprite2D an;
	public Label la; // used to display state
	public Area2D vision; // wakes
	public Area2D aggro; // triggers attack

	int altInt = 1; // alternating int for hitshake
	public float hitshakeIntensity = 0.8f;
	Random rand = new Random();

	// used to pause animator and extend attack duration
	private bool inHitstop; 
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


	Node2D playerNode; // enemies have player node in memory

	// state machine
	public string stateDebug;
	public StateMachine bashboxSM;
	public BashSleep sleep;
	public BashIdle idle;
	public BashAction action;
	public BashStun stun;

	#region // animations
	public string anState;
	const string SLEEP = "sleep";
	const string WAKE = "wake";
	const string IDLE = "idle";
	const string STUN = "stun";
	#endregion

	public override void _Ready()
	{
		la = GetNode<Label>("StateText");
		la.Visible = Oracle.Instance.myDebug;

		hh = GetNode<HitHandler>("HitHandler");
		vision = GetNode<Area2D>("Vision");
		aggro = GetNode<Area2D>("Aggro");

		playerNode = GetNode<Node2D>("/root/Scene/Player/");

		// Add animated sprite component from database as child node, hide sprite placeholder
		this.AddChild(Database.Instance.bashboxAn.Instantiate());
		an = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		GetNode<Sprite2D>("Sprite2D").Visible = false;

		// state machine
		bashboxSM = (StateMachine)GetNode("StateMachine");
		sleep = (BashSleep)GetNode("StateMachine/BashSleep");
		idle = (BashIdle)GetNode("StateMachine/BashIdle");
		action = (BashAction)GetNode("StateMachine/BashAction");
		stun = (BashStun)GetNode("StateMachine/BashStun");


		ChangeAnimationState(SLEEP);
		bashboxSM.Initialize(sleep);
	}

	public override void _Process(double delta)
	{
		bashboxSM.currentState.UpdateLogic(delta);

		// if player detected
		if(vision.OverlapsBody(playerNode)){
			//GD.Print("player near");
			if (bashboxSM.currentState == sleep){
				
				if (!waking){
					// turn around if player is behind
					if(playerNode.Position.X > Position.X){
						facingRight = true;
					} else if (playerNode.Position.X < Position.X){
						facingRight = false;
					}
					ChangeAnimationState(WAKE);

					SoundManager.Instance.PlaySoundOnNode(SoundManager.Instance.bashbox_wake, this, 0);
					waking = true;
				}
			}
		}

		if (aggro.OverlapsBody(playerNode)){
			if (bashboxSM.currentState == idle){
				// turn around if player is behind
				if(playerNode.Position.X > Position.X){
					facingRight = true;
				} else if (playerNode.Position.X < Position.X){
					facingRight = false;
				}
				// change to bash action
				currentAction = Database.Instance.bash;
				bashboxSM.ChangeState(action);
			}
		}

		// update state text
		if (la.Text != stateDebug){
			la.Text = stateDebug;
		}

		// sprite flipping
		an.FlipH = !facingRight;

		HandleAnimation();

		HandleHitstun();
		HandleHitstop();
	}

	public override void _PhysicsProcess(double delta)
	{
		bashboxSM.currentState.UpdatePhysics(delta);
		//HandlePhysics(delta);
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

	public void EnterHitstun(){

	}

	public void ExitHitstun(){
		
	}

	private void _on_area_2d_body_entered(Node2D body)
	{
		//GD.Print("bb entered with: " + body.GetType().ToString());

		// if (body.GetType().ToString() == "Player"){
		// 	//GD.Print("Player detected");
		// 	//bashboxSM.ChangeState(IDLE);
		// 	if (bashboxSM.currentState == sleep){
		// 		ChangeAnimationState(WAKE);
		// 	}
			
		// }

		// if (body == playerNode){
		// 	//GD.Print("player detected");
		// 	if (bashboxSM.currentState == sleep){
		// 		ChangeAnimationState(WAKE);
		// 	} else if (bashboxSM.currentState == idle){
		// 		// change to bash action
		// 		bashboxSM.ChangeState(action);
		// 		//ChangeAnimationState(STUN);
		// 	}
		// }
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

	void HandleHitstun(){
		InHitstun = (hh.inHitstun) ? true : false;

		if (InHitstun && bashboxSM.currentState != stun){
			bashboxSM.ChangeState(stun);
		}
	}

	// function to manually switch animation states
	public void ChangeAnimationState(string newState){
		//stop self interruption
		if (anState == newState){
			// switch statement contains all animations that can cancel into themselves
			switch (newState){
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
				case WAKE:
					bashboxSM.ChangeState(idle);
					ChangeAnimationState(IDLE);
					break;
				case SLEEP:
					break;
				default:
					break;
			}
		} else {
			// IsPlaying
			switch (anState){
				case SLEEP:
					break;
				case STUN:
					break;
				default:
					break;
			}
		}
	}

	// THIS IS NOT USED
	void HandlePhysics(double delta){
		if (!InHitstop){
			//bashboxSM.currentState.UpdatePhysics(delta);

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



