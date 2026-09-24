using UnityEngine;

namespace CloudHop
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Platform), typeof(Rigidbody2D))]
    public sealed class MovingPlatform : Gimmick
    {
        [SerializeField, Range(0.05f, 1f)] private float amplitude = 0.3f;
        [SerializeField, Min(2f)] private float period = 4f;
        private Rigidbody2D body;
        private Vector2 origin;
        public Vector2 Velocity { get; private set; }
        public override string Status => $"MOVE {(Velocity.y >= 0 ? "UP" : "DOWN")} +/-{amplitude:F2}";
        public override Color DebugColor => new Color(0.3f, 1f, 0.4f);
        protected override void Awake()
        {
            base.Awake();
            body = GetComponent<Rigidbody2D>();
            origin = body.position;
        }
        private void FixedUpdate()
        {
            if (!IsActive) { Velocity = Vector2.zero; body.linearVelocity = Vector2.zero; return; }
            Vector2 next = origin + Vector2.up * (amplitude * Mathf.Sin(Elapsed * Mathf.PI * 2 / period));
            Velocity = (next - body.position) / Time.fixedDeltaTime;
            body.MovePosition(next);
        }
        public override void ResetMechanic()
        {
            Velocity = Vector2.zero;
            if (body == null) return;
            body.position = origin;
            body.linearVelocity = Vector2.zero;
        }
    }
}
