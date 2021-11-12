using Godot;
using System;

public class Projétil : KinematicBody2D
{
  // Declare member variables here. Examples:
  private Vector2 _direction;
  private Vector2 velocity;
  private Sprite _sprite;
  private Area2D _explosionRange;
  private float _strenght = 500f;
  public Personagem Thrower;
  public bool IsSuperBomb;

  [RemoteSync]
  private void _createExplosionAnimation()
  {
    // criar um sprite novo com a animação de explosão
    var animation = new AnimatedSprite();
    animation.Frames = GD.Load<SpriteFrames>("res://textures/explosao_animacao.tres");
    animation.Play("explode");
    animation.Position = Position;
    animation.SpeedScale = 5f;
    animation.Scale = new Vector2(0.5f, 0.5f);
    animation.Offset = new Vector2(0, -125);
    // iluminação da bomba
    var light = new Light2D();
    light.Texture = GD.Load<Texture>("res://textures/light.png");
    light.TextureScale = .8f;
    light.Energy = .6f;
    light.ShadowEnabled = true;
    animation.AddChild(light);
    // quando acabar a animação, deletar a node
    animation.Connect("animation_finished", animation, "queue_free");
    GetParent().AddChild(animation);
  }

  public void explode()
  {
    Rpc(nameof(_createExplosionAnimation));
    foreach (var body in _explosionRange.GetOverlappingBodies())
    {
      // só expelir se o objeto for um jogador
      switch (body)
      {
        case Personagem player:
          var distance = player.Position - Position;
          // Aumentar em 5% a barra de super-bomba do jogador atingido.
          player.Rset(nameof(player.SuperBombCharge), player.SuperBombCharge + 4f);
          // força = (1 - distancia/200) * 500, a distancia maxima é 200, ou seja, quando a distancia for 200, a força será minima,
          // porém quando for próxima de 0, será alta, sendo 200 o máximo (ou 2000 quando for super-bomba) (1 - 0) * 500.
          var knockbackVelocity = _strenght * new Vector2(1 - (Math.Abs(distance.x) / 200), 1 - (Math.Abs(distance.y) / 200)) * distance.Normalized();
          // Se a bomba pegar o próprio jogador, mudar sua velocidade,
          // se não, pedir ao outro jogador que mude sua velocidade.
          if (player.GetNetworkMaster() == GetTree().GetNetworkUniqueId())
          {
            player.Velocity = knockbackVelocity;
            player.World.UI.GetNode<Label>("Percentage").Text = Math.Ceiling(player.SuperBombCharge) + "%";
            var superBomb = player.World.UI.GetNode<Sprite>("SuperBombForeground");
            superBomb.RegionRect = new Rect2(5f, 0, player.SuperBombCharge / 100 * 24f, 32f);
            return;
          }
          player.RsetId(player.GetNetworkMaster(), nameof(player.Velocity), knockbackVelocity);
          break;
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
    if (IsSuperBomb)
    {
      _sprite.Modulate = Colors.Gold;
      _sprite.Scale *= 1.8f;
      _strenght = 2000;
    }
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
    velocity.y += 7f; // gravidade
    velocity.x -= .1f; // resistência do ar
    _sprite.RotationDegrees += velocity.x * .05f; // rodar
    var collision = MoveAndCollide(velocity * delta);
    // se colidir com algo, explodir
    if (IsNetworkMaster() && collision != null && collision.Collider != Thrower)
    {
      explode();
      Rpc(nameof(DeleteSelf));
    }
  }
}
