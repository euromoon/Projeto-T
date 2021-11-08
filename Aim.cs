using Godot;
using System;

public class Aim : Sprite
{
    // Declare member variables here. Examples:
    private KinematicBody2D _player;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _player = GetParent<KinematicBody2D>();
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
    public override void _PhysicsProcess(float delta)
    {
      var mousePos = GetGlobalMousePosition();
      // Atan2 retorna o valor do ângulo (em radianos) no qual a tangente é igual à divisão entre os 2 argumentos.
      // Ou seja, retorna a rotação necessária para o objeto olhar na direção do mouse.
      GlobalRotation = Mathf.Atan2(mousePos.y - GlobalPosition.y, mousePos.x - GlobalPosition.x);
      GlobalScale = new Vector2(.1f, .1f); // Não deformar a mira junto do resto do personagem
    }
}
