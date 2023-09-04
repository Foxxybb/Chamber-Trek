using Godot;
using System;

public partial class Camera : Camera2D
{
	Node2D target; // target for camera to track
	bool lerping;
	float lerpSpeed = 0.04f;
	int shakeTick; // ticks down and changes offset of camera
	int altInt = 1;

	// camera clamp borders
	Node2D TL; 
	Node2D BR;
	Vector2 stageMidpoint;
	Node2D midpointNode;

	// parallax background
	//Control parallax;

	// current pixel width and height for the camera zoom level
	float viewW; 
	float viewH;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//create transition child node
		this.AddChild(Database.Instance.transition.Instantiate());

		// get parallax
		//parallax = this.GetNode<Control>("Parallax");

		// get target (currently just player)
		target = (Node2D)GetNode("/root/Scene/Player");

		// get viewport size for clamping
		viewW = GetViewportRect().Size.X;
		viewH = GetViewportRect().Size.Y;
		
		//get borders
		TL = GetNode<Node2D>("/root/Scene/Border/TL");
		BR = GetNode<Node2D>("/root/Scene/Border/BR");
		stageMidpoint = new Vector2((TL.GlobalPosition.X+BR.GlobalPosition.X)/2, (TL.GlobalPosition.Y+BR.GlobalPosition.Y)/2);
		//GD.Print(stageMidpoint);
		// get midpoint of stage, create node at that point, then get globalposition of node to use as pivot point for parallax
		midpointNode = new Node2D();
		midpointNode.Position = stageMidpoint;
		//parallax.GlobalPosition = midpointNode.GlobalPosition;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	public override void _PhysicsProcess(double delta)
	{	
		TrackTarget();
		ScrollParallax();
		HandleShake();
	}

	void TrackTarget(){
		// clamp camera inside boundaries
		// remember that -Y is UP, +Y is DOWN
		float clampX = target.GlobalPosition.X;
		float clampY = target.GlobalPosition.Y;

		clampX = Math.Clamp(clampX, TL.GlobalPosition.X + (viewW/2), BR.GlobalPosition.X - (viewW/2));
		clampY = Math.Clamp(clampY, TL.GlobalPosition.Y + (viewH/2), BR.GlobalPosition.Y - (viewH/2));

		if (!lerping){
			GlobalPosition = new Vector2(clampX, clampY);
		} else {

			GlobalPosition = GlobalPosition.Lerp(new Vector2(clampX, clampY), lerpSpeed);
			// disable lerping once target is reached
			if ((GlobalPosition.X >= target.GlobalPosition.X - 5) && ((GlobalPosition.X <= target.GlobalPosition.X + 5))){
				GD.Print("camera target reached");
				lerping = false;
			}
		}
	}

	void ScrollParallax(){
		//parallax.Position = new Vector2(parallax.Position.X+0.5f, parallax.Position.Y);
		//parallax.Position = new Vector2(this.Position.X-borderMidpoint.X, this.Position.Y);
		//parallax.GlobalPosition = new Vector2(midpointNode.GlobalPosition.X-this.GlobalPosition.X+1080*(0.5f), midpointNode.GlobalPosition.Y-this.GlobalPosition.Y+300);
	}

	// snaps camera to new target
	public void SetTarget(Node2D newTarget){
		target = newTarget;
	}

	// lerps camera to new target
	public void LerpTarget(Node2D newTarget){
		target = newTarget;
		lerping = true;

	}

	public void ShakeCamera(int newShakeTick){
		shakeTick = newShakeTick;
	}

	void HandleShake(){
		if (shakeTick > 0){
			this.Offset = new Vector2(shakeTick*altInt, 0);
			shakeTick--;
			altInt = altInt*(-1); 
		} else {
			this.Offset = Vector2.Zero;
		}
	}
}
