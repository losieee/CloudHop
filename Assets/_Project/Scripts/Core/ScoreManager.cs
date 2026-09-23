using System;
using System.Collections.Generic;
using UnityEngine;

namespace CloudHop
{
    public sealed class ScoreManager : MonoBehaviour
    {
        public event Action Changed;
        public int Score { get; private set; }
        public int Best { get; private set; }
        private readonly HashSet<Platform> visited = new HashSet<Platform>();
        private readonly LocalScoreStore store = new LocalScoreStore();
        public void ResetScore()
        {
            Score = 0;
            Best = Mathf.Max(Best, store.LoadBest());
            visited.Clear();
            Changed?.Invoke();
        }
        public void RegisterLanding(Platform platform)
        {
            if (platform == null || !visited.Add(platform) || !platform.AwardsScore) return;
            Score++;
            Best = Mathf.Max(Best, Score);
            Changed?.Invoke();
        }
        public void SaveBest() => store.SaveBest(Best);
        private void OnApplicationPause(bool paused) { if (paused) SaveBest(); }
        private void OnApplicationQuit() => SaveBest();
    }
}
