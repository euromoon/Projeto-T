using Godot;
using System;
using System.Collections.Generic;

public class main : Node2D
{
  // Declare member variables here. Examples:

  private PackedScene _playerScene;
  private Personagem _myCharacter;
  private Random _rand = new Random();
  private Dictionary<int, Vector2> _playerInfo = new Dictionary<int, Vector2>();
  public string LocalUsername;

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
    return newPlayer;
  }

  [Remote]
  private void RegisterPlayer(Vector2 pos, string name)
  {
    var id = GetTree().GetRpcSenderId();
    SpawnPlayer(id, pos, name);
  }

  private void PlayerConnected(int id)
  {
    RpcId(id, nameof(RegisterPlayer), _myCharacter.Position, _myCharacter.Name);
  }

  private void PlayerDisconnected(int id)
  {
    _playerInfo.Remove(id);
    GetNode(id.ToString()).QueueFree();
  }

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    _playerScene = GD.Load<PackedScene>("res://entities/Personagem.tscn");
    GetTree().Connect("network_peer_connected", this, nameof(PlayerConnected));
    GetTree().Connect("network_peer_disconnected", this, nameof(PlayerDisconnected));
    _myCharacter = SpawnSelf();
  }

  //  // Called every frame. 'delta' is the elapsed time since the previous frame.
  /* public override void _Process(float delta)
  {
    if (IsNetworkMaster())
    {
      if (Input.IsActionJustPressed("online_start_game"))
      {

      }
    }
  } */
}
