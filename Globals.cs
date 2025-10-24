using Microsoft.Xna.Framework.Content;

namespace MonoFighter;

public static class Globals
{
    public static float Time { get; private set; }
    public static ContentManager Content { get; set; }
    public static SpriteBatch SpriteBatch { get; set; }
    public static GraphicsDevice GraphicsDevice { get; set; }
    public static Point WindowSize { get; set; } = new(1920, 1080);

    public static Point VirtualResolution { get; } = new(256, 144);

    public static bool DebugDraw { get; set; } = true;



    public static void Update(GameTime gt)
    {
        Time = (float)gt.ElapsedGameTime.TotalSeconds;
    }
}

/*
Common Window Sizes:
Auto
3480 x 2160
2560 x 1440
1920 x 1080
1280 x 720
800 x 600
640 x 480




    R_256x144,   // orginal resolution 16:9
    R_512x288,   // x2
    R_768x432,   // x3
    R_1024x576,  // x4
    R_1120x630,  // DEBUG_DRAW
    R_1280x720,  // x5
    R_1536x864,  // x6
    R_1792x1008, // x7
    R_2560x1440  // x10

*/