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

  private void SpawnSelf()
  {
    _myCharacter = SpawnPlayer(GetTree().GetNetworkUniqueId(), new Vector2(_rand.Next(0, 1024), _rand.Next(0, 400)));
  }

  private Personagem SpawnPlayer(int id, Vector2 pos)
  {
    var newPlayer = _playerScene.Instance<Personagem>();
    newPlayer.Name = id.ToString();
    newPlayer.Position = pos;
    newPlayer.SetNetworkMaster(id);
    AddChild(newPlayer);
    return newPlayer;
  }

  [Remote]
  private void RegisterPlayer(Vector2 pos)
  {
    var id = GetTree().GetRpcSenderId();
    SpawnPlayer(id, pos);
  }

  private void PlayerConnected(int id)
  {
    RpcId(id, nameof(RegisterPlayer), _myCharacter.Position);
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
    SpawnSelf();
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
