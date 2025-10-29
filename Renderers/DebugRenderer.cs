using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace MonoFighter.Debugging;

public class DebugRenderer
{
    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;

    private int _halfScreenWidth { get { return Globals.WindowSize.X / 2; } }

    public DebugRenderer()
    {
        _font = Globals.Content.Load<SpriteFont>("Fonts/DebugFont");

        // Reusable 1x1 white texture
        _pixel = new Texture2D(Globals.GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void DrawBoxes(Rectangle gameViewRect, RenderTarget2D renderTarget)
    {
        // Example: hit and hurt boxes from game coordinates
        Rectangle hitBox = new(0, 0, 5, 5);
        Rectangle hurtBox = new(100, 80, 30, 40);

        float scaleX = (float)gameViewRect.Width / renderTarget.Width;
        float scaleY = (float)gameViewRect.Height / renderTarget.Height;

        DrawRect(TransformRect(hitBox, scaleX, scaleY, gameViewRect.Location), Color.Red);
        DrawRect(TransformRect(hurtBox, scaleX, scaleY, gameViewRect.Location), Color.Lime);
    }

    public void DrawPanel()
    {

        // Draw semi-transparent background for the debug area
        Rectangle panelRect = new(_halfScreenWidth, 0, _halfScreenWidth, Globals.WindowSize.Y);
        Globals.SpriteBatch.Draw(_pixel, panelRect, Color.White * 0.5f);


        Vector2 pos = new(panelRect.X + 20, 20);
        float fps = (float)(1 / Globals.DeltaTime);

        Globals.SpriteBatch.DrawString(_font, $"FPS: {fps:0}", pos, Color.Yellow);
        pos.Y += 30;
        Globals.SpriteBatch.DrawString(_font, "=== Debug Info ===", pos, Color.Cyan);
        pos.Y += 25;
        Globals.SpriteBatch.DrawString(_font, "Player State: Idle", pos, Color.Lime);
    }

    private static Rectangle TransformRect(Rectangle rect, float scaleX, float scaleY, Point offset)
    {
        return new Rectangle(
            offset.X + (int)(rect.X * scaleX),
            offset.Y + (int)(rect.Y * scaleY),
            (int)(rect.Width * scaleX),
            (int)(rect.Height * scaleY));
    }

    private void DrawRect(Rectangle rect, Color color)
    {
        Globals.SpriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Top, rect.Width, 1), color);
        Globals.SpriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Bottom, rect.Width, 1), color);
        Globals.SpriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Top, 1, rect.Height), color);
        Globals.SpriteBatch.Draw(_pixel, new Rectangle(rect.Right, rect.Top, 1, rect.Height), color);
    }
}
