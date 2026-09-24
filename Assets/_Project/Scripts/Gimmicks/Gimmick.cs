using UnityEngine;

namespace CloudHop
{
    public abstract class Gimmick : MonoBehaviour
    {
        [SerializeField] protected SpriteRenderer visual;
        [SerializeField] private TextMesh debugLabel;
        protected StageMechanics Clock { get; private set; }
        public bool IsActive => Clock != null && Clock.Enabled && Clock.Running;
        protected float Elapsed => Clock == null ? 0 : Clock.Elapsed;
        protected bool EnabledForCourse => Clock != null && Clock.Enabled;
        public abstract string Status { get; }
        public abstract Color DebugColor { get; }
        protected virtual void Awake() { Clock = GetComponentInParent<StageMechanics>(); }
        public abstract void ResetMechanic();
        protected virtual void LateUpdate()
        {
            if (debugLabel == null) return;
            debugLabel.text = EnabledForCourse ? Status : "GIMMICK OFF";
            debugLabel.color = DebugColor;
        }
    }
}
