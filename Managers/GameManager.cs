/*
GameManager.cs — what belongs here (make this the high-level orchestrator)
Responsibilities:
Own the active scene / state machine and handle scene transitions (ChangeScene is good).
Hold or manage persistent systems used across scenes (audio manager, input manager wiring, entity manager).
Provide APIs for global gameplay operations (StartGame, Pause, GoToMenu).
Keep out:
Per-scene asset loading or per-object lifetime management (the Scene should handle that).
*/

using System;
using System.Collections.Generic;
using MonoFighter.Scenes;


namespace MonoFighter;

public class GameManager
{

    private IScene _currentScene;

    public GameManager()
    {
    }

    public void Update(GameTime gameTime)
    {
        _currentScene.Update(gameTime);
    }

    public void Draw()
    {

        _currentScene.Draw();
    }

    public void ChangeScene(IScene newScene)
    {
        _currentScene?.Unload();
        _currentScene = newScene;
        _currentScene.Load();
    }






}
