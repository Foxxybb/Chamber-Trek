using Godot;
using System;

public partial class GameUI : Control
{
	AnimatedSprite2D an;

	public override void _Ready()
	{
		// Add animated sprite component from database as child node, hide sprite placeholder
		GetNode<CanvasLayer>("Canvas").AddChild(Database.Instance.readyAn.Instantiate());
		an = GetNode<AnimatedSprite2D>("/root/Scene/GameUI/Canvas/AnimatedSprite2D/");

		Events.Instance.Announcer += Announcement;
	}

	// function to perform each announcer event,
	// display phrase and/or play sound effect
	void Announcement(string phrase){
		switch(phrase){
			case "ready":
				an.Play(phrase);
				break;
			case "go":
				an.Play(phrase);
				break;
			default:
				break;
		}
	}

	public override void _ExitTree()
	{
		Events.Instance.Announcer -= Announcement;
		base._ExitTree();
	}
}
