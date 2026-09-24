using UnityEngine;

namespace CloudHop
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    [DefaultExecutionOrder(-100)]
    public sealed class PatrolBird : Gimmick
    {
        [SerializeField, Min(0.1f)] private float halfRange = 0.8f;
        [SerializeField, Min(1)] private float period = 4f;
        [SerializeField] private Vector2 knockback = new Vector2(-3.5f, 2.5f);
        private Rigidbody2D body;
        private Vector2 origin;
        public override string Status => "BIRD <->";
        public override Color DebugColor => new Color(1, 0.35f, 0.75f);
        protected override void Awake()
        {
            base.Awake();
            body = GetComponent<Rigidbody2D>();
            origin = body.position;
        }
        private void FixedUpdate()
        {
            if (!IsActive) { body.linearVelocity = Vector2.zero; return; }
            body.MovePosition(origin + Vector2.right * (halfRange * Mathf.Sin(Elapsed * Mathf.PI * 2 / period)));
        }
        private void OnTriggerEnter2D(Collider2D other) => Hit(other);
        private void OnTriggerStay2D(Collider2D other) => Hit(other);
        private void Hit(Collider2D other)
        {
            if (!IsActive) return;
            var player = other.GetComponentInParent<PlayerController>();
            if (player != null && !player.IsGrounded) player.TryKnockback(knockback);
        }
        public override void ResetMechanic()
        {
            // Inactive courses have not necessarily received Awake yet.
            if (body == null) return;
            body.position = origin;
            body.linearVelocity = Vector2.zero;
        }
    }
}
