using Godot;
using System;

public class Projétil : KinematicBody2D
{
  // Declare member variables here. Examples:
  private Vector2 _direction;
  private Vector2 velocity;
  private Sprite _sprite;
  private Area2D _explosionRange;

  private void _createExplosionAnimation()
  {
    var animation = new AnimatedSprite();
    animation.Frames = GD.Load<SpriteFrames>("res://textures/explosao_animacao.tres");
    animation.Play("explode");
    animation.Position = Position;
    animation.SpeedScale = 5f;
    animation.Scale = new Vector2(0.5f, 0.5f);
    animation.Offset = new Vector2(0, -125);
    var light = new Light2D();
    light.Texture = GD.Load<Texture>("res://textures/light.png");
    light.TextureScale = .8f;
    light.Energy = .6f;
    light.ShadowEnabled = true;
    animation.AddChild(light);
    animation.Connect("animation_finished", animation, "queue_free");
    GetParent().AddChild(animation);
  }

  public void explode()
  {
    _createExplosionAnimation();
    foreach (var body in _explosionRange.GetOverlappingBodies())
    {
      var player = body as Personagem;
      if (player != null)
      {
        var distance = player.Position - Position;
        // força = (1 - distancia/200) * 500, a distancia maxima é 200, ou seja, quando a distancia for 200, a força será minima,
        // porém quando for próxima de 0, será alta, sendo 500 o máximo (1 - 0) * 500.
        var knockbackVelocity = 500f * new Vector2(1 - (Math.Abs(distance.x) / 200), 1 - (Math.Abs(distance.y) / 200)) * distance.Normalized();
        // Se a bomba pegar o próprio jogador, mudar sua velocidade,
        // se não, pedir ao outro jogador que mude sua velocidade.
        if (player.GetNetworkMaster() == GetTree().GetNetworkUniqueId())
        {
          player.Velocity = knockbackVelocity;
          return;
        }
        player.RsetId(player.GetNetworkMaster(), nameof(player.Velocity), knockbackVelocity);
      }
    }
  }

  [RemoteSync]
  private void DeleteSelf()
  {
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
    if (IsNetworkMaster())
    {
      GetNode<Area2D>("Area2D").Connect("body_entered", this, nameof(_on_Area2D_body_entered));
    }
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
    explode();
    Rpc(nameof(DeleteSelf));
  }
}
