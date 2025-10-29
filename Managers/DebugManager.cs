using System.Diagnostics;

namespace MonoFighter.Debugging;


public class DebugManager
{
    public string PlayerState { get; set; } = "Idle";
    public int Hitboxes { get; set; } = 0;
    public int Hurtboxes { get; set; } = 0;

    private DebugRenderer _debugRenderer;

    private RenderTarget2D _gameRenderTarget;

    private int _halfWidth;
    private float _aspect;
    private int _gameViewHeight;

    private Rectangle _gameViewRect;

    public DebugManager(RenderTarget2D gameRenderTarget)
    {
        _gameRenderTarget = gameRenderTarget;
        _debugRenderer = new(); // need to be after Content and GraphicsDevice are set
    }


    public void Initialize()
    {
        // Any initialization logic for the DebugManager can go here
    }

    public void LoadContent()
    {
        // Any content loading logic for the DebugManager can go here
    }

    public void Update(GameTime gameTime)
    {
        _halfWidth = Globals.WindowSize.X / 2;
        _aspect = (float)_gameRenderTarget.Height / _gameRenderTarget.Width;
        _gameViewHeight = (int)(_halfWidth * _aspect);
        _gameViewRect = new Rectangle(0, 0, _halfWidth, _gameViewHeight);

    }

    public void Draw()
    {
        // Draw game view on the left half
        Globals.SpriteBatch.Draw(_gameRenderTarget, _gameViewRect, Color.White);

        // Draw debug boxes over game view
        _debugRenderer.DrawBoxes(_gameViewRect, _gameRenderTarget);

        // Draw debug info panel on the right half
        _debugRenderer.DrawPanel();
    }
}
