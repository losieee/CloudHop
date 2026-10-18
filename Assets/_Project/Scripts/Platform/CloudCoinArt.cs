using UnityEngine;
namespace CloudHop
{
    // Shared code-drawn coin artwork for the world and HUD.
    public static class CloudCoinArt
    {
        private static Sprite sprite;
        public static Sprite Sprite
        {
            get
            {
                if (sprite != null) return sprite;
                const int size = 96;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                texture.name = "Cloud coin"; texture.filterMode = FilterMode.Bilinear;
                var pixels = new Color[size * size];
                for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
                {
                    float u = (x + .5f - size / 2f) / (size / 2f), v = (y + .5f - size / 2f) / (size / 2f);
                    float radius = Mathf.Sqrt(u * u + v * v);
                    Color color = Color.clear;
                    if (radius < .94f)
                    {
                        color = radius > .83f ? new Color(.7f, .33f, .04f) : radius > .71f ? new Color(1, .95f, .49f) : Color.Lerp(new Color(1, .57f, .03f), new Color(1, .87f, .28f), (v + 1) / 2);
                        bool cloud = Ellipse(u, v, -.27f, -.06f, .21f, .19f) || Ellipse(u, v, 0, .09f, .27f, .28f) || Ellipse(u, v, .27f, -.06f, .21f, .19f) || (Mathf.Abs(u) < .28f && v > -.24f && v < 0);
                        if (cloud) color = new Color(1, 1, .94f);
                        color.a = Mathf.Clamp01((.94f - radius) * size);
                    }
                    pixels[y * size + x] = color;
                }
                texture.SetPixels(pixels); texture.Apply();
                sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 128);
                return sprite;
            }
        }
        private static bool Ellipse(float x, float y, float cx, float cy, float rx, float ry) => (x-cx)*(x-cx)/(rx*rx)+(y-cy)*(y-cy)/(ry*ry)<1;
    }
}
