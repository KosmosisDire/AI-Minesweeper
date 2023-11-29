using System.Net.Security;
using ProtoEngine;
using ProtoEngine.UI;
using SFML.Graphics;
using TerraFX.Interop.Windows;
using Image = ProtoEngine.Image;

namespace Minesweeper;

public class MinesweeperCell : Button, IComparable<MinesweeperCell>
{
    public bool isMine;
    public bool IsFlagged { get; protected set; }
    public bool IsRevealed { get; protected set; }
    public bool IsVisuallyRevealed { get; protected set; }
    public int x;
    public int y;
    public Vector2 Coord => new(x, y);

    public Image icon;
    public TextElement countText;
    public readonly MinesweeperGrid grid;
    public Element card;

    static NumericProperty size = "2em";

    public MinesweeperCell(MinesweeperGrid grid, int x, int y, bool isMine = false)
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

        icon = new Image(Properties.Resources.grass);
        icon.Style.visible = false;
        icon.Parent = card;

        this.isMine = isMine;
    }

    public void Flag()
    {
        if (IsRevealed) return;
        IsFlagged = true;
        if (isMine) SetIcon(Properties.Resources.flag);
        else SetIcon(Properties.Resources.wrongflag);
        grid.unrevealedUnflaggedCells.Remove(this);
    }

    public void SetIcon(byte[] imageData)
    {
        icon.sprite.Texture = new Texture(imageData);
        icon.Style.width = card.InnerWidth * 0.6f;
        icon.Style.height = icon.Style.width;
        icon.Style.alignSelfX = Alignment.Center;
        icon.Style.alignSelfY = Alignment.Center;
        icon.Style.ignorePointerEvents = true;
        icon.Style.visible = true;
    }

    public void AnimateReveal()
    {
        if (IsVisuallyRevealed) return;
        IsVisuallyRevealed = true;
        
        Style.marginRight.Tween(size / 2f, 0.1f, TweenType.EaseInOut, () =>
        {
            card.AddStyle(new Style
            {
                fillColor = new ColorMod((color) => color.Darken(0.6f))
            });

            Style.marginRight.Tween(0, 0.1f, TweenType.EaseInOut);

            var cost = GetCost();
            if (cost == 0)
            {
                foreach (var cell in GetNeighbors())
                {
                    if(!cell.IsFlagged) cell.AnimateReveal();
                }
            }
        });
    }

    public void InstantVisualReveal()
    {
        if (IsVisuallyRevealed) return;
        IsVisuallyRevealed = true;
        
        Style.marginRight = size / 2f;

        card.AddStyle(new Style
        {
            fillColor = new ColorMod((color) => color.Darken(0.6f))
        });

        Style.marginRight = 0;

        var cost = GetCost();
        if (cost == 0)
        {
            foreach (var cell in GetNeighbors())
            {
                if(!cell.IsFlagged) cell.InstantVisualReveal();
            }
        }
    }

    private void RevealRecursive()
    {
        if (IsRevealed) return;
        IsRevealed = true;

        grid.unrevealedCells.Remove(this);
        grid.unrevealedUnflaggedCells.Remove(this);
        grid.revealedCells.Add(this);

        if(IsFlagged && isMine) 
        {
            SetIcon(Properties.Resources.grass);
            card.AddStyle(new Style
            {
                fillColor = Color.Green.Darken(0.6f)
            });
        }
        else if (isMine) 
        {
            SetIcon(Properties.Resources.mine);
        }
        else
        {
            var cost = GetCost();
            if (cost > 0)
            {
                countText.Text = cost.ToString(); //Set number for cells
            }
            else
            {
                foreach (var cell in GetNeighbors())
                {
                    if(!cell.IsFlagged) cell.RevealRecursive();
                }
            }
        }
    }

    public void Reveal(bool isFirst = false)
    {
        AnimateReveal();
        // InstantVisualReveal();
        RevealRecursive();

        if (isMine && isFirst) 
        {
            SetIcon(Properties.Resources.explosion);
            card.AddStyle(new Style
            {
                fillColor = Color.Red.Darken(0.6f)
            });
        }
    }


    public int GetCost()
    {
        if (isMine) return -1;

        int cost = 0;
        var neighbors = GetNeighbors();
        foreach (var cell in neighbors)
        {
            if (cell.isMine) cost++;
        }
        return cost;
    }

    public float NormalizedAdjacentCost()
    {
        var revealedNeighborCount = GetRevealedNeighborCount();
        if (revealedNeighborCount == 0) return float.MaxValue;
        var total = 0;
        foreach(var cell in GetNeighbors()){
            if (cell.IsRevealed)
            {
                total += cell.GetCost();
            }
        } 
        return total / (float)revealedNeighborCount;
    }

    public float AdjacentCost()
    {
        var revealedNeighborCount = GetRevealedNeighborCount();
        if (revealedNeighborCount == 0) return float.MaxValue;
        var total = 0;
        foreach(var cell in GetNeighbors()){
            if (cell.IsRevealed)
            {
                total += cell.GetCost();
            }
        } 
        return total;
    }

    public int GetNeighborCount()
    {
        return GetNeighbors().Count();
    }

    public int GetRevealedNeighborCount()
    {
        var neighbors = GetNeighbors().Where(obj => obj.IsRevealed);
        return neighbors.Count();
    }

    public List<MinesweeperCell> GetRevealedNeighbors()
    {
        var neighbors = GetNeighbors().Where(obj => obj.IsRevealed);
        return neighbors.ToList();
    }

    public int GetUnrevealedNeighborCount()
    {
        var neighbors = GetNeighbors().Where(obj => !obj.IsRevealed);
        return neighbors.Count();
    }

    public List<MinesweeperCell> GetUnrevealedNeighbors()
    {
        var neighbors = GetNeighbors().Where(obj => !obj.IsRevealed);
        return neighbors.ToList();
    }

    public List<MinesweeperCell> GetUnrevealedUnflaggedNeighbors()
    {
        var neighbors = GetNeighbors().Where(obj => !obj.IsRevealed && !obj.IsFlagged);
        return neighbors.ToList();
    }

    public int GetFlaggedNeighborCount()
    {
        var neighbors = GetNeighbors().Where(obj => obj.IsFlagged);
        return neighbors.Count();
    }

    private MinesweeperCell[] cachedNeighbors;
    public MinesweeperCell[] GetNeighbors()
    {
        if (cachedNeighbors != null) return cachedNeighbors;
        cachedNeighbors = grid.GetNeighbors(x, y);
        return cachedNeighbors;
    }

    public int CompareTo(MinesweeperCell obj)
    {
        return  (int)MathF.Round(this.NormalizedAdjacentCost() - obj.NormalizedAdjacentCost());
    }

    public void ResetCell()
    {
        isMine = false;
        IsFlagged = false;
        IsRevealed = false;
        IsVisuallyRevealed = false;
        countText.Text = "";
        icon.Style.visible = false;
        card.ClearStyles();
        this.ClearStyles();
    }
}