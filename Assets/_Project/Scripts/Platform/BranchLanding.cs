using UnityEngine;
namespace CloudHop
{
    public sealed class BranchLanding : MonoBehaviour
    {
        [SerializeField] private int progressIndex;
        public int ProgressIndex => progressIndex;
        public void Configure(int value) => progressIndex = value;
    }
}
