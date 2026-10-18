using UnityEngine;
using UnityEngine.EventSystems;

namespace CloudHop
{
    public sealed class TouchJumpButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private ChargeInput input;
        private int? pointer;
        public void Configure(ChargeInput value) => input = value;
        public void OnPointerDown(PointerEventData data)
        {
            if (pointer.HasValue || input == null || !input.isActiveAndEnabled) return;
            pointer = data.pointerId;
            input.BeginTouch();
        }
        public void OnPointerUp(PointerEventData data)
        {
            if (pointer != data.pointerId) return;
            pointer = null;
            if (input != null) input.EndTouch();
        }
        public void Cancel()
        {
            if (!pointer.HasValue) return;
            pointer = null;
            if (input != null) input.ResetGesture();
        }
        private void OnDisable() => Cancel();
        private void OnApplicationFocus(bool focus) { if (!focus) Cancel(); }
        private void OnApplicationPause(bool paused) { if (paused) Cancel(); }
    }
}
