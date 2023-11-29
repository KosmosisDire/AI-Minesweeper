using ProtoEngine;
using ProtoEngine.UI;
using SFML.Graphics;
using Minesweeper;
using SFML.Window;
using Window = ProtoEngine.Rendering.Window;
using Minesweeper.Solvers;
using System.Diagnostics;

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

    protected override void Setup()
    {
        base.Setup();

        window.Style.fontSize = window.Width / 1920f * 16f;

        CreateBoard(30, 16);
        grid.defaultMineCount = 99;

        var infoPanel = new Panel(window);
        infoPanel.Style.width = "20em";

        var solveTimer = new Stopwatch();

        var geneticSolver = new GeneticSolver(grid);
        var semanticSolver = new SemanticSolver(grid);

        var GAPlot = new Plot(infoPanel, new Series[]
        {
            new("Average", Color.Blue, new(() => geneticSolver.fitnessAverage)),
            new("Max", Color.Red, new(() => geneticSolver.fitnessMax)),
            new("Min",Color.Green, new(() => geneticSolver.fitnessMin)),
        });

        var bestPlot = new Plot(infoPanel, new Series[]
        {
            new("Min",Color.Green, new(() => geneticSolver.fitnessMin)),
        });

        new TextElement(infoPanel, () => "Min Fitness: " + geneticSolver.fitnessMin);
        new TextElement(infoPanel, () => "Solve Time: " + solveTimer.ElapsedMilliseconds + "ms");

        var tokenSource = new CancellationTokenSource();
        var token = tokenSource.Token;

        var cancelButtonStyle = new Style() 
        {
            fillColor = Color.Red.Darken(0.8f),
        };

        var semanticSolve = new Button(infoPanel, "Semantic Solve", async (button) => 
        {
            if (!grid.solving)
            {
                button.AddStyle(cancelButtonStyle);
                button.label.Text = "Cancel";
                semanticSolver = new SemanticSolver(grid);
                solveTimer.Restart();
                await grid.RunSolver(semanticSolver, updateLoop, true, token);
            }

            button.label.Text = "Semantic Solver";
            button.RemoveStyle(cancelButtonStyle);
            tokenSource.Cancel();
            solveTimer.Stop();
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
        });

        var geneticSolve = new Button(infoPanel, "Genetic Solve", async (button) => 
        {
            
            if (!grid.solving)
            {
                button.AddStyle(cancelButtonStyle);
                button.label.Text = "Cancel";
                geneticSolver = new GeneticSolver(grid);
                grid.GenerateMap(grid.defaultMineCount);
                GAPlot.Start();
                bestPlot.Start();
                solveTimer.Restart();
                await grid.RunSolver(geneticSolver, updateLoop, false, token);
            }

            button.label.Text = "Genetic Solver";
            button.RemoveStyle(cancelButtonStyle);
            tokenSource.Cancel();
            GAPlot.Stop();
            bestPlot.Stop();
            solveTimer.Stop();
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
        });

        var boardTypePanel = new Panel(window);
        boardTypePanel.Style.width = "20em";
        boardTypePanel.container.Style.left = infoPanel.Right + 10;

        new TextElement(boardTypePanel, "Difficulty Setting");

        var Beginner = new Button(boardTypePanel, "Beginner", (button) => //Beginner difficulty
        {
            button.label.Text = "Beginner";
            CreateBoard(10, 10);
            grid.defaultMineCount = 10;
        });

        var Intermediate = new Button(boardTypePanel, "Intermediate", (button) => //Beginner difficulty
        {
            button.label.Text = "Intermediate";
            CreateBoard(16, 16);
            grid.defaultMineCount = 40;
        });

        var Expert = new Button(boardTypePanel, "Expert", (button) => //Beginner difficulty
        {
            button.label.Text = "Expert";
            CreateBoard(30, 16);
            grid.defaultMineCount = 99;
        });
    }

    public void CreateBoard(int width, int height)
    {
        if (grid is not null) grid.Remove();

        grid = new MinesweeperGrid(window, height, width);

        grid.ForEachCell((cell,x,y) => 
        {
            cell.OnClick += (obj) => cell.Reveal();
            if (cell.isMine) cell.SetIcon(Properties.Resources.mine);
        });

        window.RebuildAllChildren(true); 
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