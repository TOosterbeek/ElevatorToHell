using Godot;
using System;

public class Bullet : KinematicBody2D
{

    [Export]
    public int speed = 1200;

    [Export]
    public float knockbackForce = 4.2f;

    [Export]
    public int damage = 5;

    [Export]
    public float maxLifetime = 1f;

    private PackedScene blood = GD.Load<PackedScene>("res://scenes/fx/BloodSpatter.tscn");


    public override void _Ready()
    {
        SceneTreeTimer t = GetTree().CreateTimer(maxLifetime);
        t.Connect("timeout", this, "Destroy");
    }

    public override void _PhysicsProcess(float delta)
    {
        this.Position += Transform.x * speed * delta;
    }

    public void OnHitboxBodyEntered(Godot.Object body)
    {
        if (!(body is Player))
        {
            if (body is IDamageable)
            {
                if (body is IPushable)
                {
                    ((IPushable)body).Push(this.Position.DirectionTo(((Node2D)body).Position), knockbackForce);
                }

                ((IDamageable)body).ApplyDamage(this, damage);
                Node2D instance = blood.Instance<Node2D>();
                ((Node2D)body).AddChild(instance);
            }
            Destroy();
        }

    }

    public void Destroy()
    {
        QueueFree();
    }

}
