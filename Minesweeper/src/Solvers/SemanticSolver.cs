namespace Minesweeper.Solvers;

public class SemanticSolver : Solver
{
    public SemanticSolver(MinesweeperGrid grid) : base(grid)
    {
    }
    
    public override (bool failed, MinesweeperCell move) GetNextMove(CancellationToken token = default)
    {
        MinesweeperCell move = grid.GetRandomCell(true);

        var revealed = new List<MinesweeperCell>();
        revealed.AddRange(grid.revealedCells);

        bool noMoves = true;
        Dictionary<MinesweeperCell, float> unaccountedBombsDict = new Dictionary<MinesweeperCell, float>();

        for (int i = 0; i < revealed.Count; i++)
        {
            var cell = revealed[i]; 
            int unaccountedBombs = cell.GetCost() - cell.GetFlaggedNeighborCount();
            var unrevealedAdjacent = cell.GetUnrevealedUnflaggedNeighbors();

            unaccountedBombsDict[cell] = unaccountedBombs;

            // cell.countText.Text = unaccountedBombs.ToString();

            if (unaccountedBombs == unrevealedAdjacent.Count)
            {
                foreach (var adj in unrevealedAdjacent)
                {
                    adj.Flag();
                    noMoves = false;
                }
            }

            if (unaccountedBombs == 0)
            {
                foreach (var adj in unrevealedAdjacent)
                {
                    move = adj;
                    noMoves = false;
                    break;
                }
            }
        }

        if (noMoves)
        {
            SortedList<float, MinesweeperCell> unaccountedBombSumList = new SortedList<float, MinesweeperCell>();
            var unrevealedUnflagged = new List<MinesweeperCell>();
            unrevealedUnflagged.AddRange(grid.unrevealedUnflaggedCells);

            for (int i = 0; i < unrevealedUnflagged.Count; i++)
            {
                var cell = unrevealedUnflagged[i];
                var revealedNeighbors = cell.GetRevealedNeighbors();
                float sumAdjUnaccountedBombs = revealedNeighbors.Sum(adj => unaccountedBombsDict[adj]);
                sumAdjUnaccountedBombs /= cell.GetUnrevealedUnflaggedNeighbors().Count;
                // cell.countText.Text = sumAdjUnaccountedBombs.ToString("N1");
                if (sumAdjUnaccountedBombs > 0) unaccountedBombSumList[sumAdjUnaccountedBombs] = cell;
            }

            if (unaccountedBombSumList.Count > 0)
            {
                move = unaccountedBombSumList.First().Value;
                // Console.WriteLine(unaccountedBombSumList.First().Key);
            }
        }

        return (move.isMine, move);
    }

}
