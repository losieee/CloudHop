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
        public int IndexOf(Platform platform)
        {
            int index = System.Array.IndexOf(platforms, platform);
            if (index >= 0 || platform == null || !platform.transform.IsChildOf(transform)) return index;
            var branch = platform.GetComponent<BranchLanding>();
            return branch != null ? branch.ProgressIndex : -1;
        }
    }
}
