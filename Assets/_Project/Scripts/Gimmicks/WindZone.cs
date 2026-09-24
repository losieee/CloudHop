using UnityEngine;

namespace CloudHop
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class WindZone : Gimmick
    {
        [SerializeField, Min(0.1f)] private float activeDuration = 5f;
        [SerializeField, Min(0.1f)] private float inactiveDuration = 5f;
        [SerializeField, Min(0.1f)] private float liftSpeed = 7f;
        private float lastAppliedTime = float.NegativeInfinity;
        private const float FeedbackDuration = 0.15f;

        private float CycleTime => Mathf.Repeat(Elapsed, activeDuration + inactiveDuration);
        public bool IsBlowing => IsActive && CycleTime < activeDuration;
        public bool IsPushing => IsBlowing && Time.time - lastAppliedTime < FeedbackDuration;
        public float Remaining => CycleTime < activeDuration
            ? activeDuration - CycleTime : activeDuration + inactiveDuration - CycleTime;
        public override string Status => $"WIND UP ^ {(IsBlowing ? IsPushing ? "LIFTING" : "ON" : "OFF")} {Remaining:F1}s";
        public override Color DebugColor => IsPushing ? Color.white : new Color(0.2f, 0.9f, 1f);

        public override void ResetMechanic()
        {
            lastAppliedTime = float.NegativeInfinity;
        }

        private void OnTriggerEnter2D(Collider2D other) => Lift(other);
        private void OnTriggerStay2D(Collider2D other) => Lift(other);

        private void Lift(Collider2D other)
        {
            if (!IsBlowing) return;
            var player = other.GetComponentInParent<PlayerController>();
            if (player != null && player.ApplyUpdraft(liftSpeed))
                lastAppliedTime = Time.time;
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            if (visual == null) return;
            Color color = DebugColor;
            color.a = !EnabledForCourse ? 0.08f : IsPushing ? 0.65f : IsBlowing ? 0.35f : 0.08f;
            visual.color = color;
        }
    }
}