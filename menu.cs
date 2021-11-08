using Godot;
using System;

public class menu : Node2D
{
  // Declare member variables here. Examples:
  // private int a = 2;
  private main mainScene;
  private int SERVER_PORT = 59919;
  private int MAX_PLAYERS = 10;
  private NetworkedMultiplayerENet _peer;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    mainScene = GD.Load<PackedScene>("res://main.tscn").Instance<main>();
    _peer = new NetworkedMultiplayerENet();
  }

  private void startGame()
  {
    GetTree().Root.AddChild(mainScene);
    QueueFree();
  }

  public void _on_ClientButton_pressed()
  {
    var ipAddress = GetNode("Options").GetNode<TextEdit>("IPInput").Text;
    if (ipAddress == "") return;
    _peer.CreateClient(ipAddress, SERVER_PORT);
    GetTree().NetworkPeer = _peer;
    startGame();
  }

  public void _on_ServerButton_pressed()
  {
    _peer.CreateServer(SERVER_PORT, MAX_PLAYERS);
    GetTree().NetworkPeer = _peer;
    startGame();
  }

  //  // Called every frame. 'delta' is the elapsed time since the previous frame.
  //  public override void _Process(float delta)
  //  {
  //      
  //  }
}
