using UnityEngine;

namespace CloudHop
{
    [DefaultExecutionOrder(210)]
    public sealed class CastleGallery : MonoBehaviour
    {
        private Camera view;
        private Sprite panel;
        private Sprite ledge;
        private readonly SpriteRenderer[] walls = new SpriteRenderer[3];

        public void Configure(Camera camera)
        {
            view = camera;
            var stone = Resources.Load<Texture2D>("GalleryLedge");
            // Trim transparent margins and anchor the marble lip to the landing surface.
            var cut = new Rect(stone.width * .009f, stone.height * .22f,
                stone.width * .982f, stone.height * .48f);
            ledge = Sprite.Create(stone, cut, new Vector2(.5f, 1f), 100f, 0, SpriteMeshType.FullRect);
            var course = GetComponent<StageCourse>();
            foreach (var platform in GetComponentsInChildren<Platform>(true))
            {
                float depth = ConfigureSurface(course, platform);
                var gimmick = platform.GetComponent<GimmickArtwork>();
                if (gimmick != null && gimmick.enabled)
                {
                    gimmick.SetGalleryLedge(ledge, depth);
                    continue;
                }
                var art = platform.GetComponent<PlatformArtwork>();
                if (art == null)
                {
                    art = platform.gameObject.AddComponent<PlatformArtwork>();
                    var renderer = new GameObject("Gallery Ledge").AddComponent<SpriteRenderer>();
                    renderer.transform.SetParent(platform.transform, false);
                    art.Configure(renderer, ledge.bounds.size.x);
                }
                art.SetGalleryLedge(ledge, depth);
            }
            foreach (var bolt in GetComponentsInChildren<LightningZone>(true))
                MagicSealZone.Replace(bolt);
            var texture = Resources.Load<Texture2D>("CastleGallery");
            panel = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                Vector2.one * .5f, 100f, 0, SpriteMeshType.FullRect);
            for (int i = 0; i < walls.Length; i++)
            {
                var wall = new GameObject("Gallery Wall").AddComponent<SpriteRenderer>();
                wall.transform.SetParent(transform, false);
                wall.sprite = panel;
                wall.color = new Color(.56f, .60f, .72f, 1f);
                wall.sortingOrder = -40;
                walls[i] = wall;
            }
            LateUpdate();
        }

        public static float ConfigureSurface(StageCourse course, Platform platform)
        {
            int index = course.IndexOf(platform);
            var surface = platform.GetComponent<BoxCollider2D>();
            float width = surface.size.x * Mathf.Abs(platform.transform.lossyScale.x);
            bool thick = index == 0 || index == course.JumpCount || width >= 2.4f || index % 3 == 0;
            float depth = thick ? .8f : .28f;
            // Resize downwards only: landing height, coins and route trajectories stay aligned.
            float top = surface.offset.y + surface.size.y * .5f;
            float height = depth / Mathf.Abs(platform.transform.lossyScale.y);
            surface.size = new Vector2(surface.size.x, height);
            surface.offset = new Vector2(surface.offset.x, top - height * .5f);
            return depth;
        }

        private void LateUpdate()
        {
            if (view == null) return;
            var cameraPosition = view.transform.position;
            float height = view.orthographicSize * 2.02f;
            float scale = Mathf.Max(height / panel.bounds.size.y,
                height * view.aspect / panel.bounds.size.x);
            float width = panel.bounds.size.x * scale;
            float travel = cameraPosition.x * .32f;
            int middle = Mathf.RoundToInt(travel / width);
            for (int i = 0; i < walls.Length; i++)
            {
                int bay = middle + i - 1;
                var wall = walls[i];
                wall.flipX = (bay & 1) != 0;
                wall.transform.localScale = Vector3.one * scale;
                wall.transform.position = new Vector3(cameraPosition.x - travel + bay * width,
                    cameraPosition.y, 0);
            }
        }

        private void OnDestroy()
        {
            if (ledge != null) Destroy(ledge);
            if (panel != null) Destroy(panel);
        }
    }
}
