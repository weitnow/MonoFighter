using System.Collections.Generic;
using System.Diagnostics;

namespace MonoFighter.Debugging;


public class DebugManager
{
    public string PlayerState { get; set; } = "Idle";
    public int Hitboxes { get; set; } = 0;
    public int Hurtboxes { get; set; } = 0;

    // === internal ===
    private DebugRenderer _debugRenderer;

    private RenderTarget2D _gameRenderTarget;

    private int _halfWidth;
    private float _aspect;
    private int _gameViewHeight;

    private readonly List<GameObject> _trackedObjects = new();

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

        _debugRenderer.Update(gameTime);

    }

    public void Draw()
    {
        // Draw game view on the left half
        DrawGameView();

        // Draw debug info panel on the right half
        DrawDebugPanel();






        // Draw debug boxes over game view
        Rectangle hitBox = new(0, 0, 5, 5);
        Rectangle hurtBox = new(5, 5, 5, 5);
        DrawBox(hitBox, Color.Red);
        DrawBox(hurtBox, Color.Lime);


    }

    private void DrawGameView()
    {
        // Draw game view on the left half
        Globals.SpriteBatch.Draw(_gameRenderTarget, _gameViewRect, Color.White);
    }

    private void DrawDebugPanel()
    {
        // Draw debug info panel on the right half
        _debugRenderer.DrawPanel();
    }

    private void DrawBox(Rectangle rect, Color color)
    {
        _debugRenderer.DrawBox(rect, color, _gameViewRect, _gameRenderTarget);
    }

    public void Add(GameObject obj)
    {
        if (!_trackedObjects.Contains(obj))
            _trackedObjects.Add(obj);
    }

    public void Remove(GameObject obj)
    {
        _trackedObjects.Remove(obj);
    }




}
