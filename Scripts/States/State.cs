using Godot;
using System;

// Blueprint for states
public partial class State : Node2D
{
	// users of the state machine (set in each state script)
	public Player player; 
	public Bashbox bashbox;

	public StateMachine stateMachine;

	public Vector2 dpad;

	public override void _Ready()
	{
		stateMachine = (StateMachine)GetParent();
	}

	public virtual void Enter()
	{
		//Debug.Log("User Entered: " + this.ToString());
	}
	public virtual void UpdateLogic(double delta)
	{
		dpad = Input.GetVector("dpad_left", "dpad_right", "dpad_up", "dpad_down");
	}
	public virtual void UpdatePhysics(double delta)
	{
		dpad = Input.GetVector("dpad_left", "dpad_right", "dpad_up", "dpad_down");
	}
	public virtual void Exit(){}
}
