using Godot;
using System;

public partial class Database : Node
{
	public static Database Instance;

	// player actions
	public Action punch1;
	public Action jumpkick;
	public Action flip;
	public Action turnpunch;
	public Action lowflip;
	public Action stomp;

	// bashbox actions;
	public Action bash;

	// prefabs
	public PackedScene transition;
	public PackedScene audioShot;
	public PackedScene hitbox;
	public PackedScene afterImage;
	public PackedScene mover;

	// sprite prefabs
	public PackedScene playerAn;
	public PackedScene keyAn;
	public PackedScene doorAn;
	public PackedScene lockAn;
	public PackedScene boxAn;
	public PackedScene readyAn;
	public PackedScene bashboxAn;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;

		LoadPlayerActions();
		LoadBashActions();

		LoadPrefabs();

		LoadSprites();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	void LoadPlayerActions(){
		punch1 = (Action)GD.Load("res://Resources/Actions/Player/Punch1.tres");
		jumpkick = (Action)GD.Load("res://Resources/Actions/Player/JumpKick.tres");
		flip = (Action)GD.Load("res://Resources/Actions/Player/Flip.tres");
		turnpunch = (Action)GD.Load("res://Resources/Actions/Player/TurnPunch.tres");
		lowflip = (Action)GD.Load("res://Resources/Actions/Player/LowFlip.tres");
		stomp = (Action)GD.Load("res://Resources/Actions/Player/Stomp.tres");
	}

	void LoadBashActions(){
		bash = (Action)GD.Load("res://Resources/Actions/Bashbox/Bash.tres");
	}

	void LoadPrefabs(){
		hitbox = GD.Load<PackedScene>("res://Prefabs/Hitbox.tscn");
		transition = GD.Load<PackedScene>("res://Prefabs/Transition.tscn");
		afterImage = GD.Load<PackedScene>("res://Prefabs/AfterImage.tscn");
		audioShot = GD.Load<PackedScene>("res://Prefabs/AudioShot.tscn");
		mover = GD.Load<PackedScene>("res://Prefabs/Mover.tscn");
	}

	void LoadSprites(){
		playerAn = GD.Load<PackedScene>("res://Prefabs/SpritePreloads/PlayerAn.tscn");
		keyAn = GD.Load<PackedScene>("res://Prefabs/SpritePreloads/KeyAn.tscn");
		doorAn = GD.Load<PackedScene>("res://Prefabs/SpritePreloads/DoorAn.tscn");
		lockAn = GD.Load<PackedScene>("res://Prefabs/SpritePreloads/LockAn.tscn");
		boxAn = GD.Load<PackedScene>("res://Prefabs/SpritePreloads/BoxAn.tscn");
		readyAn = GD.Load<PackedScene>("res://Prefabs/SpritePreloads/ReadyAn.tscn");
		bashboxAn = GD.Load<PackedScene>("res://Prefabs/SpritePreloads/BashboxAn.tscn");
	}
}
