using UnityEngine;

namespace CloudHop
{
    public enum HazardPhase { Safe, Warning, Active }

    public abstract class PeriodicGimmick : Gimmick
    {
        [SerializeField, Min(0.1f)] private float safeDuration = 3.5f;
        [SerializeField, Min(0.2f)] private float warningDuration = 1.2f;
        [SerializeField, Min(0.1f)] private float activeDuration = 1.2f;
        [SerializeField, Min(0)] private float phaseOffset;
        private float CycleTime => Mathf.Repeat(Elapsed + phaseOffset, safeDuration + warningDuration + activeDuration);
        public HazardPhase Phase => CycleTime < safeDuration ? HazardPhase.Safe :
            CycleTime < safeDuration + warningDuration ? HazardPhase.Warning : HazardPhase.Active;
        public float Remaining => Phase == HazardPhase.Safe ? safeDuration - CycleTime :
            Phase == HazardPhase.Warning ? safeDuration + warningDuration - CycleTime :
            safeDuration + warningDuration + activeDuration - CycleTime;
        public override void ResetMechanic() { }
        protected override void LateUpdate()
        {
            base.LateUpdate();
            if (visual != null)
            {
                Color color = DebugColor;
                color.a = !EnabledForCourse ? 0.08f : Phase == HazardPhase.Active ? 0.65f : Phase == HazardPhase.Warning ? 0.3f : 0.12f;
                visual.color = color;
            }
        }
    }
}
