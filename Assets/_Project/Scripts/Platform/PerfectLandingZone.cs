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
        }
        public void Configure(float fraction) => Fraction = Mathf.Clamp(fraction, .2f, .7f);
        public bool Contains(float playerCenterX) => Mathf.Abs(playerCenterX - surface.bounds.center.x) <= HalfWidth;
        public void Pulse() => flash = .32f;
        private void LateUpdate()
        {
            marker.enabled = surface.enabled;
            var bounds = surface.bounds;
            float x = bounds.center.x, y = bounds.max.y + .05f, half = HalfWidth;
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
