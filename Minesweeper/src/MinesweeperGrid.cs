using Minesweeper.Solvers;
using ProtoEngine;
using ProtoEngine.UI;

namespace Minesweeper;

public class MinesweeperGrid : Grid<MinesweeperCell>
{
    public List<MinesweeperCell> unrevealedCells = new List<MinesweeperCell>();
    public List<MinesweeperCell> revealedCells = new List<MinesweeperCell>();
    public List<MinesweeperCell> unrevealedUnflaggedCells = new List<MinesweeperCell>();

    public bool AllMinesAreFlagged => cells.TrueForAll(cell => cell.IsFlagged == cell.isMine);
    public int MineCount => cells.Count(cell => cell.isMine);
    public int defaultMineCount = 16;

    public MinesweeperGrid(Element parent, int rows, int columns) : base(parent)
    {
        Resize(rows, columns);

        ForEachRow(row => {row.Style.contentFitX = Fit.Fit; row.Style.contentFitY = Fit.Fit;});

        Style.contentFitX = Fit.Fit;
        Style.contentFitY = Fit.Fit;

        Style.gap = "0.2em";
        Style.alignSelfX = Alignment.Center;
        Style.alignSelfY = Alignment.Center;
        Style.fillColor = Theme.GlobalTheme.surface1;
        Style.outlineColor = Theme.GlobalTheme.surface1Outline;
        Style.outlineWidth = "2px";
        Style.radius = "2em";
        Style.paddingX = "1.5em";
        Style.paddingY = "1.5em";

        ResetGrid();
    }

    public void Resize(int rows, int columns)
    {
        Resize(rows, columns, (x, y) => 
        { 
            var cell = new MinesweeperCell(this, x, y);
            return cell;
        });
    }

    public void GenerateMap(int mineCount, MinesweeperCell? exclude = null)
    {
        defaultMineCount = mineCount;
        ResetGrid();
        List<MinesweeperCell> bombs = new List<MinesweeperCell>(cells);
        bombs.AddRange(cells);
        if (exclude != null) bombs.Remove(exclude);
        bombs.Shuffle();
        bombs.RemoveRange(defaultMineCount, bombs.Count - defaultMineCount);
        
        bombs.ForEach(cell => cell.isMine = true);
    }

    public void RevealAll()
    {
        ForEachCell((cell, x, y) => 
        {
            cell.Reveal();
        });
    }

    public void RevealMines()
    {
        ForEachCell((cell, x, y) => 
        {
            if (cell.isMine) cell.Reveal();
        });
    }


    public void ResetGrid()
    {
        ForEachCell((cell, x, y) => 
        {
            cell.ResetCell();
        });

        unrevealedCells.Clear();
        revealedCells.Clear();
        unrevealedUnflaggedCells.Clear();
        unrevealedCells.AddRange(cells);
        unrevealedUnflaggedCells.AddRange(cells);
    }

    public MinesweeperCell GetRandomCell(bool onlyUnrevealedAndUnflagged)
    {
        if (onlyUnrevealedAndUnflagged)
        {
            if (unrevealedUnflaggedCells.Count == 0) throw new Exception("No unrevealed cells left");
            return unrevealedUnflaggedCells[Application.random.Next(0, unrevealedUnflaggedCells.Count)];
        }
        else
        {
            return cells[Application.random.Next(0, cells.Count)];
        }
    }

    public bool solving = false;

    public async Task<bool> RunSolver(Solver solver, Loop loop, bool generateMap = true, CancellationToken token = default)
    {
        solving = true;
        solver.grid = this;
        bool failed = false;
        bool complete = false;
        bool firstIteration = true;

        if(generateMap) ResetGrid();

        void SolveIteration(float dt)
        {
            if(complete) return;

            (failed, MinesweeperCell move) = solver.GetNextMove(token);
            complete = failed;

            if (firstIteration && generateMap)
            {
                GenerateMap(defaultMineCount, move);
            }

            if (token.IsCancellationRequested) 
            {
                return;
            }

            move.Reveal(true); 

            // if (AllMinesAreFlagged)
            // {
            //     RevealMines();
            //     success = true;
            //     tokenSource.Cancel();
            // }

            if (unrevealedCells.TrueForAll(cell => cell.isMine) && revealedCells.TrueForAll(cell => !cell.isMine))
            {
                Console.WriteLine("All mines found");
                complete = true;
                failed = false;
            }

            firstIteration = false;
        }

        loop.Connect(SolveIteration);

        await Utils.WaitUntil(() => complete, 50, -1, token);
        RevealMines();
        loop.Disconnect(SolveIteration);
        solving = false;
        return !failed;
    }

}