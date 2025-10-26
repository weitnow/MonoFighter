using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoFighter.Debugging;

public class DebugRenderer
{
    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;

    public DebugRenderer(GraphicsDevice graphics, SpriteFont font)
    {
        _font = font;

        _pixel = new Texture2D(graphics, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void DrawBoxes(SpriteBatch spriteBatch, Rectangle gameViewRect, RenderTarget2D renderTarget)
    {
        // Example: hit and hurt boxes from game coordinates
        Rectangle hitBox = new(0, 0, 5, 5);
        Rectangle hurtBox = new(100, 80, 30, 40);

        float scaleX = (float)gameViewRect.Width / renderTarget.Width;
        float scaleY = (float)gameViewRect.Height / renderTarget.Height;

        DrawRect(spriteBatch, TransformRect(hitBox, scaleX, scaleY, gameViewRect.Location), Color.Red);
        DrawRect(spriteBatch, TransformRect(hurtBox, scaleX, scaleY, gameViewRect.Location), Color.Lime);
    }

    public void DrawPanel(SpriteBatch spriteBatch, Rectangle rect, GameTime time)
    {
        spriteBatch.Draw(_pixel, rect, Color.LightBlue * 0.5f);

        Vector2 pos = new(rect.X + 20, 20);
        float fps = (float)(1 / time.ElapsedGameTime.TotalSeconds);

        spriteBatch.DrawString(_font, $"FPS: {fps:0}", pos, Color.Yellow);
        pos.Y += 30;
        spriteBatch.DrawString(_font, "=== Debug Info ===", pos, Color.Cyan);
        pos.Y += 25;
        spriteBatch.DrawString(_font, "Player State: Idle", pos, Color.Lime);
    }

    private static Rectangle TransformRect(Rectangle rect, float scaleX, float scaleY, Point offset)
    {
        return new Rectangle(
            offset.X + (int)(rect.X * scaleX),
            offset.Y + (int)(rect.Y * scaleY),
            (int)(rect.Width * scaleX),
            (int)(rect.Height * scaleY));
    }

    private void DrawRect(SpriteBatch spriteBatch, Rectangle rect, Color color)
    {
        spriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Top, rect.Width, 1), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Bottom, rect.Width, 1), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Top, 1, rect.Height), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Right, rect.Top, 1, rect.Height), color);
    }
}
