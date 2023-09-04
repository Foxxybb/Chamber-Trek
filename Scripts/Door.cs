using Godot;
using System;

public partial class Door : CharacterBody2D
{
	// components
	public HitHandler hh; // handles hitstop/hitstun
	public AnimatedSprite2D an;
	public AnimatedSprite2D lockAn;

	public bool isOpen;

	#region // animations
	public string anState;
	const string CLOSE = "close";
	const string LOCKED = "locked";
	const string UNLOCK = "unlock";
	const string OPEN = "open";
	const string OPENLOOP = "openloop";
	#endregion

	public override void _Ready()
	{
		// Add animated sprite component from database as child node, hide sprite placeholder
		this.AddChild(Database.Instance.doorAn.Instantiate());
		this.AddChild(Database.Instance.lockAn.Instantiate());
		an = GetNode<AnimatedSprite2D>("DoorAn");
		lockAn = GetNode<AnimatedSprite2D>("LockAn");
		GetNode<Sprite2D>("Sprite2D").Visible = false;
		GetNode<Sprite2D>("Sprite2D2").Visible = false;

		ChangeAnimationState(CLOSE);
	}

	public override void _Process(double delta)
	{
		HandleAnimation();
	}

	public override void _PhysicsProcess(double delta)
	{
		
	}

	// function to manually switch animation states
	public void ChangeAnimationState(string newState){

		//stop self interruption
		if (anState == newState){
			return;
		}

		// play animation
		an.Play(newState);
		// also play animation for lock
		lockAn.Play(newState);

		// assign state
		anState = newState;
	}

	// function to automatically transition animations
	void HandleAnimation(){
		
		// auto transitions (on animation end)
		if (!an.IsPlaying())
		{	
			switch (anState){
				case CLOSE:
					ChangeAnimationState(LOCKED);
					break;
				case UNLOCK:
					isOpen = true;
					ChangeAnimationState(OPEN);
					break;
				case OPEN:
					ChangeAnimationState(OPENLOOP);
					break;
				default:
					break;
			}
		} else {
			// state interrupts
			switch (anState){
				default:
					break;
			}
		}


	}
}
