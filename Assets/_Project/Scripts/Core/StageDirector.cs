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

        public void ActivateStage(int index)
        {
            CurrentIndex = Mathf.Clamp(index, 0, courses.Length - 1);
            for (int i = 0; i < courses.Length; i++) courses[i].gameObject.SetActive(i == CurrentIndex);
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
