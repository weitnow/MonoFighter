using System.Diagnostics;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

using MonoFighter.Debugging;


namespace MonoFighter;

public static class Globals
{
    // Configuration constants
    public static readonly Point VirtualResolution = new(209, 155);

    // Runtime systems
    public static ContentManager Content { get; set; }
    public static SpriteBatch SpriteBatch { get; set; }
    public static GraphicsDevice GraphicsDevice { get; set; }
    public static Point WindowSize { get; set; } = new(1920, 1080);
    public static bool DebugDraw { get; set; } = true;

    public static DebugManager DebugManager { get; set; }

    // Timing
    public static float DeltaTime { get; private set; }
    public static double TotalTime { get; private set; }

    public static void Update(GameTime gameTime)
    {
        DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        TotalTime += DeltaTime;
        DebugManager?.Update(gameTime);
    }

    public static class ResolutionPresets
    {
        public static readonly Dictionary<string, Point> Values = new()
        {
            ["R_640x480"] = new Point(640, 480),
            ["R_800x600"] = new Point(800, 600),
            ["R_1280x720"] = new Point(1280, 720),
            ["R_1920x1080"] = new Point(1920, 1080),
            ["R_2560x1440"] = new Point(2560, 1440),
            ["R_3440x1440"] = new Point(3440, 1440),
            ["R_3840x2160"] = new Point(3840, 2160)
        };
    }

}




