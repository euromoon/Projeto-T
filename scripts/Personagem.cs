using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

// Bugs conhecidos:
// 1. Personagens de outros jogadores estão se esticando demais quando caem no chão

public class Personagem : KinematicBody2D
{
  // Declare member variables here. Examples:
  [Remote]
  public Vector2 Velocity = new Vector2(0, 0);
  private int max_speed = 240;
  private float acceleration = 8f;
  private int jumpHeight = 300;
  private int doubleJumpHeight = 250;
  private bool doubleJumpAvailable = true;
  private Sprite sprite;
  private PackedScene projetil = GD.Load<PackedScene>("res://entities/bomba.tscn");
  private List<Vector2> _positionUpdateQueue = new List<Vector2>();
  private int _frameCount = 0;
  private Vector2 _distanceToUpdate = new Vector2(0, 0);
  private int _bombCount = 0;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    sprite = GetNode<Sprite>("Sprite");
    GetNode<Label>("NameLabel").Text = Name;
    if (IsNetworkMaster()) GetNode<Sprite>("Aim").Visible = true;
  }

  [Remote]
  public void _updatePosition(Vector2 pos)
  {
    Position = pos;
  }

  [RemoteSync]
  private void throwBomb(Vector2 dir, string projectileName)
  {
    var bomba = projetil.Instance<Projétil>();
    bomba.AddCollisionExceptionWith(this); // não colidir com o próprio personagem
    bomba.Position = Position;
    bomba.Thrower = this;
    bomba.Name = projectileName;
    GetParent().AddChild(bomba);
    bomba.SetNetworkMaster(GetTree().GetRpcSenderId());
    bomba.SetProjectileDirection(dir, Velocity);
  }

  [RemoteSync]
  private void _announce(string announcement)
  {
    // criar uma caixa de texto
    var announcementLabel = new Label();
    var theme = new Theme();
    // carregar a fonte do texto
    announcementLabel.AddFontOverride("font", GD.Load<Font>("res://other_assets/font.tres"));
    announcementLabel.Text = announcement; // mudar texto na caixa de texto
    announcementLabel.RectSize = new Vector2(1024, 60); // tamanho da caixa do texto
    announcementLabel.RectPivotOffset = new Vector2(512, 0); // colocar o centro da caixa no meio dela
    announcementLabel.Align = Label.AlignEnum.Center; // alinhar o texto com o centro
    var timer = new Timer(); // timer de 2 segundos para apagar o anúncio
    timer.WaitTime = 3;
    timer.OneShot = true; // não reiniciar o timer após acabar
    timer.Autostart = true;
    announcementLabel.AddChild(timer);
    GetParent().AddChild(announcementLabel);
    // conectar o evento do fim do timer com a função que apaga o texto
    timer.Connect("timeout", announcementLabel, "queue_free");
  }

  [RemoteSync]
  private void _die()
  {
    // anunciar a morte do jogador.
    _announce(Name + " foi eliminado");
    QueueFree();
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
    if (IsNetworkMaster())
    {
      if (Position.y > 760)
      {
        Rpc(nameof(_die));
      }
      if (Input.IsActionPressed("ui_right"))
      {
        Velocity.x = Math.Min(Velocity.x + acceleration, max_speed);
      }
      else if (Input.IsActionPressed("ui_left"))
      {
        Velocity.x = Math.Max(Velocity.x - acceleration, -max_speed);
      }
      if (!IsOnFloor())
      {
        if (Input.IsActionJustPressed("ui_up"))
        {
          if (doubleJumpAvailable)
          {
            doubleJumpAvailable = false;
            Scale = new Vector2(1f, 1f);
            Velocity.y = -doubleJumpHeight;
          }
          else
          {
            // Força da gravidade
            Velocity.y += 7f;
          }
        }
        else
        {
          Velocity.y += 7f;
        }
        if (IsOnCeiling())
        {
          // Parar de pular quando bater no teto
          Velocity.y = 0;
        }
        if (IsOnWall())
        {
          Velocity.x = 0;
        }
      }
      else
      {
        Scale = new Vector2(1f, 1f);
        doubleJumpAvailable = true;
        // Pulo
        if (Input.IsActionJustPressed("ui_up"))
        {
          Velocity.y = -jumpHeight;
        }
        else
        {
          Velocity.y = 0;
        }
      }

      var friction = IsOnFloor() ? .4f : .1f;
      if (Velocity.x > 0)
      {
        Velocity.x = Math.Max(Velocity.x - acceleration * friction, 0);
      }
      else
      {
        Velocity.x = Math.Min(Velocity.x + acceleration * friction, 0);
      }

      if (Input.IsActionJustPressed("combat_attack"))
      {
        _bombCount++;
        Rpc(nameof(throwBomb), GetGlobalMousePosition(), GetTree().GetNetworkUniqueId().ToString() + "_bomba" + _bombCount.ToString());
      }

      MoveAndSlide(Velocity, new Vector2(0, -1));
      Rpc(nameof(_updatePosition), Position);
      Rset(nameof(Velocity), Velocity);
    }
    // Deformar a bola quando cair/subir.
    if (Velocity.y > 0)
    {
      Scale = new Vector2(Math.Max(Scale.x - 0.01f, .8f), Math.Min(Scale.y + 0.01f, 1.05f));
    }
    else
    {
      Scale = new Vector2(Math.Min(Scale.x + 0.01f, 1.1f), Math.Max(Scale.y - 0.01f, .9f));
    }
    sprite.RotationDegrees += Velocity.x * 0.04f;
  }
}
