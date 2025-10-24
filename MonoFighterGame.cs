using MonoFighter.Scenes;
using System.Collections.Generic;

namespace MonoFighter;

/// <summary>
/// Main MonoGame entry point. Handles initialization, lifecycle, and delegates game logic to GameManager.
/// </summary>
public class MonoFighterGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private GameManager _gameManager;

    private RenderTarget2D _gameRenderTarget;
    private SpriteFont _debugFont;
    private OverlayRenderer _overlayRenderer;

    private int _currentResolutionIndex = 0;

    public MonoFighterGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    #region === MonoGame Lifecycle ===

    protected override void Initialize()
    {
        SetupInitialGraphics();
        _gameManager = new();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        // Global references
        Globals.Content = Content;
        Globals.GraphicsDevice = GraphicsDevice;
        Globals.SpriteBatch = new SpriteBatch(GraphicsDevice);

        _debugFont = Globals.Content.Load<SpriteFont>("Fonts/DebugFont");
        _overlayRenderer = new OverlayRenderer("Graphics/overlay", new Rectangle(145, 25, 209, 155));

        _gameRenderTarget = new RenderTarget2D(
            GraphicsDevice,
            Globals.VirtualResolution.X,
            Globals.VirtualResolution.Y,
            false,
            SurfaceFormat.Color,
            DepthFormat.None,
            0,
            RenderTargetUsage.DiscardContents);

        _gameManager.ChangeScene(new TestScene());
    }

    protected override void Update(GameTime gameTime)
    {
        HandleGlobalInput();

        Globals.Update(gameTime);
        InputManager.Update();
        _gameManager.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // 1. Render the game scene to the offscreen target
        Globals.GraphicsDevice.SetRenderTarget(_gameRenderTarget);
        Globals.GraphicsDevice.Clear(Color.CornflowerBlue);
        _gameManager.Draw();
        Globals.GraphicsDevice.SetRenderTarget(null);

        // 2. Render final composition to screen
        Globals.GraphicsDevice.Clear(Color.Black);

        Globals.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        if (!Globals.DebugDraw)
        {
            // Normal game rendering with overlay
            _overlayRenderer.Draw(Globals.SpriteBatch, _gameRenderTarget);
        }
        else
        {
            // Debug mode: draw scaled render target (half screen width)

            var windowWidth = Globals.WindowSize.X;
            var windowHeight = Globals.WindowSize.Y;

            // Half width of window
            float targetWidth = windowWidth / 2f;

            // Maintain aspect ratio of render target
            float aspect = (float)_gameRenderTarget.Height / _gameRenderTarget.Width;
            float targetHeight = targetWidth * aspect;

            // Destination rectangle: top-left corner
            Rectangle destRect = new Rectangle(
                x: 0,
                y: 0,
                width: (int)targetWidth,
                height: (int)targetHeight
            );

            // Draw scaled render target
            Globals.SpriteBatch.Draw(_gameRenderTarget, destRect, Color.White);
        }

        Globals.SpriteBatch.End();

        // 3. Optional: draw debug overlay (later will occupy the right half)
        if (Globals.DebugDraw)
            DrawDebugOverlay(gameTime);

        base.Draw(gameTime);
    }

    #endregion

    #region === Graphics & Input Helpers ===

    private void SetupInitialGraphics()
    {
        _graphics.PreferredBackBufferWidth = Globals.WindowSize.X;
        _graphics.PreferredBackBufferHeight = Globals.WindowSize.Y;
        _graphics.ApplyChanges();
    }

    private void HandleGlobalInput()
    {
        // Toggle debug overlay
        InputManager.KeyPressed(Keys.F1, () => Globals.DebugDraw = !Globals.DebugDraw);

        // Toggle fullscreen or max window
        InputManager.KeyPressed(Keys.F11, () =>
        {
            var available = GetAvailableResolutions();
            if (available.Count > 0)
            {
                var maxRes = available[available.Count - 1]; // last = largest
                SetWindowSize(maxRes);
            }
        });

        // Cycle through resolutions with F10
        InputManager.KeyPressed(Keys.F10, () =>
        {
            var available = GetAvailableResolutions();
            if (available.Count == 0) return;

            _currentResolutionIndex = (_currentResolutionIndex + 1) % available.Count;
            SetWindowSize(available[_currentResolutionIndex]);
        });

        // Exit
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
    }

    public void SetWindowSize(Point newSize)
    {
        Globals.WindowSize = newSize;
        _graphics.PreferredBackBufferWidth = newSize.X;
        _graphics.PreferredBackBufferHeight = newSize.Y;
        _graphics.ApplyChanges();
    }

    public List<Point> GetAvailableResolutions()
    {
        // Get monitor's current max resolution
        var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        int maxWidth = displayMode.Width;
        int maxHeight = displayMode.Height;

        var available = new List<Point>();

        foreach (Globals.ResolutionPreset preset in Enum.GetValues(typeof(Globals.ResolutionPreset)))
        {
            Point res = preset switch
            {
                Globals.ResolutionPreset.R_640x480 => new Point(640, 480),
                Globals.ResolutionPreset.R_800x600 => new Point(800, 600),
                Globals.ResolutionPreset.R_1280x720 => new Point(1280, 720),
                Globals.ResolutionPreset.R_1920x1080 => new Point(1920, 1080),
                Globals.ResolutionPreset.R_2560x1440 => new Point(2560, 1440),
                Globals.ResolutionPreset.R_3440x1440 => new Point(3440, 1440),
                Globals.ResolutionPreset.R_3840x2160 => new Point(3840, 2160),
                _ => new Point(1920, 1080)
            };

            if (res.X <= maxWidth && res.Y <= maxHeight)
                available.Add(res);
        }

        return available;
    }

    #endregion

    #region === Debug Overlay ===

    private void DrawDebugOverlay(GameTime gameTime)
    {
        Globals.SpriteBatch.Begin(samplerState: SamplerState.LinearClamp, blendState: BlendState.AlphaBlend);
        Globals.SpriteBatch.DrawString(_debugFont, $"FPS: {(1 / gameTime.ElapsedGameTime.TotalSeconds):0}", new Vector2(10, 10), Color.Yellow);
        DrawDebugBox(new Rectangle(50, 50, 20, 30), Color.Red);
        DrawDebugBox(new Rectangle(100, 60, 16, 40), Color.Lime);
        Globals.SpriteBatch.End();
    }

    private void DrawDebugBox(Rectangle rect, Color color)
    {
        using var pixel = new Texture2D(GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        Globals.SpriteBatch.Draw(pixel, new Rectangle(rect.Left, rect.Top, rect.Width, 1), color);
        Globals.SpriteBatch.Draw(pixel, new Rectangle(rect.Left, rect.Bottom, rect.Width, 1), color);
        Globals.SpriteBatch.Draw(pixel, new Rectangle(rect.Left, rect.Top, 1, rect.Height), color);
        Globals.SpriteBatch.Draw(pixel, new Rectangle(rect.Right, rect.Top, 1, rect.Height), color);
    }

    #endregion
}
