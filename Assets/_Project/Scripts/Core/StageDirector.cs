using System;
using UnityEngine;

namespace CloudHop
{
    // Courses are baked into the scene so the full map can be inspected before Play.
    public sealed class StageDirector : MonoBehaviour
    {
        [SerializeField] private StageCourse[] courses;
        [SerializeField, Range(0, 2)] private int startingStageIndex;
        [SerializeField] private Camera gameCamera;
        public event Action Changed;
        public int StartingStageIndex => startingStageIndex;
        public int CurrentIndex { get; private set; }
        public int Progress { get; private set; }
        public StageCourse CurrentCourse => courses[CurrentIndex];
        public bool IsLastStage => CurrentIndex == courses.Length - 1;
        public int StageCount => courses.Length;

        private Sprite gateSprite;
        public Transform CastleGate { get; private set; }

        private void CreateCastleGate()
        {
            var finish = CurrentCourse.Platforms[CurrentCourse.JumpCount];
            foreach (var art in finish.GetComponentsInChildren<SpriteRenderer>(true))
                if (art.name == "Finish Flag") art.enabled = false;
            var bounds = finish.GetComponent<Collider2D>().bounds;
            var texture = Resources.Load<Texture2D>("CastleGate");
            gateSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(.5f, 0), texture.width / 6f);
            var renderer = new GameObject("Castle Gate").AddComponent<SpriteRenderer>();
            CastleGate = renderer.transform;
            CastleGate.SetParent(CurrentCourse.transform, true);
            CastleGate.position = new Vector3(bounds.center.x, bounds.max.y - .06f, 0);
            renderer.sprite = gateSprite;
            renderer.sortingOrder = -15;
        }

        private void OnDestroy()
        {
            if (gateSprite != null) Destroy(gateSprite);
        }

        public void ActivateStage(int index)
        {
            CurrentIndex = Mathf.Clamp(index, 0, courses.Length - 1);
            for (int i = 0; i < courses.Length; i++) courses[i].gameObject.SetActive(i == CurrentIndex);
            if (CurrentIndex == 1 && CastleGate == null) CreateCastleGate();
            if (CurrentIndex == 2 && !CurrentCourse.TryGetComponent<CastleGallery>(out _))
                CurrentCourse.gameObject.AddComponent<CastleGallery>().Configure(gameCamera);
            gameCamera.backgroundColor = CurrentCourse.Definition.skyColor;
            ResetProgress();
        }
        public void ResetProgress() { Progress = 0; Changed?.Invoke(); }
        public bool RegisterLanding(Platform platform)
        {
            int index = CurrentCourse.IndexOf(platform);
            if (index > Progress) { Progress = index; Changed?.Invoke(); }
            return index == CurrentCourse.JumpCount;
        }
        public string SectionName => CurrentCourse.Definition.platforms[Mathf.Clamp(Progress, 0, CurrentCourse.JumpCount)].section;
    }
}
