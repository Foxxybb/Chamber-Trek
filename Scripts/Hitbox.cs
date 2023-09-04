using Godot;
using System;

public partial class Hitbox : Node2D
{
    CollisionShape2D hitboxShape;
    public Node2D hitboxOwner; // to prevent hitboxes from affecting owner

    // hitbox properties
    public Action hitboxAction; // containing all the properties needed for interactions

    public Vector2 knockback;
    public Vector2 selfKnockback;
    public Vector2 stageKnockback;
    public bool facingRight;

    public int damage;
    public int hitstop;
    public int hitstun;

    public override void _Ready()
    {
        hitboxShape = GetChild<Area2D>(0).GetChild<CollisionShape2D>(0);
    }

    public override void _Process(double delta)
    {
        QueueRedraw(); // hitbox for debugging
    }

    // child area2D signal
    void _on_area_2d_body_entered(Node2D body)
    {
        //GD.Print("hitbox entered: " + body.GetType().ToString());

		if (body != hitboxOwner)
		{
			HitHandler ownerHH = hitboxOwner.GetNode<HitHandler>("HitHandler");
			HitHandler victimHH = body.GetNodeOrNull<HitHandler>("HitHandler"); // not all bodies have hithandler
			
			switch (body.GetType().ToString())
			{
				case "Player":
					GD.Print("player hit");
					break;
				case "BaseTileMap":
					if (stageKnockback != Vector2.Zero)
					{
						CharacterBody2D cb = (CharacterBody2D)hitboxOwner;
						cb.Velocity = stageKnockback;
						// play stagebounce sound
						SoundManager.Instance.PlaySoundAtNode(SoundManager.Instance.wallbounce, hitboxOwner, -1);
					}
					break;
				case "Box":
					ownerHH.ApplyHit(0, hitstop, selfKnockback);
					victimHH.ApplyHit(hitstun, hitstop, knockback);
					SoundManager.Instance.PlaySoundAtNode(hitboxAction.hitSound, hitboxOwner, 0);
					break;
				case "Key":
					Key key = (Key)victimHH.GetParent();
					// only allow hit if key is not unlocking
					if (!key.unlocking)
					{
						ownerHH.ApplyHit(0, hitstop, selfKnockback);
						victimHH.ApplyHit(hitstun, hitstop, knockback);
						// play key note
						SoundManager.Instance.PlaySoundAtNode(SoundManager.Instance.GetKeyNote(), key, 0);
					}
					break;
				case "Bashbox":
					ownerHH.ApplyHit(0, hitstop, selfKnockback);
					victimHH.ApplyHit(hitstun, hitstop, knockback);
					SoundManager.Instance.PlaySoundAtNode(hitboxAction.hitSound, hitboxOwner, 0);
					break;
				default:
					break;

			}
		}
    }

    public override void _Draw()
    {
        if (Oracle.Instance.myDebug)
        {
            DrawRect(hitboxShape.Shape.GetRect(), new Color(255, 0, 0, 0.8f));
        }
    }

    public void DestroyHitbox()
    {
        this.QueueFree();
    }
}
