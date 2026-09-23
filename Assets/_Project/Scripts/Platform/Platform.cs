using UnityEngine;

namespace CloudHop
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class Platform : MonoBehaviour
    {
        [SerializeField] private bool awardsScore = true;
        public bool AwardsScore => awardsScore;
    }
}
