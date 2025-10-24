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

    private Texture2D _pixel;


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

        // Reusable 1x1 white texture
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

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
        // 1. Render everything to the game render target
        GraphicsDevice.SetRenderTarget(_gameRenderTarget);
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _gameManager.Draw();
        GraphicsDevice.SetRenderTarget(null);

        // 2. Prepare main screen
        GraphicsDevice.Clear(Color.Black);
        Globals.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);

        if (!Globals.DebugDraw)
        {
            // Normal mode — draw with overlay renderer
            _overlayRenderer.Draw(Globals.SpriteBatch, _gameRenderTarget);
        }
        else
        {
            // --- Debug mode layout ---
            int screenWidth = GraphicsDevice.Viewport.Width;
            int screenHeight = GraphicsDevice.Viewport.Height;
            int halfWidth = screenWidth / 2;

            float aspect = (float)_gameRenderTarget.Height / _gameRenderTarget.Width;
            int gameViewHeight = (int)(halfWidth * aspect);

            // Left side: scaled render target
            Rectangle gameViewRect = new(0, 0, halfWidth, gameViewHeight);
            Globals.SpriteBatch.Draw(_gameRenderTarget, gameViewRect, Color.White);

            // Overlay hitboxes/hurtboxes on top of game view
            DrawBoxesOverlay(gameViewRect);

            // Right side: debug panel area
            Rectangle debugPanelRect = new(halfWidth, 0, halfWidth, screenHeight);
            DrawDebugPanel(gameTime, debugPanelRect);
        }

        Globals.SpriteBatch.End();
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

    private void DrawDebugPanel(GameTime gameTime, Rectangle debugRect)
    {
        // Dim background behind debug text
        Globals.SpriteBatch.Draw(_pixel, debugRect, Color.LightBlue * 0.5f);

        Vector2 textPos = new(debugRect.X + 20, 20);
        float fps = (float)(1 / gameTime.ElapsedGameTime.TotalSeconds);

        Globals.SpriteBatch.DrawString(_debugFont, $"FPS: {fps:0}", textPos, Color.Yellow);
        textPos.Y += 30;

        Globals.SpriteBatch.DrawString(_debugFont, "=== Debug Info ===", textPos, Color.Cyan);
        textPos.Y += 25;
        Globals.SpriteBatch.DrawString(_debugFont, "Player State: Idle", textPos, Color.Lime);
        textPos.Y += 25;
        Globals.SpriteBatch.DrawString(_debugFont, "Hitboxes: 2", textPos, Color.Orange);
        textPos.Y += 25;
        Globals.SpriteBatch.DrawString(_debugFont, "Hurtboxes: 3", textPos, Color.Red);
    }

    private void DrawBoxesOverlay(Rectangle gameViewRect)
    {
        // Simulated boxes in game coordinates
        Rectangle hitBox = new(0, 0, 5, 5);
        Rectangle hurtBox = new(100, 80, 30, 40);

        float scaleX = (float)gameViewRect.Width / _gameRenderTarget.Width;
        float scaleY = (float)gameViewRect.Height / _gameRenderTarget.Height;

        Rectangle hitScreen = TransformRect(hitBox, scaleX, scaleY, gameViewRect.Location);
        Rectangle hurtScreen = TransformRect(hurtBox, scaleX, scaleY, gameViewRect.Location);

        DrawDebugBox(hitScreen, Color.Red);
        DrawDebugBox(hurtScreen, Color.Lime);
    }

    private Rectangle TransformRect(Rectangle rect, float scaleX, float scaleY, Point offset)
    {
        return new Rectangle(
            offset.X + (int)(rect.X * scaleX),
            offset.Y + (int)(rect.Y * scaleY),
            (int)(rect.Width * scaleX),
            (int)(rect.Height * scaleY));
    }

    private void DrawDebugBox(Rectangle rect, Color color)
    {
        Globals.SpriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Top, rect.Width, 1), color);     // Top
        Globals.SpriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Bottom, rect.Width, 1), color);  // Bottom
        Globals.SpriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Top, 1, rect.Height), color);    // Left
        Globals.SpriteBatch.Draw(_pixel, new Rectangle(rect.Right, rect.Top, 1, rect.Height), color);   // Right
    }




    #endregion
}
