using UnityEngine;

namespace CloudHop
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class LightningZone : PeriodicGimmick
    {
        [SerializeField] private Vector2 knockback = new Vector2(-4.5f, 3f);
        public override string Status => $"BOLT {Phase.ToString().ToUpper()} {Remaining:F1}s";
        public override Color DebugColor => Phase == HazardPhase.Active ? new Color(1f, 0.25f, 0.15f) : new Color(1f, 0.9f, 0.25f);
        private void OnTriggerEnter2D(Collider2D other) => Hit(other);
        private void OnTriggerStay2D(Collider2D other) => Hit(other);
        private void Hit(Collider2D other)
        {
            if (!IsActive || Phase != HazardPhase.Active) return;
            var player = other.GetComponentInParent<PlayerController>();
            if (player != null && !player.IsGrounded) player.TryKnockback(knockback);
        }
    }
}
