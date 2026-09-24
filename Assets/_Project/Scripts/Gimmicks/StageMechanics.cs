using UnityEngine;

namespace CloudHop
{
    [DefaultExecutionOrder(-200)]
    public sealed class StageMechanics : MonoBehaviour
    {
        [SerializeField] private GameManager game;
        [SerializeField] private bool gimmicksEnabled = true;
        public float Elapsed { get; private set; }
        public bool Enabled => gimmicksEnabled;
        public bool Running => game != null && (game.State == GameState.Ready || game.State == GameState.Playing);
        private void FixedUpdate() { if (Running && gimmicksEnabled) Elapsed += Time.fixedDeltaTime; }
        public void ResetCourse()
        {
            Elapsed = 0;
            foreach (var gimmick in GetComponentsInChildren<Gimmick>(true)) gimmick.ResetMechanic();
        }
        public void SetEnabled(bool enabled) { gimmicksEnabled = enabled; ResetCourse(); }
    }
}
