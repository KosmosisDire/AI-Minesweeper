using ProtoEngine;
using ProtoEngine.UI;
using SFML.Graphics;
using Image = ProtoEngine.Image;

namespace Minesweeper;

public class MinesweeperCell : Button, IComparable<MinesweeperCell>
{
    public bool IsMine { get; protected set;}
    public bool IsRevealed { get; protected set; }
    public int x;
    public int y;
    public Vector2 Coord => new(x, y);

    public Image? icon;
    public TextElement countText;
    public readonly Grid<MinesweeperCell> grid;
    private Element card;

    static NumericProperty size = "40px";

    public MinesweeperCell(Grid<MinesweeperCell> grid, int x, int y, bool isMine = false)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;

        card = new Element(this);
        card.Style = DefaultStyle;
        card.Style.height = size;
        card.HoverStyle = HoverStyle;
        card.PressedStyle = PressedStyle;
        
        Style.fillColor = Color.Transparent;
        Style.outlineColor = Color.Transparent;
        Style.width = size;
        Style.height = size;
        Style.marginRight = "0px";
        Style.marginLeft = Style.marginRight;

        label.Remove();

        countText = new TextElement(card, "");
        countText.Style.fontSize = "20px";
        countText.Style.alignSelfX = Alignment.Center;
        countText.Style.alignSelfY = Alignment.Center;

        if (isMine) MakeMine();
    }

    public void MakeMine()
    {
        IsMine = true;
        icon = new Image(Properties.Resources.mine);
        icon.Style.width = "2em";
        icon.Style.height = icon.Style.width;
        icon.Style.alignSelfX = Alignment.Center;
        icon.Style.alignSelfY = Alignment.Center;
        icon.Style.visible = false;
        icon.Style.ignorePointerEvents = true;
        
        AddChild(icon);
    }

    public void Reveal()
    {
        if (IsRevealed) return;
        IsRevealed = true;

        Style.marginRight.Tween(size / 2f, 0.1f, TweenType.EaseInOut, () =>
        {
            Style.marginRight.Tween(0, 0.1f, TweenType.EaseInOut);

            card.AddStyle(new Style
            {
                fillColor = new ColorMod((color) => color.Darken(0.8f))
            });

            if(IsMine && icon is not null) icon.Style.visible = !icon.Style.visible;
            else
            {
                var count = GetCost(); 
                if (count > 0) countText.Text = count.ToString(); //Set number for cells
                else
                {
                    foreach (var cell in GetNeighbors())
                    {
                        if(cell != null) cell.Reveal();
                    }
                }
            }
        });
    }


    public int GetCost()
    {
        int cost = 0;
        var neighbors = GetNeighbors();
        foreach (var cell in neighbors)
        {
            if (cell != null && cell.IsMine) cost++;
        }
        return cost;
    }
    public int AdjacentCost()
    {
        var total = 0;
        foreach(var cell in GetNeighbors()){
            if (cell !=null && cell.IsRevealed)
            {
                total += GetCost();
            }
        } 
        return total;
    }

    public MinesweeperCell[] GetNeighbors()
    {
        return grid.GetNeighbors(x, y);
    }

    public int CompareTo(MinesweeperCell obj)
    {
        return  this.AdjacentCost() - obj.AdjacentCost();
    }
}