using Godot;
using System;
using System.Collections.Generic;

public class main : Node2D
{
  // Declare member variables here. Examples:

  private PackedScene _playerScene;
  private Personagem _myCharacter;
  private Random _rand = new Random();
  // Essa variável vai servir para associar o id dos jogadores ao objeto de seu personagem.
  private Dictionary<int, Personagem> _playerInfo = new Dictionary<int, Personagem>();
  public Announcer Announcer;
  public Node2D UI;
  public string LocalUsername;
  public List<Personagem> PlayersAlive = new List<Personagem>();

  private Personagem SpawnSelf()
  {
    return SpawnPlayer(GetTree().GetNetworkUniqueId(), new Vector2(_rand.Next(0, 1024), _rand.Next(0, 400)), LocalUsername);
  }

  private Personagem SpawnPlayer(int id, Vector2 pos, string name)
  {
    var newPlayer = _playerScene.Instance<Personagem>();
    newPlayer.Name = name;
    newPlayer.Position = pos;
    newPlayer.SetNetworkMaster(id);
    AddChild(newPlayer);
    if (GetTree().IsNetworkServer())
      PlayersAlive.Add(newPlayer);
    return newPlayer;
  }

  [Remote]
  private void RegisterPlayer(Vector2 pos, string name)
  {
    var id = GetTree().GetRpcSenderId();
    _playerInfo.Add(id, SpawnPlayer(id, pos, name));
  }

  private void PlayerConnected(int id)
  {
    RpcId(id, nameof(RegisterPlayer), _myCharacter.Position, _myCharacter.Name);
  }

  private void PlayerDisconnected(int id)
  {
    // Deletar o personagem quando o jogador desconectar.
    _playerInfo[id].QueueFree();
    _playerInfo.Remove(id);
  }

  private void ServerDisconnected()
  {
    // Voltar ao menu.
    GetTree().ChangeSceneTo(GD.Load<PackedScene>("res://menu.tscn"));
  }

  public void RestartGame()
  {
    PlayersAlive.Clear(); // Limpar a lista de jogadores vivos.
    foreach (var player in _playerInfo.Values)
    {
      PlayersAlive.Add(player); // Readicionar a lista de jogadores vivos.
      player.Rset(nameof(player.Dead), false); // Reviver todos jogadores mortos.
      player.Velocity = new Vector2(0, 0); // Resetar velocidade dos jogadores.
      player.Rset(nameof(player.Velocity), player.Velocity); // Atualizar a velocidade para os outros jogadores online.
      player.Position = new Vector2(_rand.Next(0, 1024), _rand.Next(0, 400)); // Criar uma posição nova para cada um.
      player.Rpc(nameof(player.UpdateRemotePosition), player.Position); // Atualizar a posição para os outros jogadores online.
    }
  }

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    _playerScene = GD.Load<PackedScene>("res://entities/Personagem.tscn");
    Announcer = GetNode<Announcer>("Announcer");
    UI = GetNode<Node2D>("UI");
    GetTree().Connect("network_peer_connected", this, nameof(PlayerConnected));
    GetTree().Connect("network_peer_disconnected", this, nameof(PlayerDisconnected));
    GetTree().Connect("server_disconnected", this, nameof(ServerDisconnected));
    _myCharacter = SpawnSelf();
    _playerInfo.Add(GetTree().GetNetworkUniqueId(), _myCharacter);
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  // public override void _Process(float delta)
  // {
  // }
}
