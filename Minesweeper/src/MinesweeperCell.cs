using ProtoEngine;
using ProtoEngine.UI;
using SFML.Graphics;
using Image = ProtoEngine.Image;

namespace Minesweeper;

public class MinesweeperCell : Button
{
    public bool IsMine { get; protected set;}
    public int x;
    public int y;
    public Vector2 Coord => new(x, y);

    public Image? icon;
    public readonly Grid<MinesweeperCell> grid;
    private Element card;

    static NumericProperty size = "40px";

    public MinesweeperCell(Grid<MinesweeperCell> grid, bool isMine = false)
    {
        this.grid = grid;

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

        if (isMine) MakeMine();
    }

    public void MakeMine()
    {
        IsMine = true;
        label.Remove();
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
        Style.marginRight.Tween(size / 2f, 0.2f, TweenType.EaseInOut, () =>
        {
            if(icon is not null) icon.Style.visible = !icon.Style.visible;
            Style.marginRight.Tween(0, 0.2f, TweenType.EaseInOut);
        });
    }

}