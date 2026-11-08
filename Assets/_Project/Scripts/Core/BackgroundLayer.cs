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
        [SerializeField] private bool stageOneCastle;
        private StageDirector stages;
        private Vector3 originalScale;

        private void Awake()
        {
            originalScale = transform.localScale;
            if (stageOneCastle) stages = FindFirstObjectByType<StageDirector>();
        }

        public void SetStageOneCastle() => stageOneCastle = true;
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
            int stage = stages != null && stages.CurrentCourse.gameObject.activeInHierarchy
                ? stages.CurrentIndex : -1;
            float speed = scrollSpeed;
            Vector2 position = offset;
            float scale = 1f;
            if (stage == 0)
            {
                speed = 0.018f;
                position = new Vector2(4.8f, 2.1f);
                scale = 0.43f;
            }
            else if (stage >= 1)
            {
                var course = stages.CurrentCourse;
                float progress = Mathf.InverseLerp(course.SpawnPosition.x,
                    course.Platforms[course.JumpCount].transform.position.x, x);
                // Keep one landmark in view across the whole approach to the gate.
                speed = 0f;
                position = new Vector2(Mathf.Lerp(4.5f, 0.5f, progress), 0.25f);
                scale = stage == 1 ? Mathf.Lerp(5.8f, 6.6f, progress) : Mathf.Lerp(2.1f, 2.7f, progress);
            }
            if (stageOneCastle && Application.isPlaying)
                transform.localScale = originalScale * scale;
            // Absolute camera position makes Retry/teleports deterministic.
            int center = Mathf.RoundToInt((x * speed - position.x) / repeatDistance);
            for(int i=0;i<tiles.Length;i++)
            {
                int slot = center + i - tiles.Length / 2;
                tiles[i].position = new Vector3(x * (1-speed) + slot * repeatDistance + position.x,
                    targetCamera.transform.position.y + position.y, 0);
            }
        }
    }
}
