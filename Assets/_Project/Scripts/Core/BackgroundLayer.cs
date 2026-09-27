using UnityEngine;

namespace CloudHop
{
    [DefaultExecutionOrder(200)]
    public sealed class BackgroundLayer : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField, Range(0,1)] private float scrollSpeed = 0.2f;
        [SerializeField, Min(1)] private float repeatDistance = 32;
        [SerializeField] private Vector2 offset;
        [SerializeField] private Transform[] tiles;
        public float ScrollSpeed => scrollSpeed;
        public void Configure(Camera camera, float speed, float distance, Vector2 position, Transform[] items)
        {
            targetCamera = camera;
            scrollSpeed = speed;
            repeatDistance = distance;
            offset = position;
            tiles = items;
            Refresh();
        }
        private void LateUpdate() => Refresh();
        public void Refresh()
        {
            if (targetCamera == null || tiles == null) return;
            float x = targetCamera.transform.position.x;
            // Absolute camera position makes Retry/teleports deterministic.
            int center = Mathf.RoundToInt((x * scrollSpeed - offset.x) / repeatDistance);
            for(int i=0;i<tiles.Length;i++)
            {
                int slot = center + i - tiles.Length / 2;
                tiles[i].position = new Vector3(x * (1-scrollSpeed) + slot * repeatDistance + offset.x,
                    targetCamera.transform.position.y + offset.y, 0);
            }
        }
    }
}
