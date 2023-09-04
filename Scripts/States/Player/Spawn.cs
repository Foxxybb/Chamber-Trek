using Godot;
using System;

public partial class Spawn : State
{
    public override void _Ready()
    {
        base._Ready();
        player = (Player)GetParent().GetParent();
    }

    public override void Enter()
    {
        base.Enter();
        player.playerStateDebug = "spawn";
        player.Velocity = Vector2.Zero;
        player.an.SelfModulate = Color.Color8(0,0,0,255);
        player.ChangeAnimationState("fall");
    }

    public override void UpdateLogic(double delta)
    {
        base.UpdateLogic(delta);
    }

    public override void UpdatePhysics(double delta)
    {
        base.UpdatePhysics(delta);
    }

    public override void Exit()
    {
        base.Exit();
        player.an.SelfModulate = Color.Color8(255,255,255,255);
    }
}
