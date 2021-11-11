using Godot;
using System;
using System.Collections.Generic;

public class Plataformas : TileMap
{
  // Declare member variables here. Examples:
  private Timer _timer;
  private List<int> _tileDeleteQueue = new List<int> { -6, 17, -5, 16, -4, 15, -3, 14, -2, 13, -1, 12, 0, 1, 2, 3, 4, 5 };

  private void onTimerTimeout()
  {
    if (_tileDeleteQueue.Count == 18)
    {
      _timer.WaitTime = 15; 
    }
    // Deletar as tiles na ordem estabelecida anteriormente.
    SetCell(_tileDeleteQueue[0], 8, -1);
    _tileDeleteQueue.RemoveAt(0);
  }

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    _timer = GetNode<Timer>("DeleteTilesTimer");
    _timer.Connect("timeout", this, nameof(onTimerTimeout));
  }

  //  // Called every frame. 'delta' is the elapsed time since the previous frame.
  //  public override void _Process(float delta)
  //  {
  //      
  //  }
}
