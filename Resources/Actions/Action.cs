using Godot;
using System;

public partial class Action : Resource
{
	[Export] public string name; // used to call animation
	[Export] public AudioStreamWav swingSound;
	[Export] public AudioStreamWav hitSound;
	[Export] public bool cancelSound; // removes all current sound sources on player

	// frame data
	[Export] public int duration; // total duration of action, set to -1 to force action to be cancelled
	[Export] public int startup; // frames before hitbox spawns
	[Export] public int active; // number of frames hitbox is active, set to -1 so hitbox does not expire until action ends

	// physics
	[Export] public float drag;
	[Export] public float gravScale;
	[Export] public Vector2 force;
	[Export] public int forceDelay;
	[Export] public bool resetX; // resets current X velocity before applying new force
	[Export] public bool resetY; // resets current Y velocity before applying new force
	[Export] public float actionSpeed; // max speed that player can influence action (X)
	[Export] public float actionAccel; // speed that player can influence action (X)

	// hitbox properties
	[Export] public Vector2 knockback;
	[Export] public Vector2 selfKnockback;
	[Export] public Vector2 stageKnockback;

	[Export] public int damage;
	[Export] public int hitstun;
	[Export] public int hitstop;

	[Export] public bool launcher; // hitbox of action causes special state on hit for victim
	[Export] public bool reverseHitbox; // hitbox causes reverse knockback if victim is behind owner

	// hitbox size/position
	[Export] public Vector2 size;
	[Export] public Vector2 offset;

	// action properties
	[Export] public bool landCancel; // action ends when player lands
	[Export] public int landFrame; // frame that attack is allowed to land cancel
	[Export] public int holdFrame; // frame that attack must be held to trigger holdAttack

	// action chains
	[Export] public Action nextAction; // action when player interrupts current action with attack button
	[Export] public Action holdAction; // action when player holds attack button
	[Export] public Action releaseAction; // action when player releases attack button
	[Export] public Action autoAction; // action that plays when action ends
}
