using System;

namespace MonoFighter.Scenes;

public interface IScene
{
    void Load();
    void Unload();
    void Update(GameTime gameTime);
    void Draw();

}
