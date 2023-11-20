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
    public float successRate = 1;
    public int wins = 0;
    public int losses = 0;
    public int runs = 0;
    
    public Plot winLossPlot;
    public Plot successPlot;

    public bool foundMine = false;
    protected override void Setup()
    {
        base.Setup();

        window.Style.fontSize = window.Width / 1920f * 16f;

        CreateBoard(30, 16, 0.15f);

        var infoPanel = new Panel(window);
        winLossPlot = new Plot(infoPanel, new Series[]
        {
            new(Color.Green, new(() => wins)),
            new(Color.Red, new(() => losses))
        });

        successPlot = new Plot(infoPanel, new Series[]
        {
            new(Color.Blue, new(() => successRate))
        });

        new TextElement(infoPanel, () => "Success Rate: " + successRate);
        new TextElement(infoPanel, () => "Wins: " + wins);
        new TextElement(infoPanel, () => "Losses: " + losses);

        window.globalEvents.KeyPressed+= (KeyEventArgs e, Window window) => {
            if (e.Code==Keyboard.Key.R) 
            {
                RunDensityTest();
            }
        };
    }

    public void RunSolveBatch(int runs, float mineDensity)
    {
        int localWins = 0;
        int localLosses = 0;
        for (int i = 0; i < runs; i++)
        {
            grid.ResetGrid();
            grid.GenerateMap(mineDensity);
            var success = RunSolve();
            if (success) localWins++;
            else localLosses++;
        }

        successRate = (float)localWins / (localWins + localLosses);
        wins = localWins;
        losses = localLosses;

        Console.WriteLine($"Success Rate: {successRate} Wins: {wins} Losses: {losses}");
    }

    public void RunDensityTest()
    {
        winLossPlot.Reset();
        successPlot.Reset();
        winLossPlot.Start();
        successPlot.Start();

        updateLoop.RunAction(() => {

            for (float i = 0; i < 0.25; i += 0.02f)
            {
                RunSolveBatch(100, i);
            }

            winLossPlot.Stop();
            successPlot.Stop();
        });

        
    }

    public void CreateBoard(int width, int height, float bombChance)
    {
        lock(this)
        {
            if (grid is not null) grid.Remove();

            grid = new MinesweeperGrid(window, height, width);

            grid.GenerateMap(bombChance);

            grid.ForEachCell((cell,x,y) => 
            {
                cell.OnClick += (obj) => cell.Reveal();
                if (cell.isMine) cell.SetIcon(Properties.Resources.mine);
            });

            foundMine = false;
            window.RebuildAllChildren(true); 
        }
    }

    public bool RunSolve()
    {
        while (true)
        {
            lock(this)
            {
                var success = Algorithms.BrushfireSolver(grid);

                if (grid.AllMinesFlagged())
                {
                    grid.RevealMines();
                    return true;
                }

                if (!success)
                {
                    grid.RevealMines();
                    return false;
                }
            }
        }
    }

    protected override void Update(float dt)
    {
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