using UnityEngine;

namespace CloudHop
{
    public enum CloudPhase { Solid, Crumbling, Gone }

    [RequireComponent(typeof(Platform))]
    public sealed class CrumblingPlatform : Gimmick
    {
        [SerializeField, Min(1.1f)] private float crumbleDelay = 2.2f;
        [SerializeField, Min(0.5f)] private float respawnDelay = 2.5f;
        public CloudPhase Phase { get; private set; }
        private float timer;
        private Collider2D surface;
        private Platform platform;
        public override string Status => Phase == CloudPhase.Solid ? "CRUMBLE - LAND TO START" :
            Phase == CloudPhase.Crumbling ? $"CRUMBLE {timer:F1}s" : $"GONE / RETURN {timer:F1}s";
        public override Color DebugColor => new Color(1f, 0.65f, 0.2f);
        protected override void Awake()
        {
            base.Awake();
            surface = GetComponent<Collider2D>();
            platform = GetComponent<Platform>();
            ResetMechanic();
        }
        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!IsActive || Phase != CloudPhase.Solid) return;
            var player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null && player.IsGrounded && player.GroundPlatform == platform)
            { Phase = CloudPhase.Crumbling; timer = crumbleDelay; }
        }
        private void FixedUpdate()
        {
            if (!IsActive || Phase == CloudPhase.Solid) return;
            timer = Mathf.Max(0, timer - Time.fixedDeltaTime);
            if (timer > 0) return;
            if (Phase == CloudPhase.Crumbling)
            {
                Phase = CloudPhase.Gone;
                timer = respawnDelay;
                surface.enabled = false;
            }
            else ResetMechanic();
        }
        protected override void LateUpdate()
        {
            base.LateUpdate();
            if (visual == null) return;
            Color color = DebugColor;
            color.a = Phase == CloudPhase.Gone ? 0.12f : 1;
            if (Phase == CloudPhase.Crumbling) color = Color.Lerp(Color.red, color, timer / crumbleDelay);
            visual.color = color;
        }
        public override void ResetMechanic()
        {
            Phase = CloudPhase.Solid;
            timer = 0;
            if (surface == null) surface = GetComponent<Collider2D>();
            surface.enabled = true;
        }
    }
}
