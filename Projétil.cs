using Godot;
using System;

public class Projétil : KinematicBody2D
{
  // Declare member variables here. Examples:
  private Vector2 _direction;
  private Vector2 velocity;
  private Sprite _sprite;
  private Area2D _explosionRange;

  public void explode()
  {
    foreach (var body in _explosionRange.GetOverlappingBodies())
    {
      var player = body as Personagem;
      if (player != null)
      {
        var distance = player.Position - Position;
        player.Velocity = 500f * distance.Normalized();
      }
    }
    QueueFree();
  }

  public void SetProjectileDirection(Vector2 dir, Vector2 throwerVelocity)
  {
    _direction = dir - Position;
    velocity = throwerVelocity + _direction.Normalized() * 350f;
  }

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    _sprite = GetNode<Sprite>("Sprite");
    _explosionRange = GetNode<Area2D>("ExplosionRange");
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
    velocity.y += 7f;
    _sprite.RotationDegrees += velocity.x * .05f;
    MoveAndCollide(velocity * delta);
  }

  public void _on_Area2D_body_entered(Node body)
  {
    CallDeferred(nameof(explode));
  }
}
