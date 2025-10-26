namespace MonoFighter.Debugging;


public class DebugManager
{
    public string PlayerState { get; set; } = "Idle";
    public int Hitboxes { get; set; } = 0;
    public int Hurtboxes { get; set; } = 0;

    public void Draw(DebugRenderer renderer, GameTime gameTime)
    {
        renderer.DrawPanel(gameTime);
    }
}
