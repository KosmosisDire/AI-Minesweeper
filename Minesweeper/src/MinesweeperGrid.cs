using ProtoEngine;
using ProtoEngine.UI;

namespace Minesweeper;

public class MinesweeperGrid : Grid<MinesweeperCell>
{
    public List<MinesweeperCell> unrevealedCells = new List<MinesweeperCell>();
    public List<MinesweeperCell> revealedCells = new List<MinesweeperCell>();

    public MinesweeperGrid(Element parent, int rows, int columns) : base(parent)
    {
        Regenerate(rows, columns, (x, y) => 
        {
            var cell = new MinesweeperCell(this, x, y);
            return cell;
        });

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
        Style.paddingX = "2em";
        Style.paddingY = "2em";

        unrevealedCells.AddRange(cells);
    }

    public void GenerateMap(float mineChance = 0.1f)
    {
        ForEachCell((cell, x, y) => 
        {
            cell.countText.Text = "";
            if (Application.random.NextSingle() < mineChance)
            {
                cell.isMine = true;
            }
        });
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

    public bool AllMinesFlagged()
    {
        return cells.TrueForAll(cell => cell.IsFlagged == cell.isMine);
    }

}