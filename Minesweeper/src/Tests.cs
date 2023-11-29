using Minesweeper.Solvers;
using ProtoEngine;
using ProtoEngine.UI;

namespace Minesweeper;

public class MinesweeperTests
{
    public MinesweeperGrid grid;
    public Plot? winLossPlot;

    public MinesweeperTests(MinesweeperGrid grid)
    {
        this.grid = grid;
    }
    
    public async Task<(float successRate, int wins, int losses)> RunSolveBatch(int runs, int numMines, Solver solver, Loop inLoop, CancellationToken token)
    {
        int localWins = 0;
        int localLosses = 0;
        for (int i = 0; i < runs; i++)
        {
            grid.ResetGrid();
            grid.GenerateMap(numMines);
            var success = await grid.RunSolver(solver, inLoop, true, token);
            if (success) localWins++;
            else localLosses++;
        }

        var successRate = (float)localWins / (localWins + localLosses);
        var wins = localWins;
        var losses = localLosses;

        return (successRate, wins, losses);
    }

    // public async void RunDensityTest(Solver solver, Loop inLoop, CancellationTokenSource tokenSource)
    // {
    //     winLossPlot?.Reset();
    //     winLossPlot?.Start();

    //     for (int i = 9; i <= 99; i += 10)
    //     {
    //         await RunSolveBatch(100, i, solver, inLoop, tokenSource);
    //     }

    //     winLossPlot?.Stop();
    // }

}