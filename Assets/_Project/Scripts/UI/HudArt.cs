using System.Collections.Generic;
using UnityEngine;
namespace CloudHop
{
    public static class HudArt
    {
        private static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        // Crop transparent margins at runtime without modifying the original generated PNGs.
        public static Sprite Get(string key)
        {
            if (sprites.TryGetValue(key, out var sprite) && sprite != null) return sprite;
            var texture = Resources.Load<Texture2D>("HudArt/" + key);
            if (texture == null) return null;
            Rect rect;
            Vector2 original = new Vector2(2172, 724);
            switch (key)
            {
                case "CloudMeter": rect = new Rect(24, 217, 2124, 276); break;
                case "LandingTarget": rect = new Rect(30, 195, 2110, 328); break;
                case "InfoPanel": rect = new Rect(19, 196, 2134, 367); break;
                default: original = new Vector2(1278, 1230); rect = new Rect(38, 24, 1204, 1179); break;
            }
            rect = new Rect(rect.x * texture.width / original.x, rect.y * texture.height / original.y,
                rect.width * texture.width / original.x, rect.height * texture.height / original.y);
            var border = key == "InfoPanel" ? new Vector4(360 * texture.width / original.x, 0, 360 * texture.width / original.x, 0) : Vector4.zero;
            sprite = Sprite.Create(texture, rect, new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, border);
            sprite.name = key; sprite.hideFlags = HideFlags.DontSave;
            sprites[key] = sprite; return sprite;
        }
        public static Sprite Fill
        {
            get
            {
                if (sprites.TryGetValue("Fill", out var sprite) && sprite != null) return sprite;
                var texture = new Texture2D(256, 32, TextureFormat.RGBA32, false);
                texture.name = "HUD glossy capsule"; texture.hideFlags = HideFlags.DontSave;
                texture.wrapMode = TextureWrapMode.Clamp;
                var pixels = new Color[256 * 32];
                for (int y = 0; y < 32; y++) for (int x = 0; x < 256; x++)
                {
                    float dx = x < 16 ? 15.5f - x : x > 239 ? x - 239.5f : 0;
                    float dy = y - 15.5f;
                    float alpha = Mathf.Clamp01(16 - Mathf.Sqrt(dx * dx + dy * dy));
                    float light = y > 17 ? 1 : .72f + y / 64f;
                    pixels[y * 256 + x] = new Color(light, light, light, alpha);
                }
                texture.SetPixels(pixels); texture.Apply(false, true);
                sprite = Sprite.Create(texture, new Rect(0, 0, 256, 32), new Vector2(.5f, .5f));
                sprite.name = "Glossy fill"; sprite.hideFlags = HideFlags.DontSave;
                sprites["Fill"] = sprite; return sprite;
            }
        }
    }
}
