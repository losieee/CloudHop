using UnityEngine;

namespace CloudHop
{
    // Only content moves into the safe area. The screen's dimming Image stays full screen.
    public sealed class MobileSafeArea : MonoBehaviour
    {
        private RectTransform root;
        private RectTransform content;
        public void Configure(RectTransform screen)
        {
            root = screen;
            content = new GameObject("Safe Area Content", typeof(RectTransform)).GetComponent<RectTransform>();
            content.SetParent(root, false);
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i);
                if (child != content) child.SetParent(content, false);
            }
            // Preserve original rendering order after collecting children in reverse.
            for (int i = content.childCount - 1; i >= 0; i--) content.GetChild(i).SetAsLastSibling();
            Apply();
        }
        private void LateUpdate() => Apply();
        private void Apply()
        {
            if (content == null || Screen.width <= 0 || Screen.height <= 0) return;
            var safe = Screen.safeArea;
            var size = root.rect.size;
            var fraction = new Vector2(safe.width / Screen.width, safe.height / Screen.height);
            content.anchorMin = content.anchorMax = new Vector2(.5f, .5f);
            content.sizeDelta = size;
            content.anchoredPosition = new Vector2((safe.center.x / Screen.width - .5f) * size.x, (safe.center.y / Screen.height - .5f) * size.y);
            // CanvasScaler already fits the reference resolution. Apply only the safe-area inset.
            float scale = Mathf.Clamp01(Mathf.Min(fraction.x, fraction.y));
            content.localScale = Vector3.one * scale;
        }
    }
}
