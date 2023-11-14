﻿using ProtoEngine;
using ProtoEngine.UI;
using SFML.Graphics;
using Minesweeper;
using Image = ProtoEngine.Image;
using TerraFX.Interop.Windows;

var app = new MinesweeperApp("Minesweeper", Theme.GlobalTheme.background, false);
app.Run();

namespace Minesweeper
{

public class MinesweeperApp : Application
{
    public MinesweeperApp(string name, Color windowFill, bool fullscreen, Vector2? size = null) : base(name, windowFill, fullscreen, size){}
    public List<MinesweeperCell> AdjCells = new List<MinesweeperCell>();
    public MinesweeperGrid grid;
    protected override void Setup()
    {
        base.Setup();
        grid = new MinesweeperGrid(window, 16, 30);
        grid.GenerateMap(0.1f);
        window.RebuildAllChildren();
        AdjCells.AddRange(grid.cells);
        AdjCells.First().Reveal();
        AdjCells.Remove(AdjCells.First());
    }

    protected override void Update(float dt)
    {
        base.Update(dt);
        AdjCells.Sort();
        var min = AdjCells.Find(obj => obj.AdjacentCost() != 0); //Find neighbor with lowest cost
        if (min.AdjacentCost() >= min.GetNeighborCount()) {
            min = AdjCells.First();
        }
        Console.WriteLine("Minimum Cell: Is it a mine? " + min.IsMine);
        if (min == null) return;
        min.Reveal();
        Console.WriteLine("Sorted list: " + min.AdjacentCost());
        if (min.IsMine) {
            grid.ForEachCell((cell,x,y) => cell.Reveal());
            return; //Stop checking if mine is found
        }
        AdjCells.Remove(min); //Remove adjacent cell after revealing
        Thread.Sleep(1000); //Sloww down
    }

    protected override void FixedUpdate(float dt)
    {
        base.FixedUpdate(dt);
    }

    protected override void Draw(float dt)
    {
        base.Draw(dt);
    }
}

}