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
    public List<MinesweeperCell> AdjCells = new List<MinesweeperCell>();
    public MinesweeperGrid grid;

    public bool foundMine = false;
    protected override void Setup()
    {
        base.Setup();
        //grid = new MinesweeperGrid(window, 16, 30);
        //grid.GenerateMap(0.2f);
        window.globalEvents.KeyPressed+= (KeyEventArgs e, Window window) => {
            if (e.Code==Keyboard.Key.R) {
                lock(this)
                {
                    if (grid!=null) grid.Remove();
                    grid = new MinesweeperGrid(window, 16, 30);
                    grid.GenerateMap(0.2f);
                    AdjCells.Clear();
                    AdjCells.AddRange(grid.cells); //Get adjacent cells
                    grid.cells[Application.random.Next(0,grid.cells.Count())].Reveal();
                    AdjCells.Remove(AdjCells.First());
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
            AdjCells = grid.GetUnrevealed();
            AdjCells.ForEach((cell) => cell.countText.Text = cell.AdjacentCost().ToString());
            AdjCells.Sort();
            var min = AdjCells.Find(obj => obj.AdjacentCost() != 0); //Find neighbor with lowest cost
            if (min == null) return;
            if (min.AdjacentCost() >= min.GetNeighborCount()) { //If greater than neighborcount => choose random cell
                min = AdjCells.First(); 
            }
            min.Reveal();
            Console.WriteLine("Sorted list: " + min.AdjacentCost());
            if (min.IsMine) {
                //grid.ForEachCell((cell,x,y) => cell.Reveal());
                foundMine = true;
                //grid.RevealMines();
                return; //Stop checking if mine is found
            }
            //AdjCells.Remove(min); //Remove adjacent cell after revealing
            //Thread.Sleep(1000); //Sloww down
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