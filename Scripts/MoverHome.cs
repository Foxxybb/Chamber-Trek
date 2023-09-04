using Godot;
using System;

public partial class MoverHome : Node2D
{
	AnimatableBody2D mover;

	Node2D goal;
	bool returning;

	[Export] public Vector2 goalOffset;
	const int TILESIZE = 80;

	public override void _Ready()
	{
		// hide placeholder sprite
		ColorRect cr = (ColorRect)this.GetChild(0);
		cr.Visible = false;

		// create mover, add as child, set position to home
		mover = (AnimatableBody2D)Database.Instance.mover.Instantiate();
		this.AddChild(mover);
		goal = new Node2D();
		this.AddChild(goal);
		goal.Position = new Vector2(goal.Position.X+(TILESIZE*goalOffset.X), goal.Position.Y+(TILESIZE*goalOffset.Y));

	}

	public override void _Process(double delta)
	{
		
	}

	public override void _PhysicsProcess(double delta)
	{
		// update mover position
		if (!returning){
			mover.GlobalPosition = mover.GlobalPosition.MoveToward(goal.GlobalPosition, 2f);
			if (mover.GlobalPosition == goal.GlobalPosition){returning = true;}
		} else {
			mover.GlobalPosition = mover.GlobalPosition.MoveToward(this.GlobalPosition, 2f);
			if (mover.GlobalPosition == this.GlobalPosition){returning = false;}
		}
	}

}
