using UnityEngine;

namespace CloudHop
{
    // The wall spans the low route. A charged arc can clear its flat crest.
    public sealed class DashWall : MonoBehaviour
    {
        private readonly Transform[] pieces = new Transform[3];
        private readonly Vector3[] homes = new Vector3[3];
        private BoxCollider2D barrier, solid;
        private float height;
        private Platform landingPlatform;
        private Sprite shard;
        private Texture2D texture;
        private float elapsed;
        private bool broken;

        public static void Place(StageCourse course, int start)
        {
            if (start + 1 >= course.Platforms.Length) return;
            var from = course.Platforms[start].GetComponent<Collider2D>().bounds;
            var to = course.Platforms[start + 1].GetComponent<Collider2D>().bounds;
            var wall = new GameObject("Breakable Brick Wall").AddComponent<DashWall>();
            wall.transform.SetParent(course.transform, true);
            wall.transform.position = new Vector3((from.max.x + to.min.x) * .5f,
                Mathf.Min(from.max.y, to.max.y) - 1.6f, 0);
            wall.height = from.max.y + 1.7f - wall.transform.position.y;
            wall.landingPlatform = course.Platforms[start + 1];
            wall.Build();
        }

        private void Build()
        {
            texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            var pixels = new Color[32 * 32];
            for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
            {
                bool mortar = y < 2 || y > 29 || (x + (y < 16 ? 0 : 16)) % 32 < 2 || y == 15;
                bool edge = x < 2 || x > 29;
                bool crack = Mathf.Abs(x - (16 + (y % 12 - 6) * .6f)) < 1;
                pixels[y * 32 + x] = mortar || edge ? new Color(.22f,.18f,.33f) :
                    crack ? new Color(.95f,.83f,.55f) :
                    y % 16 > 12 ? new Color(.87f,.70f,.52f) : new Color(.59f,.43f,.39f);
            }
            texture.SetPixels(pixels);
            texture.Apply();
            shard = Sprite.Create(texture, new Rect(0,0,32,32), new Vector2(.5f,0), 32);
            for (int i = 0; i < pieces.Length; i++)
            {
                var art = new GameObject("Cracked masonry").AddComponent<SpriteRenderer>();
                art.sprite = shard;
                art.sortingOrder = 8;
                pieces[i] = art.transform;
                pieces[i].SetParent(transform, false);
                homes[i] = new Vector3(0, i * height / 3f, 0);
                pieces[i].localScale = new Vector3(.5f, height / 3f, 1);
            }
            barrier = gameObject.AddComponent<BoxCollider2D>();
            barrier.isTrigger = true;
            barrier.size = new Vector2(.85f, height);
            barrier.offset = Vector2.up * height * .5f;
            solid = gameObject.AddComponent<BoxCollider2D>();
            solid.size = new Vector2(.5f, height);
            solid.offset = barrier.offset;
            Restore();
        }

        public void Restore()
        {
            broken = false;
            elapsed = 0;
            barrier.enabled = true;
            solid.enabled = true;
            for (int i = 0; i < pieces.Length; i++)
            {
                pieces[i].gameObject.SetActive(true);
                pieces[i].localPosition = homes[i];
                pieces[i].localRotation = Quaternion.identity;
            }
        }

        private void OnTriggerEnter2D(Collider2D other) => Contact(other);
        private void OnTriggerStay2D(Collider2D other) => Contact(other);

        private void Contact(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (!player || broken) return;
            if (!player.IsBoosting)
            {
                player.TryKnockback(new Vector2(-3,4));
                return;
            }
            broken = true;
            barrier.enabled = false;
            solid.enabled = false;
            player.ReboundFromWall(landingPlatform);
            CloudAudio.Cue("chop", .65f);
        }

        private void Update()
        {
            if (!broken || elapsed >= .32f) return;
            elapsed += Time.deltaTime;
            for (int i = 0; i < pieces.Length; i++)
            {
                pieces[i].localPosition = homes[i] + new Vector3((i-1)*3f+1, 4f-12f*elapsed, 0)*elapsed;
                pieces[i].Rotate(0,0,(i-1)*240f*Time.deltaTime);
                if (elapsed >= .32f) pieces[i].gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (shard) Destroy(shard);
            if (texture) Destroy(texture);
        }
    }
}
