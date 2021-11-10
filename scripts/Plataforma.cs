using Godot;
using System;

public class Plataforma : KinematicBody2D
{
  // Declare member variables here. Examples:
  private float _speed = 50f;
  private Vector2 velocity;

  [Remote]
  private void setPosition(Vector2 position)
  {
    Position = position;
  }

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    velocity.y = _speed;
  }

  public override void _Process(float delta)
  {
    if (GetTree().IsNetworkServer())
    {
      if ((Position.y > 500 && velocity.y > 0) || (Position.y < 100 && velocity.y < 0))
      {
        velocity.y = -velocity.y;
      }
      Position += velocity.Normalized() * delta * _speed;
      Rpc("setPosition", Position);
    }
  }
}
