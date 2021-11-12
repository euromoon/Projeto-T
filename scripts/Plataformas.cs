using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Plataformas : TileMap
{
  // Declare member variables here. Examples:
  public List<int> BottomCells { get; set; }
  private Timer _timer;
  private List<int> _originalPositions = new List<int> { -6, 17, -5, 16, -4, 15, -3, 14, -2, 13, -1, 12, 0, 1, 2, 3, 4, 5 };

  private void onTimerTimeout()
  {
    // Deletar as tiles na ordem estabelecida anteriormente.
    SetCell(BottomCells[0], 8, -1);
    BottomCells.RemoveAt(0);
    if (BottomCells.Count == 0) // Parar o timer se não houver mais tiles para deletar.
      _timer.Stop();
    Rpc(nameof(SetBottomCellsAs), BottomCells); // Mudar também as tiles nos clientes.
  }

  [RemoteSync]
  public void Reset()
  {
    foreach (var tileX in _originalPositions)
    {
      SetCell(tileX, 8, 0);
    }
    BottomCells = new List<int>(_originalPositions);
  }

  // Essa função serve para sincronizar o chão do servidor com a dos clients.
  [Remote]
  public void SetBottomCellsAs(int[] cells)
  {
    foreach (var cell in _originalPositions)
    {
      // Se a célula estiver na lista enviada pelo servidor, adicionar
      // a tile do chão, se não, deixar vazio
      var fill = cells.Contains(cell) ? 0 : -1;
      SetCell(cell, 8, fill);
    }
  }

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    BottomCells = new List<int>(_originalPositions);
    _timer = GetNode<Timer>("DeleteTilesTimer");
    if (GetTree().IsNetworkServer())
      _timer.Connect("timeout", this, nameof(onTimerTimeout));
  }

  //  // Called every frame. 'delta' is the elapsed time since the previous frame.
  //  public override void _Process(float delta)
  //  {
  //      
  //  }
}
