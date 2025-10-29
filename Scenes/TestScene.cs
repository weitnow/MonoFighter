
/*
TestScene.cs — what belongs here (scene-local responsibilities)
Responsibilities:
Instantiate scene-local GameObject instances.
Load scene-specific assets (LoadFromAseprite calls), initialize objects.
Implement Update and Draw for the scene's objects (you already do).
Clean up resources in Unload if needed (e.g., dispose Textures not managed by Content).
Keep out:
Globals initialization (that's Game1).
Cross-scene orchestration (GameManager or a higher system).
*/

using System;

namespace MonoFighter.Scenes;

public class TestScene : IScene
{
    GameObject _fighter;

    public void Load()
    {
        _fighter = new();
        _fighter.LoadFromAseprite("Graphics/gbFighter");
        _fighter.LoadFromAseprite("Graphics/nesFighter");
        _fighter.LoadFromAseprite("Graphics/debug32");
        _fighter.Play("gbFighter-Idle");

        Globals.DebugManager.Add(_fighter);
    }

    public void Unload()
    {
        // Cleanup resources if needed

    }

    public void Update(GameTime gameTime)
    {
        _fighter.Update(gameTime);
    }

    public void Draw()
    {
        Globals.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _fighter.Draw(new Vector2(10, 10));

        Globals.SpriteBatch.End();
    }
}
