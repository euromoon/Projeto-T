using Godot;
using System;
using System.Collections.Generic;

public class Announcer : Label
{
  // Declare member variables here. Examples:
  private List<string> _announcements = new List<string>();
  private Timer _timer;

  private void FinishedAnnouncement()
  {
    // Se tiver algum anúncio na fila, reproduza-o.
    if (_announcements.Count > 0)
    {
      _timer.Start();
      Visible = true;
      Text = _announcements[0]; // Pegar o primeiro anúncio da fila.
      _announcements.RemoveAt(0); // Remover da fila.
      return;
    }
    // Se não, apague o anunciador.
    Visible = false;
  }

  [RemoteSync]
  public void Announce(string announcement)
  {
    if (_timer.IsStopped())
    {
      // Mostrar o anúncio.
      Visible = true;
      Text = announcement;
      _timer.Start();
      return;
    }
    _announcements.Add(announcement);
  }

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    _timer = GetNode<Timer>("Timer");
    _timer.Connect("timeout", this, nameof(FinishedAnnouncement));
  }

  //  // Called every frame. 'delta' is the elapsed time since the previous frame.
  //  public override void _Process(float delta)
  //  {
  //      
  //  }
}
