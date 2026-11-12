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
                    // A bevelled face with the body extruded down and to the right.
                    var face = new Vector2((u + .09f) / .76f, (v - .09f) / .82f);
                    var depth = new Vector2(.21f / .76f, -.19f / .82f);
                    float along = Mathf.Clamp01(Vector2.Dot(face, depth) / depth.sqrMagnitude);
                    float bodyRadius = (face - depth * along).magnitude;
                    float radius = face.magnitude;
                    Color color = Color.clear;
                    if (bodyRadius < 1f)
                    {
                        float grooves = .5f + .5f * Mathf.Cos(Mathf.Atan2(face.y, face.x) * 40f);
                        color = Color.Lerp(new Color(.48f,.21f,.035f), new Color(.92f,.53f,.10f), grooves);
                        if (radius < .97f)
                        {
                            float light = Mathf.Clamp01(.52f + .48f * (face.y - face.x));
                            color = radius > .78f
                                ? Color.Lerp(new Color(.69f,.37f,.055f), new Color(1f,.98f,.71f), light)
                                : Color.Lerp(new Color(.96f,.53f,.035f), new Color(1f,.88f,.30f), light);
                            if (Mathf.Abs(radius - .77f) < .026f) color = new Color(.66f,.35f,.055f);
                            if (Cloud(face.x - .035f, face.y + .065f)) color = new Color(.65f,.33f,.055f);
                            if (Cloud(face.x, face.y)) color = new Color(1f,.99f,.82f);
                        }
                        if (bodyRadius > .945f) color = new Color(.40f,.23f,.09f);
                        color.a = Mathf.Clamp01((1f - bodyRadius) * 40f);
                    }                    pixels[y * size + x] = color;
                }
                texture.SetPixels(pixels); texture.Apply();
                sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 128);
                return sprite;
            }
        }
        private static bool Cloud(float x, float y) =>
            Ellipse(x,y,-.28f,-.04f,.22f,.19f) || Ellipse(x,y,0,.09f,.27f,.27f) ||
            Ellipse(x,y,.28f,-.04f,.22f,.19f) || (Mathf.Abs(x)<.28f && y>-.21f && y<0);

        private static bool Ellipse(float x, float y, float cx, float cy, float rx, float ry) => (x-cx)*(x-cx)/(rx*rx)+(y-cy)*(y-cy)/(ry*ry)<1;
    }
}
