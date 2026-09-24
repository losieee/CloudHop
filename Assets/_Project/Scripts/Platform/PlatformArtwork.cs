using UnityEngine;

namespace CloudHop
{
    // Art is independent of the rectangular gameplay collider.
    [ExecuteAlways]
    public sealed class PlatformArtwork : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer artwork;
        [SerializeField] private float opaqueWidth = 1;
        private SpriteRenderer marker;
        private BoxCollider2D surface;

        public void Configure(SpriteRenderer renderer, float width)
        {
            artwork = renderer;
            opaqueWidth = width;
            Refresh();
        }

        private void LateUpdate() => Refresh();
        private void OnEnable() => Refresh();
        private void OnDisable()
        {
            if (marker != null) marker.forceRenderingOff = false;
        }

        private void Refresh()
        {
            if (artwork == null) return;
            if (marker == null) marker = GetComponent<SpriteRenderer>();
            if (surface == null) surface = GetComponent<BoxCollider2D>();
            if (marker == null || surface == null) return;
            marker.forceRenderingOff = true;
            Vector3 scale = transform.lossyScale;
            float worldWidth = surface.size.x * Mathf.Abs(scale.x);
            float size = worldWidth / Mathf.Max(opaqueWidth, 0.001f);
            artwork.transform.localScale = new Vector3(size / Mathf.Abs(scale.x),
                size / Mathf.Abs(scale.y), 1);
            artwork.transform.localPosition = new Vector3(surface.offset.x,
                surface.offset.y + surface.size.y * 0.5f, 0);
            artwork.sortingLayerID = marker.sortingLayerID;
            artwork.sortingOrder = marker.sortingOrder;
            // Keep crumble warning and disappearance visible without tinting every themed sprite.
            artwork.color = GetComponent<CrumblingPlatform>() != null
                ? Color.Lerp(Color.white, marker.color, 0.35f) : Color.white;
            Color color = artwork.color;
            color.a = marker.color.a;
            artwork.color = color;
            artwork.enabled = marker.enabled;
        }
    }
}
