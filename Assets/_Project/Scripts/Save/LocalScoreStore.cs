using UnityEngine;

namespace CloudHop
{
    // Storage is separate from scoring; a JSON Top 10 repository can be added here later.
    public sealed class LocalScoreStore
    {
        private const string BestKey = "CloudHop.BestScore.v1";
        public int LoadBest() => Mathf.Max(0, PlayerPrefs.GetInt(BestKey, 0));
        public void SaveBest(int score)
        {
            PlayerPrefs.SetInt(BestKey, Mathf.Max(LoadBest(), score));
            PlayerPrefs.Save();
        }
    }
}
