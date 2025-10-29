using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace MonoFighter.Debugging;

public class DebugRenderer
{
    // === internal ===
    private readonly Process _process;
    private double _lastCpuTime;
    private double _cpuUsagePercent;
    private readonly int _cpuCoreCount;
    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;
    private int _halfScreenWidth { get { return Globals.WindowSize.X / 2; } }

    public DebugRenderer()
    {
        _font = Globals.Content.Load<SpriteFont>("Fonts/DebugFont");

        // Reusable 1x1 white texture
        _pixel = new Texture2D(Globals.GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _process = Process.GetCurrentProcess();
        _cpuCoreCount = Environment.ProcessorCount;


    }

    public void Update(GameTime gameTime)
    {
        // --- CPU usage (approximate) ---
        _process.Refresh();
        double currentCpuTime = _process.TotalProcessorTime.TotalMilliseconds;
        double deltaCpuTime = currentCpuTime - _lastCpuTime;
        _lastCpuTime = currentCpuTime;

        // (CPU usage per frame / number of cores)
        double deltaRealTime = gameTime.ElapsedGameTime.TotalMilliseconds;
        if (deltaRealTime > 0)
            _cpuUsagePercent = (deltaCpuTime / (deltaRealTime * _cpuCoreCount)) * 100.0;
    }


    public void DrawPanel()
    {

        // Draw semi-transparent background for the debug area
        Rectangle panelRect = new(_halfScreenWidth, 0, _halfScreenWidth, Globals.WindowSize.Y);
        Globals.SpriteBatch.Draw(_pixel, panelRect, Color.White * 0.5f);


        Vector2 pos = new(panelRect.X + 20, 20);
        float fps = (float)(1 / Globals.DeltaTime);

        double memoryMB = _process.WorkingSet64 / (1024.0 * 1024.0);

        string text =
                $"RAM: {memoryMB:0.0} MB\n" +
                $"CPU: {_cpuUsagePercent:0.0}%\n" +
                $"Threads: {_process.Threads.Count}";

        Globals.SpriteBatch.DrawString(_font, $"FPS: {fps:0}", pos, Color.Yellow);
        pos.Y += 30;
        Globals.SpriteBatch.DrawString(_font, "=== Debug Info ===", pos, Color.Cyan);
        pos.Y += 25;
        Globals.SpriteBatch.DrawString(_font, "Player State: Idle", pos, Color.Lime);
        pos.Y += 20;
        Globals.SpriteBatch.DrawString(_font, text, pos, Color.LimeGreen);




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

    internal void DrawBox(Rectangle rect, Color color, Rectangle gameViewRect, RenderTarget2D renderTarget)
    {
        float scaleX = (float)gameViewRect.Width / renderTarget.Width;
        float scaleY = (float)gameViewRect.Height / renderTarget.Height;

        DrawRect(TransformRect(rect, scaleX, scaleY, gameViewRect.Location), color);
    }
}
