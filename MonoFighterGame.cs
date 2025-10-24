/*
MonoGame lifecycle methods (Initialize, LoadContent, Update, Draw).
Create and initialize global services (store in Globals if you need quick access).
Create lower-level renderer objects (SpriteBatch, RenderTarget2D).
Instantiate and wire the StateMachine or a GameManager, then delegate Update/Draw to them.
Keep out:
Game object creation and game-specific asset loading (move to GameManager or Scene).
Gameplay logic.
*/

using MonoFighter.Scenes;

namespace MonoFighter;

public class MonoFighterGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private GameManager _gameManager;

    private RenderTarget2D _gameRenderTarget;

    private SpriteFont _debugFont;
    private Texture2D _overlayTexture;

    public struct OverlayLayout
    {
        public Rectangle OverlayRect;      // Where to draw the overlay on the window
        public Rectangle GameDestRect;     // Where to draw the game render target
    }

    private OverlayLayout CalculateOverlayLayout(Point windowSize, Point overlaySize, Rectangle transparentScreen, Point virtualResolution)
    {
        // 1. Calculate overlay scale based on window height
        float overlayScale = (float)windowSize.Y / overlaySize.Y;

        // Scaled overlay size
        int scaledOverlayWidth = (int)(overlaySize.X * overlayScale);
        int scaledOverlayHeight = (int)(overlaySize.Y * overlayScale);

        // Center overlay horizontally
        int overlayX = (windowSize.X - scaledOverlayWidth) / 2;
        int overlayY = 0;

        Rectangle overlayRect = new Rectangle(overlayX, overlayY, scaledOverlayWidth, scaledOverlayHeight);

        // 2. Calculate scaled transparent screen rectangle inside overlay
        Rectangle scaledScreen = new Rectangle(
            overlayX + (int)(transparentScreen.X * overlayScale),
            overlayY + (int)(transparentScreen.Y * overlayScale),
            (int)(transparentScreen.Width * overlayScale),
            (int)(transparentScreen.Height * overlayScale)
        );

        // 3. Scale game render target to fit inside scaled transparent screen
        float scaleX = (float)scaledScreen.Width / virtualResolution.X;
        float scaleY = (float)scaledScreen.Height / virtualResolution.Y;
        float gameScale = Math.Min(scaleX, scaleY);

        Rectangle gameDestRect = new Rectangle(
            scaledScreen.X,
            scaledScreen.Y,
            (int)(virtualResolution.X * gameScale),
            (int)(virtualResolution.Y * gameScale)
        );

        return new OverlayLayout
        {
            OverlayRect = overlayRect,
            GameDestRect = gameDestRect
        };
    }

    public MonoFighterGame()
    {
        // 1. Basic setup (no graphics or content yet)
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // 2. Game logic setup (still no asset loading)
        _graphics.PreferredBackBufferWidth = Globals.WindowSize.X;
        _graphics.PreferredBackBufferHeight = Globals.WindowSize.Y;
        _graphics.ApplyChanges();
        _gameManager = new();

        base.Initialize();


    }

    protected override void LoadContent()
    {
        // 3. ContentManager and GraphicsDevice ready — load assets here
        Globals.Content = Content;
        Globals.GraphicsDevice = GraphicsDevice;
        Globals.SpriteBatch = new SpriteBatch(GraphicsDevice);

        _overlayTexture = Globals.Content.Load<Texture2D>("Graphics/overlay");
        _debugFont = Globals.Content.Load<SpriteFont>("Fonts/DebugFont");

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
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        Globals.Update(gameTime);
        InputManager.Update();

        InputManager.KeyPressed(Keys.F1, () =>
        {
            Globals.DebugDraw = !Globals.DebugDraw;
        });

        InputManager.KeyPressed(Keys.F11, () =>
        {
            SetWindowSize(GetMaxWindowSize());
        });

        _gameManager.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Render game to render target
        Globals.GraphicsDevice.SetRenderTarget(_gameRenderTarget);
        Globals.GraphicsDevice.Clear(Color.CornflowerBlue);
        _gameManager.Draw();
        Globals.GraphicsDevice.SetRenderTarget(null);
        Globals.GraphicsDevice.Clear(Color.Black);

        Globals.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Calculate overlay layout
        OverlayLayout layout = CalculateOverlayLayout(
            Globals.WindowSize,
            new Point(_overlayTexture.Width, _overlayTexture.Height),
            new Rectangle(145, 25, 209, 155), // transparent screen
            Globals.VirtualResolution
        );

        // Draw scaled game
        Globals.SpriteBatch.Draw(_gameRenderTarget, layout.GameDestRect, Color.White);

        // Draw scaled overlay
        Globals.SpriteBatch.Draw(_overlayTexture, layout.OverlayRect, Color.White);

        Globals.SpriteBatch.End();

        if (Globals.DebugDraw)
            DrawDebugOverlay(gameTime);

        base.Draw(gameTime);
    }

    private Rectangle GetScaledDestinationRect()
    {
        float scaleX = (float)Globals.WindowSize.X / Globals.VirtualResolution.X;
        float scaleY = (float)Globals.WindowSize.Y / Globals.VirtualResolution.Y;
        float scale = Math.Min(scaleX, scaleY); // keep aspect ratio

        int w = (int)(Globals.VirtualResolution.X * scale);
        int h = (int)(Globals.VirtualResolution.Y * scale);
        int x = (Globals.WindowSize.X - w) / 2;
        int y = (Globals.WindowSize.Y - h) / 2;

        return new Rectangle(x, y, w, h);
    }

    private void DrawDebugOverlay(GameTime gameTime)
    {
        Globals.SpriteBatch.Begin(samplerState: SamplerState.LinearClamp, blendState: BlendState.AlphaBlend);

        // Example: Draw debug text
        Globals.SpriteBatch.DrawString(_debugFont, $"FPS: {(1 / gameTime.ElapsedGameTime.TotalSeconds):0}", new Vector2(10, 10), Color.Yellow);

        // Example: Draw hit/hurt boxes (from world→screen transform)
        DrawDebugBox(new Rectangle(50, 50, 20, 30), Color.Red);
        DrawDebugBox(new Rectangle(100, 60, 16, 40), Color.Lime);

        Globals.SpriteBatch.End();
    }

    private void DrawDebugBox(Rectangle rect, Color color)
    {
        Texture2D pixel = new Texture2D(GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        // outline only
        Globals.SpriteBatch.Draw(pixel, new Rectangle(rect.Left, rect.Top, rect.Width, 1), color);
        Globals.SpriteBatch.Draw(pixel, new Rectangle(rect.Left, rect.Bottom, rect.Width, 1), color);
        Globals.SpriteBatch.Draw(pixel, new Rectangle(rect.Left, rect.Top, 1, rect.Height), color);
        Globals.SpriteBatch.Draw(pixel, new Rectangle(rect.Right, rect.Top, 1, rect.Height), color);
    }

    private Vector2 VirtualToScreen(Vector2 virtualPos)
    {
        var dest = GetScaledDestinationRect();
        float scale = (float)dest.Width / Globals.VirtualResolution.X;
        return new Vector2(dest.X + virtualPos.X * scale, dest.Y + virtualPos.Y * scale);
    }

    private Vector2 ScreenToVirtual(Vector2 screenPos)
    {
        var dest = GetScaledDestinationRect();
        float scale = (float)dest.Width / Globals.VirtualResolution.X;

        // If outside game area, you can decide whether to clamp or return NaN
        return new Vector2((screenPos.X - dest.X) / scale, (screenPos.Y - dest.Y) / scale);
    }

    public void SetWindowSize(Point newSize)
    {
        Globals.WindowSize = newSize;

        _graphics.PreferredBackBufferWidth = newSize.X;
        _graphics.PreferredBackBufferHeight = newSize.Y;
        _graphics.ApplyChanges();
    }

    public Point GetMaxWindowSize()
    {
        var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        return new Point(displayMode.Width, displayMode.Height);
    }


}