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
  private Label _usernameError;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    mainScene = GD.Load<PackedScene>("res://main.tscn").Instance<main>();
    _peer = new NetworkedMultiplayerENet();
    _usernameError = GetChild(1).GetChild(4).GetNode<Label>("usernameError");
  }

  private void startGame(string username)
  {
    mainScene.LocalUsername = username;
    GetTree().Root.AddChild(mainScene);
    QueueFree();
  }

  public void _on_ClientButton_pressed()
  {
    var username = GetChild(1).GetChild(4).GetNode<TextEdit>("username").Text;
    if (username.Empty())
    {
      _usernameError.Visible = true;
      return;
    }
    var ipAddress = GetNode("Options").GetNode<TextEdit>("IPInput").Text;
    if (ipAddress == "") return;
    _peer.CreateClient(ipAddress, SERVER_PORT);
    GetTree().NetworkPeer = _peer;
    startGame(username);
  }

  public void _on_ServerButton_pressed()
  {
    var username = GetChild(1).GetChild(4).GetNode<TextEdit>("username").Text;
    if (username.Empty())
    {
      _usernameError.Visible = true;
      return;
    }
    _peer.CreateServer(SERVER_PORT, MAX_PLAYERS);
    GetTree().NetworkPeer = _peer;
    startGame(username);
  }

  //  // Called every frame. 'delta' is the elapsed time since the previous frame.
  //  public override void _Process(float delta)
  //  {
  //      
  //  }
}
