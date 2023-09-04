using Godot;
using System;

public partial class BashIdle : State
{
    public override void _Ready()
    {
        base._Ready();
        bashbox = (Bashbox)GetParent().GetParent();
    }

    public override void Enter()
    {
        base.Enter();
        bashbox.stateDebug = "idle";
        bashbox.waking = false;
    }

    public override void UpdateLogic(double delta)
    {
        base.UpdateLogic(delta);
    }

    public override void UpdatePhysics(double delta)
    {
        base.UpdatePhysics(delta);

        if (!bashbox.InHitstop){

			Vector2 velocity = bashbox.Velocity;

			// Add the gravity.
			if (!bashbox.IsOnFloor()){
				velocity.Y += bashbox.gravity * bashbox.gravScale * (float)delta;

				velocity.X = Mathf.MoveToward(bashbox.Velocity.X, 0, bashbox.airDrag);
			} else {
				velocity.X = Mathf.MoveToward(bashbox.Velocity.X, 0, bashbox.drag);
			}

			bashbox.Velocity = velocity;
			bashbox.MoveAndSlide();
		}
    }

    public override void Exit()
    {
        base.Exit();
    }
}
