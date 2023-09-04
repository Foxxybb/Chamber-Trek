using Godot;
using System;
using System.Threading.Tasks;

public partial class Oracle : Node2D
{
	public static Oracle Instance;

	public Node CurrentScene { get; set; }
	string nextScenePath; // used to store path to delay scene change until transition animation finishes

	AnimationPlayer transition;
	Camera cam;

	public bool myDebug = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
		Engine.MaxFps = 60;
		DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled);

		Events.Instance.GameSceneReady += () => OnNewScene();

		Viewport root = GetTree().Root;
		CurrentScene = root.GetChild(root.GetChildCount() - 1);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("reset_action"))
		{
			ResetScene();
		}

		if (Input.IsActionJustPressed("window_action"))
		{
			if (DisplayServer.WindowGetMode() == (DisplayServer.WindowMode.ExclusiveFullscreen)){
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
			} else {
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
			}
		}

		if (Input.IsActionJustPressed("test_action"))
		{
			GD.Print("test");
			
			//cam.SetTarget(GetNode<Door>("/root/Scene/Door/"));
			//Events.Instance.EmitSignal("Announcer", "ready");
			//cam.LerpTarget(CurrentScene.GetNode<Node2D>("Player"));
			//SoundManager.Instance.PlayMusic(SoundManager.Instance.warmup);
			//SoundManager.Instance.PlaySound(SoundManager.Instance.dash);
			cam.ShakeCamera(5);
			
		}

		if (Input.IsActionJustPressed("debug_action"))
		{
			myDebug = !myDebug;
			GD.Print("debug: " + myDebug);
			GetNode<CanvasLayer>("/root/Scene/DebugUI/Canvas").Visible = myDebug;
			//GetNode<Label>("/root/Scene/Player/StateText/").Visible = myDebug;
		}
	}

	public void ResetScene(){
		GD.Print("scene reset");
		// play transition out
		transition.Play("transition_out");
		// reset scene
		nextScenePath = GetTree().CurrentScene.SceneFilePath;
	}

	// Scene change script
	public void GoToScene(string path)
	{
		// call is deferred to prevent crashing when changing scenes
		CallDeferred(nameof(DeferredGotoScene), path);

		// play transition_in animation
		transition.Play("transition_in");
	}

	public void DeferredGotoScene(string path)
	{
		// It is now safe to remove the current scene
		CurrentScene.Free();

		// Load a new scene.
		var nextScene = (PackedScene)GD.Load(path);

		// Instance the new scene.
		CurrentScene = nextScene.Instantiate();

		// Add it to the active scene, as child of root.
		GetTree().Root.AddChild(CurrentScene);

		// Optionally, to make it compatible with the SceneTree.change_scene_to_file() API.
		GetTree().CurrentScene = CurrentScene;
	}

	void OnNewScene(){
		transition = GetNode<AnimationPlayer>("/root/Scene/Camera2D/Transition/TransitionAnimator");
		transition.AnimationFinished += _on_transition_end;
		cam = CurrentScene.GetNode<Camera>("Camera2D");

		// level start sequence
		LevelStartSequence(); 
	}

	void _on_transition_end(StringName anim_name)
	{
		// if transitioning OUT, trigger scene change
		if (anim_name == "transition_out"){
			GoToScene(nextScenePath);
		}
	}

	async void LevelStartSequence(){
		if (myDebug){
			cam.SetTarget(CurrentScene.GetNode<Node2D>("Player"));
			// spawn player
			Player player = CurrentScene.GetNode<Player>("Player");
			player.playerSM.ChangeState(player.air);

		} else {
			// set camera target to door
			cam.SetTarget(GetNode<Door>("/root/Scene/Door/"));
			SoundManager.Instance.PlaySoundAtNode(SoundManager.Instance.door_lock, GetNode<Door>("/root/Scene/Door/"), 0);
			// wait 3 seconds
			await ToSignal(GetTree().CreateTimer(3f), "timeout");
			// play ready, lerp to player, trigger player spawn
			Events.Instance.EmitSignal("Announcer", "ready");
			cam.LerpTarget(CurrentScene.GetNode<Node2D>("Player"));
			await ToSignal(GetTree().CreateTimer(2f), "timeout");
			Events.Instance.EmitSignal("Announcer", "go");
			// spawn player
			Player player = CurrentScene.GetNode<Player>("Player");
			player.playerSM.ChangeState(player.air);
		}
	}
}
