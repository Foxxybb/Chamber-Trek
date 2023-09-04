using Godot;
using System;

public partial class PlayerTest : CharacterBody2D
{
	// components
	Oracle oracle;
	AnimatedSprite2D an;

	// fields
	public const float Speed = 500.0f;
	public const float JumpVelocity = -500.0f;

	// animation names

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	public override void _Ready(){
		oracle = GetNode<Oracle>("/root/Oracle");
		an = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}

	public override void _Process(double delta){
		
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor()){
			velocity.Y += gravity * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("jump_action") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
			//oracle.GoToScene("res://Scenes/scene2.tscn");
		}
			
		// Get the input direction and handle the movement/deceleration.
		Vector2 direction = Input.GetVector("dpad_left", "dpad_right", "dpad_up", "dpad_down");
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
			// change animation
			an.Animation = "run";
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			an.Animation = "idle";
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}



