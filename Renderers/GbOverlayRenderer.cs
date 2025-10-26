namespace MonoFighter;

/// <summary>
/// Handles scaling, centering, and drawing of the overlay and the game render target.
/// </summary>
public class GbOverlayRenderer
{
    private readonly Texture2D _overlayTexture;
    private readonly Rectangle _transparentScreen;

    public GbOverlayRenderer(string overlayPath, Rectangle transparentScreen)
    {
        _overlayTexture = Globals.Content.Load<Texture2D>(overlayPath);
        _transparentScreen = transparentScreen;
    }

    public void Draw(SpriteBatch spriteBatch, RenderTarget2D gameRenderTarget)
    {
        var layout = CalculateLayout(Globals.WindowSize, new Point(_overlayTexture.Width, _overlayTexture.Height),
            _transparentScreen, Globals.VirtualResolution);

        // Draw game content
        spriteBatch.Draw(gameRenderTarget, layout.GameDestRect, Color.White);

        // Draw overlay (scaled and centered)
        spriteBatch.Draw(_overlayTexture, layout.OverlayRect, Color.White);
    }

    private OverlayLayout CalculateLayout(Point windowSize, Point overlaySize, Rectangle transparentScreen, Point virtualResolution)
    {
        float overlayScale = (float)windowSize.Y / overlaySize.Y;

        int scaledOverlayWidth = (int)(overlaySize.X * overlayScale);
        int scaledOverlayHeight = (int)(overlaySize.Y * overlayScale);
        int overlayX = (windowSize.X - scaledOverlayWidth) / 2;

        Rectangle overlayRect = new(overlayX, 0, scaledOverlayWidth, scaledOverlayHeight);

        Rectangle scaledScreen = new(
            overlayX + (int)(transparentScreen.X * overlayScale),
            (int)(transparentScreen.Y * overlayScale),
            (int)(transparentScreen.Width * overlayScale),
            (int)(transparentScreen.Height * overlayScale));

        float scaleX = (float)scaledScreen.Width / virtualResolution.X;
        float scaleY = (float)scaledScreen.Height / virtualResolution.Y;
        float gameScale = Math.Min(scaleX, scaleY);

        Rectangle gameDestRect = new(
            scaledScreen.X,
            scaledScreen.Y,
            (int)(virtualResolution.X * gameScale),
            (int)(virtualResolution.Y * gameScale));

        return new OverlayLayout(overlayRect, gameDestRect);
    }

    private readonly record struct OverlayLayout(Rectangle OverlayRect, Rectangle GameDestRect);
}
