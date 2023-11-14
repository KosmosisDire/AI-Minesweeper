using ProtoEngine;
using ProtoEngine.UI;
using SFML.Graphics;
using Minesweeper;
using Image = ProtoEngine.Image;

var app = new MinesweeperApp("Minesweeper", Theme.GlobalTheme.background, true);
app.Run();

namespace Minesweeper
{

public class MinesweeperApp : Application
{
    public MinesweeperApp(string name, Color windowFill, bool fullscreen, Vector2? size = null) : base(name, windowFill, fullscreen, size){}

    protected override void Setup()
    {
        base.Setup();

        var grid = new MinesweeperGrid(window, 20, 20);
        grid.Style.alignSelfX = Alignment.Center;
        grid.Style.alignSelfY = Alignment.Center;
        grid.Style.fillColor = Theme.GlobalTheme.surface1;
        grid.Style.outlineColor = Theme.GlobalTheme.surface1Outline;
        grid.Style.outlineWidth = "2px";
        grid.Style.radius = "2em";
        grid.Style.paddingX = "2em";
        grid.Style.paddingY = "2em";
        grid.GenerateMap(0.1f);
        grid.ForEachCell((cell, x, y) => 
        {
            cell.OnClick += (button) => 
            {
                cell.Reveal();
            };
        });

        window.RebuildAllChildren();
    }

    protected override void Update(float dt)
    {
        base.Update(dt);
    }

    protected override void FixedUpdate(float dt)
    {
        base.FixedUpdate(dt);
    }

    protected override void Draw(float dt)
    {
        base.Draw(dt);
    }
}

}