namespace Minesweeper.Solvers;

public abstract class Solver
{
    public MinesweeperGrid grid;
    public abstract (bool failed, MinesweeperCell move) GetNextMove(CancellationToken token = default);

    public Solver(MinesweeperGrid grid)
    {
        this.grid = grid;
    }
}
