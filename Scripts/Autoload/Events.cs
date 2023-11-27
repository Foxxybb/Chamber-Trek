using Godot;
using System;

public partial class Events : Node
{
	public static Events Instance;

	// CUSTOM SIGNALS MUST END WITH "EventHandler"

	[Signal] public delegate void GameSceneReadyEventHandler(); // Emitted by GameScene Script when _Ready()
	[Signal] public delegate void PlayerStateChangeEventHandler(); // Emitted by Player when state text changes
	
	[Signal] public delegate void AnnouncerEventHandler(string phrase); // Emitted by: Oracle

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
	}
}
