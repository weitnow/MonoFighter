using MonoFighter.Debugging;
using MonoFighter.Scenes;
using System.Collections.Generic;
using System.Diagnostics;

namespace MonoFighter;

/// <summary>
/// Main MonoGame entry point. Handles initialization, lifecycle, and delegates game logic to GameManager.
/// </summary>
public class MonoFighterGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private GameManager _gameManager;

    private RenderTarget2D _gameRenderTarget;
    private GbOverlayRenderer _overlayRenderer;

    private int _currentResolutionIndex = 0;

    public MonoFighterGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }


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

        _overlayRenderer = new GbOverlayRenderer("Graphics/overlay", new Rectangle(145, 25, 209, 155)); // first two params x/y of the first top right pixel of the screen area, second two parameters with and height of the screen area

        _gameRenderTarget = new RenderTarget2D(
            GraphicsDevice,
            Globals.VirtualResolution.X,
            Globals.VirtualResolution.Y,
            false,
            SurfaceFormat.Color,
            DepthFormat.None,
            0,
            RenderTargetUsage.DiscardContents);

        Globals.DebugManager = new DebugManager(_gameRenderTarget);

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
            // === Debug mode layout ===

            Globals.DebugManager.Draw(gameTime); // draw the debug info panel on the right half
        }

        Globals.SpriteBatch.End();
        base.Draw(gameTime);
    }



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
        var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        int maxWidth = displayMode.Width;
        int maxHeight = displayMode.Height;

        var available = new List<Point>();

        foreach (var res in Globals.ResolutionPresets.Values.Values)
        {
            if (res.X <= maxWidth && res.Y <= maxHeight)
                available.Add(res);
        }

        return available;
    }

}