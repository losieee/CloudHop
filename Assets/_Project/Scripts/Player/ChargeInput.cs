using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace CloudHop
{
    // Multiple held controls count as one gesture; release all to jump.
    public sealed class ChargeInput : MonoBehaviour
    {
        public event Action Pressed;
        public event Action Released;
        public event Action Cancelled;
        private bool held;
        private bool waitingForRelease = true;
        private bool mouseAccepted;

        private void Update()
        {
            bool space = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
            bool mouse = Mouse.current != null && Mouse.current.leftButton.isPressed;
            if (waitingForRelease)
            {
                if (!space && !mouse) waitingForRelease = false;
                return;
            }
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                mouseAccepted = EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject();
            if (!mouse) mouseAccepted = false;
            bool next = space || (mouse && mouseAccepted);
            if (next && !held) Pressed?.Invoke();
            if (!next && held) Released?.Invoke();
            held = next;
        }

        public void ResetGesture()
        {
            held = false;
            mouseAccepted = false;
            waitingForRelease = true;
            Cancelled?.Invoke();
        }

        private void OnApplicationFocus(bool focused) { if (!focused) ResetGesture(); }
        private void OnApplicationPause(bool paused) { if (paused) ResetGesture(); }
        private void OnDisable() => ResetGesture();
    }
}
