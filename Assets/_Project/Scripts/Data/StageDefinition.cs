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

    [CreateAssetMenu(menuName = "Cloud Hop/Stage Definition")]
    public sealed class StageDefinition : ScriptableObject
    {
        public string stageName;
        public string difficulty;
        public Color skyColor;
        public Color platformColor = Color.white;
        public StagePlatformLayout[] platforms;
    }
}
