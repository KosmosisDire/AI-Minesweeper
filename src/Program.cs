using ProtoEngine;
using ProtoEngine.UI;
using SFML.Graphics;
using Minesweeper;

var app = new MinesweeperApp("Minesweeper", Theme.GlobalTheme.background, false);
app.Run();

namespace Minesweeper
{

public class MinesweeperApp : Application
{
    public MinesweeperApp(string name, Color windowFill, bool fullscreen) : base(name, windowFill, fullscreen){}

    protected override void Setup()
    {
        base.Setup();
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