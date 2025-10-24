using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AsepriteDotNet.Aseprite;

namespace MonoFighter;

public class GameObject
{
    private Dictionary<string, AnimatedSprite> _animations; // all animations (assetName-tag)
    private AnimatedSprite _currentAnimation;

    public Vector2 Scale
    {
        get => _currentAnimation?.Scale ?? Vector2.One;
        set
        {
            if (_currentAnimation != null)
                _currentAnimation.Scale = value;
        }
    }

    // ---- Empty constructor ----
    public GameObject()
    {
        _animations = new Dictionary<string, AnimatedSprite>();
    }

    // ---- Load Aseprite asset and add animations ----
    public void LoadFromAseprite(string assetName)
    {
        var aseFile = Globals.Content.Load<AsepriteFile>(assetName);

        var spriteSheet = aseFile.CreateSpriteSheet(
            Globals.GraphicsDevice,
            onlyVisibleLayers: true,
            includeBackgroundLayer: false,
            includeTilemapLayers: false,
            mergeDuplicateFrames: true,
            borderPadding: 0,
            spacing: 0,
            innerPadding: 0
        );

        // Extract only the file name after the last '/'
        string cleanName = assetName;
        int lastSlash = assetName.LastIndexOf('/');
        if (lastSlash >= 0 && lastSlash < assetName.Length - 1)
            cleanName = assetName.Substring(lastSlash + 1);

        // Add each tag as "cleanName-tagName"
        foreach (var tag in spriteSheet.GetAnimationTagNames())
        {
            string key = $"{cleanName}-{tag}";
            _animations[key] = spriteSheet.CreateAnimatedSprite(tag);
        }

        // If no animation currently playing, try to start Idle
        if (_currentAnimation == null)
        {
            string idleKey = $"{cleanName}-Idle";
            if (_animations.ContainsKey(idleKey))
            {
                _currentAnimation = _animations[idleKey];
                _currentAnimation.Play();
            }
        }
    }

    public void Play(string animationKey)
    {
        if (_animations.TryGetValue(animationKey, out var newAnim))
        {
            if (_currentAnimation == newAnim)
                return; // already playing

            _currentAnimation = newAnim;
            _currentAnimation.Play();
        }
        else
        {
            Console.WriteLine($"[Warning] Animation '{animationKey}' not found.");
        }
    }

    public void Update(GameTime gameTime)
    {
        _currentAnimation?.Update(gameTime);
    }

    public void Draw(Vector2 position)
    {
        _currentAnimation?.Draw(Globals.SpriteBatch, position);
    }
}
