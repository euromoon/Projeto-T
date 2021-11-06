using Godot;
using System;

public class Personagem : KinematicBody2D
{
  // Declare member variables here. Examples:
  private Vector2 velocity = new Vector2(0, 0);
  private int max_speed = 240;
  private float acceleration = 8f;
  private int jumpHeight = 300;
  private int doubleJumpHeight = 250;
  private bool doubleJumpAvailable = true;
  private Sprite sprite;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    sprite = GetNode<Sprite>("Sprite");
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
    if (Input.IsActionPressed("ui_right"))
    {
      velocity.x = Math.Min(velocity.x + acceleration, max_speed);
    }
    else if (Input.IsActionPressed("ui_left"))
    {
      velocity.x = Math.Max(velocity.x - acceleration, -max_speed);
    }
    else if (velocity.x != 0)
    {
      if (velocity.x > 0)
      {
        velocity.x = Math.Max(velocity.x - acceleration * 2.5f, 0);
      }
      else
      {
        velocity.x = Math.Min(velocity.x + acceleration * 2.5f, 0);
      }
    }
    if (!IsOnFloor())
    {
      if (Input.IsActionJustPressed("ui_up"))
      {
        if (doubleJumpAvailable)
        {
          doubleJumpAvailable = false;
          velocity.y = -doubleJumpHeight;
        }
        else
        {
          // Força da gravidade
          velocity.y += 7f;
        }
      }
      else
      {
        velocity.y += 7f;
      }
    }
    else
    {
      doubleJumpAvailable = true;
      // Pulo
      if (Input.IsActionJustPressed("ui_up"))
      {
        velocity.y = -jumpHeight;
      }
      else
      {
        velocity.y = 0;
      }
    }
    MoveAndSlide(velocity, new Vector2(0, -1));
  }
}
