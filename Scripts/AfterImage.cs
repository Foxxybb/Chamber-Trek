using Godot;
using System;

public partial class AfterImage : CharacterBody2D
{
	public AnimatedSprite2D an;

	int imageTickCap = 180; // number of frames until afterImage fully disappears
	int imageTick;

	// afterImage Colors
	byte r = 10;
	byte g = 10;
	byte b = 255;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		an = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		an.SelfModulate = Color.Color8(r,g,b,255);
		imageTick = imageTickCap;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// tick down alpha until 0, then destroy afterimage
		if(imageTick > 0){
			imageTick--;
		} else {
			// destroy afterimage
			this.QueueFree();
		}

		int newTick = (int)(((float)imageTick/(float)imageTickCap)*255);
		//GD.Print(newTick);
		
		an.SelfModulate = Color.Color8(r, g, b, (byte)newTick);
	}
}
