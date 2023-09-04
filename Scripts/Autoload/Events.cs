using Godot;
using System;

public partial class Events : Node
{
	public static Events Instance;
	// SINGLETON class that can be used as an event handler
	// CUSTOM SIGNALS MUST END WITH "EventHandler"

	[Signal] public delegate void GameSceneReadyEventHandler(); // Emitted by GameScene Script when _Ready()
	[Signal] public delegate void PlayerStateChangeEventHandler(); // Emitted by Player when state text changes
	
	[Signal] public delegate void AnnouncerEventHandler(string phrase); // Emitted by: Oracle
	//[Signal] public delegate void SoundEffectEventHandler(AudioStreamWav sound, Node2D node, float baseDB); // Emitted by: Objects that need to play sounds

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}
}
