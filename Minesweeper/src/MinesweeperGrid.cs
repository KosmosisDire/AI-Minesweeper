using ProtoEngine;
using ProtoEngine.UI;

namespace Minesweeper;

public class MinesweeperGrid : Grid<MinesweeperCell>
{
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
    }

    public void GenerateMap(float mineChance = 0.1f)
    {
        ForEachCell((cell, x, y) => 
        {
            cell.label.Text = "";
            if (Application.random.NextSingle() < mineChance)
            {
                cell.MakeMine();
            }
        });
    }

}