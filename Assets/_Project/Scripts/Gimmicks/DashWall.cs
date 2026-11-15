using UnityEngine;

namespace CloudHop
{
    // Suspended across the low route: a high arc clears the crest.
    public sealed class DashWall : MonoBehaviour
    {
        private readonly Transform[] pieces = new Transform[3];
        private readonly Vector3[] homes = new Vector3[3];
        private BoxCollider2D barrier;
        private Sprite shard;
        private Texture2D texture;
        private float elapsed;
        private bool broken;

        public static void Place(StageCourse course, int start)
        {
            if (start + 1 >= course.Platforms.Length) return;
            var from = course.Platforms[start].GetComponent<Collider2D>().bounds;
            var to = course.Platforms[start + 1].GetComponent<Collider2D>().bounds;
            var wall = new GameObject("Dash Crystal Wall").AddComponent<DashWall>();
            wall.transform.SetParent(course.transform, true);
            wall.transform.position = new Vector3((from.max.x + to.min.x) * .5f,
                Mathf.Min(from.max.y, to.max.y) - .35f, 0);
            wall.Build();
        }

        private void Build()
        {
            texture = new Texture2D(24, 48, TextureFormat.RGBA32, false);
            var pixels = new Color[24 * 48];
            for (int y = 0; y < 48; y++)
            for (int x = 0; x < 24; x++)
            {
                float half = Mathf.Min(11, (48 - y) * .65f, (y + 2) * 1.5f);
                float edge = half - Mathf.Abs(x - 11.5f);
                if (edge < 0) continue;
                pixels[y * 24 + x] = edge < 1.3f ? new Color(.18f,.17f,.42f) :
                    Mathf.Abs(y - (22 + Mathf.Abs(x - 12) * .65f)) < 1 ? Color.white :
                    x < 9 ? new Color(.72f,.96f,1) : x < 16 ? new Color(.35f,.62f,.94f) : new Color(.48f,.37f,.8f);
            }
            texture.SetPixels(pixels);
            texture.Apply();
            shard = Sprite.Create(texture, new Rect(0,0,24,48), new Vector2(.5f,0), 40);
            for (int i = 0; i < pieces.Length; i++)
            {
                var art = new GameObject("Cracked crystal").AddComponent<SpriteRenderer>();
                art.sprite = shard;
                art.sortingOrder = 8;
                pieces[i] = art.transform;
                pieces[i].SetParent(transform, false);
                homes[i] = new Vector3((i - 1) * .19f, 0, 0);
                pieces[i].localScale = Vector3.one * (i == 1 ? 1 : .74f);
            }
            barrier = gameObject.AddComponent<BoxCollider2D>();
            barrier.isTrigger = true;
            barrier.size = new Vector2(.63f, 1.12f);
            barrier.offset = Vector2.up * .56f;
            Restore();
        }

        public void Restore()
        {
            broken = false;
            elapsed = 0;
            barrier.enabled = true;
            for (int i = 0; i < pieces.Length; i++)
            {
                pieces[i].gameObject.SetActive(true);
                pieces[i].localPosition = homes[i];
                pieces[i].localRotation = Quaternion.Euler(0,0,(1-i)*14);
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
