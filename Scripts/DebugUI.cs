using Godot;
using System;
using System.Collections.Generic;

public partial class DebugUI : Control
{
	Player player;

	List<Label> stateLabelList = new List<Label>();
	List<Label> inputLabelList = new List<Label>();
	public Vector2 dpad;
	string storedInput = "";
	int frameCount = 1;
	Label hSpeedLabel;
	Label vSpeedLabel;
	Label stockLabel;
	Label restockLabel;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Events.Instance.PlayerStateChange += UpdateStateUI;
		//eh.Connect("PlayerStateChange", Callable.From(UpdateStateUI));

		player = GetNode<Player>("/root/Scene/Player");

		hSpeedLabel = GetNode<Label>("/root/Scene/DebugUI/Canvas/SpeedDebug/HSpeed/");
		vSpeedLabel = GetNode<Label>("/root/Scene/DebugUI/Canvas/SpeedDebug/VSpeed/");

		stockLabel = GetNode<Label>("/root/Scene/DebugUI/Canvas/MeterDebug/Stock/");
		restockLabel = GetNode<Label>("/root/Scene/DebugUI/Canvas/MeterDebug/Restock/");

		// get playerstate gui nodes
		var stateLabels = GetNode<Node>("/root/Scene/DebugUI/Canvas/StateDebug/").GetChildren();
		var inputLabels = GetNode<Node>("/root/Scene/DebugUI/Canvas/InputDebug/").GetChildren();

		// cast each child to label type and add to list to manipulate
		foreach(Node node in stateLabels){
			stateLabelList.Add((Label)node);
		}
		foreach(Node node in inputLabels){
			inputLabelList.Add((Label)node);
		}
		
		this.GetChild<CanvasLayer>(0).Visible = Oracle.Instance.myDebug;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		// read inputs and update UI
		string newInput = GetStringFromInputs();
		
		if(storedInput != newInput){
			storedInput = newInput;
			frameCount = 1;
			// append framecount to input display
			UpdateInputUI(storedInput);
		} else {
			if(frameCount<99){
				frameCount++;
			}
			if(frameCount<10){
				inputLabelList[0].Text = "0" + frameCount + ": " + newInput;
			} else {
				inputLabelList[0].Text = frameCount + ": " + newInput;
			}
		}

		// update player speed UI
		hSpeedLabel.Text = "H:" + Math.Round(player.Velocity.X);
		vSpeedLabel.Text = "V:" + Math.Round(player.Velocity.Y);

		//update player meter UI
		stockLabel.Text = "S:" + player.dashStock;
		restockLabel.Text = "R:" + player.dashRestock;

	}

	// this script should update state UI with a waterfall display
	public void UpdateStateUI(){
		for (int i=stateLabelList.Count-1; i>0; i--){
			stateLabelList[i].Text = stateLabelList[i-1].Text;
		}

		stateLabelList[0].Text = player.playerStateDebug;
	}

	// this script should update recent input UI
	public void UpdateInputUI(string currentInput){
		for (int i=inputLabelList.Count-1; i>0; i--){
			inputLabelList[i].Text = inputLabelList[i-1].Text;
		}

		if(frameCount<10){
				inputLabelList[0].Text = "0" + frameCount + ": " + currentInput;
			} else {
				inputLabelList[0].Text = frameCount + ": " + currentInput;
			}
	}

	string GetStringFromInputs(){
		dpad = Input.GetVector("dpad_left", "dpad_right", "dpad_up", "dpad_down");
		string inputString = "";

		if(dpad.X < 0){
			inputString += "<";
		} else if (dpad.X > 0){
			inputString += ">";
		}

		if(dpad.Y < 0){
			inputString += "^";
		} else if (dpad.Y > 0){
			inputString += "v";
		}

		if(Input.IsActionPressed("jump_action")){
			inputString += "J";
		}
		if(Input.IsActionPressed("attack_action")){
			inputString += "A";
		}
		if(Input.IsActionPressed("dashL_action")){
			inputString += "L";
		}
		if(Input.IsActionPressed("dashR_action")){
			inputString += "R";
		}

		return inputString;
	}

	public override void _ExitTree()
	{
		Events.Instance.PlayerStateChange -= UpdateStateUI;
		base._ExitTree();
	}
}
