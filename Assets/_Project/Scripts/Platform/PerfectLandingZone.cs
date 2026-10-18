using UnityEngine;

namespace CloudHop
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class PerfectLandingZone : MonoBehaviour
    {
        public float Fraction { get; private set; } = .5f;
        private BoxCollider2D surface;
        private LineRenderer marker;
        private Material material;
        private SpriteRenderer target;
        private float flash;
        public float HalfWidth => Mathf.Min(.8f, surface.bounds.size.x * Fraction * .5f);
        private void Awake()
        {
            surface = GetComponent<BoxCollider2D>();
            marker = new GameObject("Perfect landing marker").AddComponent<LineRenderer>();
            marker.transform.SetParent(transform, false);
            material = new Material(Shader.Find("Sprites/Default"));
            marker.sharedMaterial = material;
            marker.useWorldSpace = true; marker.positionCount = 4;
            marker.startWidth = marker.endWidth = .065f;
            marker.sortingOrder = 25;
            target = new GameObject("Landing target artwork").AddComponent<SpriteRenderer>();
            target.transform.SetParent(transform, false);
            target.sprite = HudArt.Get("LandingTarget");
            target.sortingOrder = 24;
        }
        public void Configure(float fraction) => Fraction = Mathf.Clamp(fraction, .2f, .7f);
        public bool Contains(float playerCenterX) => Mathf.Abs(playerCenterX - surface.bounds.center.x) <= HalfWidth;
        public void Pulse() => flash = .32f;
        private void LateUpdate()
        {
            marker.enabled = surface.enabled;
            target.enabled = surface.enabled && target.sprite != null;
            var bounds = surface.bounds;
            float x = bounds.center.x, y = bounds.max.y + .05f, half = HalfWidth;
            if (target.sprite != null)
            {
                target.transform.position = new Vector3(x, y + .06f, 0);
                var scale = transform.lossyScale;
                target.transform.localScale = new Vector3(half * 2 / target.sprite.bounds.size.x / Mathf.Abs(scale.x),
                    (.18f + .06f * flash / .32f) / target.sprite.bounds.size.y / Mathf.Abs(scale.y), 1);
                target.color = new Color(1, 1, 1, flash > 0 ? 1 : .8f);
            }
            marker.startWidth = marker.endWidth = flash > 0 ? .065f : .025f;
            marker.SetPosition(0, new Vector3(x-half,y+.09f,0));
            marker.SetPosition(1, new Vector3(x-half,y,0));
            marker.SetPosition(2, new Vector3(x+half,y,0));
            marker.SetPosition(3, new Vector3(x+half,y+.09f,0));
            flash = Mathf.Max(0, flash - Time.deltaTime);
            marker.startColor = marker.endColor = flash > 0 ? new Color(1, .87f, .2f) : new Color(.35f, 1, .97f, .9f);
        }
        private void OnDestroy() { if (material != null) Destroy(material); }
    }
}
