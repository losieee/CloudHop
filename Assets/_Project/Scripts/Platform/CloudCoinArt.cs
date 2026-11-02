using UnityEngine;
namespace CloudHop
{
    // Shared code-drawn coin artwork for the world and HUD.
    public static class CloudCoinArt
    {
        private static Sprite sprite, silver, crystal;
        public static Sprite Silver => Variant(false);
        public static Sprite Crystal => Variant(true);
        private static Sprite Variant(bool gem)
        {
            if (gem ? crystal != null : silver != null) return gem ? crystal : silver;
            var source = Sprite.texture; int size = source.width;
            var texture = new Texture2D(size,size,TextureFormat.RGBA32,false);
            var pixels = source.GetPixels();
            for(int y=0;y<size;y++)for(int x=0;x<size;x++) {
                int i=y*size+x; var c=pixels[i];
                if(!gem) { float v=c.grayscale; pixels[i]=new Color(v*.82f,v*.94f,Mathf.Min(1,v*1.15f),c.a); }
                else {
                    float u=(x+.5f-size/2f)/(size/2f), v=(y+.5f-size/2f)/(size/2f);
                    float edge=Mathf.Abs(u)*1.3f+Mathf.Abs(v);
                    pixels[i]=edge>.92f?Color.clear:edge>.8f?new Color(.85f,1,1):u<0?new Color(.2f,.8f,1):new Color(.1f,.45f,.95f);
                    if(edge<.7f && Mathf.Abs(u+v*.15f)<.055f)pixels[i]=Color.white;
                }
            }
            texture.SetPixels(pixels);texture.Apply();
            var result=Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),128);
            if(gem)crystal=result;else silver=result;
            return result;
        }
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
