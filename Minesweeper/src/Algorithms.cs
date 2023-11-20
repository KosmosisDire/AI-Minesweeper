using ProtoEngine;
using SFML.Graphics;

namespace Minesweeper;

public delegate bool Solver(MinesweeperGrid grid);


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

        if (min == null) return false;

        min.Reveal();
        if (min.isMine) 
        {
            return false; //Stop checking if mine is found
        }

        grid.unrevealedCells.ForEach((cell) => 
        {
            var adjCost = cell.AdjacentCost();
            if (cell.GetRevealedNeighborCount() > 1)
                cell.countText.Text = cell.AdjacentCost().ToString("N1");
        });

        Thread.Sleep(1000);

        return true;
    };


    
    public static Solver BrushfireSolver = (MinesweeperGrid grid) =>
    {
        // Thread.Sleep(100);

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

            cell.countText.Text = unaccountedBombs.ToString();

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
                    adj.Reveal();
                    noMoves = false;
                }
            }
        }


        if (noMoves)
        {
            // SortedList<float, MinesweeperCell> unaccountedBombSumList = new SortedList<float, MinesweeperCell>();
            // var unrevealed = new List<MinesweeperCell>();
            // unrevealed.AddRange(grid.unrevealedCells);
            // unrevealed.Where(cell => !cell.IsFlagged);

            // for (int i = 0; i < unrevealed.Count; i++)
            // {
            //     var cell = unrevealed[i];
            //     var revealedNeighbors = cell.GetRevealedNeighbors();
            //     float sumAdjUnaccountedBombs = revealedNeighbors.Sum(adj => unaccountedBombsDict[adj]);
            //     sumAdjUnaccountedBombs /= cell.GetUnrevealedUnflaggedNeighbors().Count;
            //     cell.countText.Text = sumAdjUnaccountedBombs.ToString("N1");
            //     if (sumAdjUnaccountedBombs > 0) unaccountedBombSumList[sumAdjUnaccountedBombs] = cell;
            // }

            // if (unaccountedBombSumList.Count > 0)
            // {
            //     var min = unaccountedBombSumList.First().Value;
            //     Console.WriteLine(unaccountedBombSumList.First().Key);
            //     min.Reveal(true);

            //     if (min.isMine) 
            //     {
            //         return false; //Stop checking if mine is found
            //     }
            // }
            // else
            // {
            var randomCell = grid.unrevealedCells[Application.random.Next(0,grid.unrevealedCells.Count())];

            if (randomCell.isMine) 
            {
                randomCell.Reveal(true);
                return false; //Stop checking if mine is found
            }

            // Console.WriteLine("Random");

            randomCell.Reveal();
            // }

            // for (int i = 0; i < unrevealed.Count; i++)
            // {
            //     var cell = unrevealed[i];
            //     var revealedNeighbors = cell.GetRevealedNeighbors();
            //     float sumAdjUnaccountedBombs = revealedNeighbors.Sum(adj => unaccountedBombsDict[adj]);
            //     cell.countText.Text = sumAdjUnaccountedBombs.ToString("N1");
            //     unaccountedBombSumList[sumAdjUnaccountedBombs] = cell;
            // }
        }


        return true;
    };

    public static Solver SemanticSolver = (MinesweeperGrid grid) =>
    {
        var revealed = new List<MinesweeperCell>();
        revealed.AddRange(grid.revealedCells);

        bool noMoves = true;

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
                        noMoves = false;
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
                        noMoves = false;
                    }
                }
            }
        }

        if (noMoves && grid.unrevealedCells.Count > 0)
        {
            // reveal a random cell if no certain moves are found
            var randomCell = grid.unrevealedCells[Application.random.Next(0,grid.unrevealedCells.Count())];

            if (randomCell.isMine) 
            {
                randomCell.Reveal(true);
                return false; //Stop checking if mine is found
            }

            randomCell.Reveal();
        }

        Thread.Sleep(100);

        return true;
    };

}