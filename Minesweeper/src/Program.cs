using ProtoEngine;
using ProtoEngine.UI;
using SFML.Graphics;
using Minesweeper;
using Image = ProtoEngine.Image;
using TerraFX.Interop.Windows;
using SFML.Window;
using Window = ProtoEngine.Rendering.Window;

var app = new MinesweeperApp("Minesweeper", Theme.GlobalTheme.background, false);
app.Run();

namespace Minesweeper
{

public class MinesweeperApp : Application
{
    public MinesweeperApp(string name, Color windowFill, bool fullscreen, Vector2? size = null) : base(name, windowFill, fullscreen, size){}
    public MinesweeperGrid grid;

    public bool foundMine = false;
    protected override void Setup()
    {
        base.Setup();
        //grid = new MinesweeperGrid(window, 16, 30);
        //grid.GenerateMap(0.2f);
        window.globalEvents.KeyPressed+= (KeyEventArgs e, Window window) => {
            if (e.Code==Keyboard.Key.R) 
            {
                lock(this)
                {
                    if (grid!=null) grid.Remove();
                    grid = new MinesweeperGrid(window, 10, 10);
                    grid.GenerateMap(0.15f);
                    grid.cells[random.Next(0,grid.cells.Count())].Reveal();
                    grid.ForEachCell((cell,x,y) => cell.OnClick+= obj => //Reveal mines when clicked
                        grid.RevealMines()
                    );
                    foundMine = false;
                    window.RebuildAllChildren();
                }
            }
        };
    }

    protected override void Update(float dt)
    {
        if (grid==null || foundMine) return;
        lock(this)
        {
            base.Update(dt);
            var toReveal = Algorithms.StupidButIntuitiveSolver(grid);

            if (toReveal == null)
            {
                foundMine = true;
                return;
            }
        }
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