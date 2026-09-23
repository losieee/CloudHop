using UnityEngine;

namespace CloudHop
{
    public sealed class StageCourse : MonoBehaviour
    {
        [SerializeField] private StageDefinition definition;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Platform[] platforms;
        public StageDefinition Definition => definition;
        public Vector3 SpawnPosition => spawnPoint.position;
        public Platform[] Platforms => platforms;
        public int JumpCount => platforms.Length - 1;
        public int IndexOf(Platform platform) => System.Array.IndexOf(platforms, platform);
    }
}
