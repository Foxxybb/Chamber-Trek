#if tools
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Blueprint for states
public class State
{
	// users of the state machine
	public Player player; 
	public Bashbox bashbox;

	public StateMachine stateMachine;

	public InputAction startAction, selectAction, moveAction, jumpAction, attackAction, dashLAction, dashRAction;

	// fields to record input while in state
	public Vector2 dpad;
	public float jumpButton, attackButton, dashLButton, dashRButton;

	public State(Player _player, StateMachine _stateMachine)
	{
		player = _player;
		stateMachine = _stateMachine;

		startAction = player.pi.actions["Start"];
		selectAction = player.pi.actions["Select"];
		moveAction = player.pi.actions["Move"];
		jumpAction = player.pi.actions["Jump"];
		attackAction = player.pi.actions["Attack"];
		dashLAction = player.pi.actions["DashL"];
		dashRAction = player.pi.actions["DashR"];
	}
	public State(Bashbox _bashbox, StateMachine _stateMachine)
	{
		bashbox = _bashbox;
		stateMachine = _stateMachine;

	}

	public virtual void Enter(){
		//Debug.Log("Player Entered: " + this.ToString());
	}
	public virtual void HandleInput(){
		if (player != null){
			dpad = moveAction.ReadValue<Vector2>();
			jumpButton = jumpAction.ReadValue<float>();
			attackButton = attackAction.ReadValue<float>();
			dashLButton = dashLAction.ReadValue<float>();
			dashRButton = dashRAction.ReadValue<float>();
		}
		
	}
	public virtual void UpdateLogic(){}
	public virtual void UpdatePhysics(){}
	public virtual void Exit(){}
}
#endif
