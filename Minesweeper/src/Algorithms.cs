namespace Minesweeper;

public delegate MinesweeperCell? Solver(MinesweeperGrid grid);

public static class Algorithms
{
    public static Solver StupidButIntuitiveSolver = (MinesweeperGrid grid) =>
    {
        var unrevealed = new List<MinesweeperCell>();
        unrevealed.AddRange(grid.unrevealedCells);

        var nonCorners = unrevealed.Where(cell => cell.GetRevealedNeighborCount() > 0).ToList();
        if (nonCorners.Count == 0) nonCorners = unrevealed;

        var min = nonCorners.MinBy(obj => obj.AdjacentCost());

        Console.WriteLine($"Playing {min.x}, {min.y}: {min.AdjacentCost()}");
        var costs = nonCorners.Select(obj => obj.AdjacentCost()).ToList();
        Console.WriteLine($"Min: {costs.Min()} Max: {costs.Max()}");

        if (min == null) return null;

        min.Reveal();
        if (min.isMine) 
        {
            return null; //Stop checking if mine is found
        }

        grid.unrevealedCells.ForEach((cell) => 
        {
            var adjCost = cell.AdjacentCost();
            if (cell.GetRevealedNeighborCount() > 1)
                cell.countText.Text = cell.AdjacentCost().ToString("N1");
        });

        Thread.Sleep(1000);

        return min;
    };

    public static Solver SemanticSolver = (MinesweeperGrid grid) =>
    {
        var revealed = new List<MinesweeperCell>();
        revealed.AddRange(grid.revealedCells);

        for (int i = 0; i < revealed.Count; i++)
        {
            var cell = revealed[i];
            var cost = cell.GetCost();
            if (cost == 0) continue;

            var neighbors = cell.GetNeighbors();

            if (cell.GetFlaggedNeighborCount() == cost)
            {
                foreach (var n in neighbors) 
                {
                    if (!n.IsRevealed && !n.IsFlagged)
                    {
                        n.Reveal();
                    }
                }
            }

            if (cell.GetUnrevealedNeighborCount() == cost)
            {
                foreach (var n in neighbors) 
                {
                    if (!n.IsRevealed && !n.IsFlagged)
                    {
                        n.Flag();
                    }
                }
            }
        }

        // Thread.Sleep(1000);

        return null;
    };

}