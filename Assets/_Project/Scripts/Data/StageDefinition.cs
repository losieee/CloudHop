using System;
using UnityEngine;

namespace CloudHop
{
    [Serializable]
    public struct StagePlatformLayout
    {
        public Vector2 position;
        [Min(0.6f)] public float width;
        public string section;
    }

    [Serializable]
    public struct StageChoiceRoute
    {
        public int launchIndex;
        public int safeIndex;
        public int rejoinIndex;
        [Min(.5f)] public float arcHeight;
    }

    [CreateAssetMenu(menuName = "Cloud Hop/Stage Definition")]
    public sealed class StageDefinition : ScriptableObject
    {
        public string stageName;
        public string difficulty;
        public Color skyColor;
        public bool followElevation;
        public Color platformColor = Color.white;
        public StagePlatformLayout[] platforms;
        public StageCoinLayout[] coins = new StageCoinLayout[0];
        public StageChoiceRoute[] choiceRoutes = new StageChoiceRoute[0];
    }
}
