using System;
using UnityEngine;

namespace CloudHop
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(ChargeInput))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Charge (seconds)")]
        [SerializeField, Min(0)] private float minimumChargeTime = 0.05f;
        [SerializeField, Min(0.01f)] private float maximumChargeTime = 1f;
        [Header("Launch velocity (units / second)")]
        [SerializeField, Min(0.1f)] private float minimumJumpSpeed = 5f;
        [SerializeField, Min(0.1f)] private float maximumJumpSpeed = 11f;
        [SerializeField, Min(0.1f)] private float horizontalSpeed = 5f;
        [Header("Landing / fall")]
        [SerializeField, Range(0.1f, 1f)] private float minimumGroundNormal = 0.65f;
        [SerializeField] private float fallThreshold = -6f;

        public event Action ChargeStarted;
        public event Action<Platform> Landed;
        public event Action Fell;
        public bool IsGrounded { get; private set; }
        public bool IsCharging { get; private set; }
        public float Charge01 => IsCharging ? Mathf.InverseLerp(minimumChargeTime, maximumChargeTime, chargeTime) : 0f;
        private Rigidbody2D body;
        private ChargeInput input;
        private readonly ContactPoint2D[] contacts = new ContactPoint2D[16];
        private float chargeTime;
        private float queuedCharge;
        private bool jumpQueued;
        private bool playable;
        private bool awaitingSeparation;
        private Platform ground;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            input = GetComponent<ChargeInput>();
        }
        private void OnEnable()
        {
            input.Pressed += BeginCharge;
            input.Released += ReleaseCharge;
            input.Cancelled += CancelCharge;
        }
        private void OnDisable()
        {
            input.Pressed -= BeginCharge;
            input.Released -= ReleaseCharge;
            input.Cancelled -= CancelCharge;
        }
        private void Update()
        {
            if (!playable) return;
            if (IsCharging) chargeTime = Mathf.Min(maximumChargeTime, chargeTime + Time.deltaTime);
            if (body.position.y < fallThreshold)
            {
                SetPlayable(false);
                Fell?.Invoke();
            }
        }
        public void BeginCharge()
        {
            if (!playable || !IsGrounded || jumpQueued || IsCharging) return;
            chargeTime = 0f;
            IsCharging = true;
            ChargeStarted?.Invoke();
        }
        public void ReleaseCharge()
        {
            if (!playable || !IsCharging) return;
            queuedCharge = Charge01;
            IsCharging = false;
            jumpQueued = true;
        }
        public void CancelCharge() { IsCharging = false; chargeTime = 0f; jumpQueued = false; }

        private void FixedUpdate()
        {
            if (!playable) return;
            Platform support = FindSupport();
            // Ignore the launch contact until the player has actually left the surface.
            if (awaitingSeparation)
            {
                if (support == null) awaitingSeparation = false;
                support = null;
            }
            bool wasGrounded = IsGrounded;
            IsGrounded = support != null;
            if (IsGrounded && (!wasGrounded || support != ground))
            {
                body.linearVelocity = Vector2.zero;
                Landed?.Invoke(support);
            }
            ground = support;
            if (!IsGrounded && IsCharging) CancelCharge();
            if (jumpQueued)
            {
                jumpQueued = false;
                if (!IsGrounded) return;
                body.linearVelocity = new Vector2(horizontalSpeed, Mathf.Lerp(minimumJumpSpeed, maximumJumpSpeed, queuedCharge));
                IsGrounded = false;
                ground = null;
                awaitingSeparation = true;
            }
            else if (IsGrounded) body.linearVelocity = Vector2.zero;
        }
        private Platform FindSupport()
        {
            if (body.linearVelocity.y > 0.1f) return null;
            int count = body.GetContacts(contacts);
            for (int i = 0; i < count; i++)
            {
                if (contacts[i].normal.y < minimumGroundNormal) continue;
                var platform = contacts[i].collider.GetComponentInParent<Platform>();
                if (platform != null) return platform;
            }
            return null;
        }
        public void ResetPlayer(Vector3 position)
        {
            body.simulated = false;
            transform.position = position;
            body.position = position;
            body.rotation = 0;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0;
            ground = null;
            IsGrounded = false;
            awaitingSeparation = false;
            CancelCharge();
            input.ResetGesture();
            playable = true;
            body.simulated = true;
        }
        public void SetPlayable(bool value)
        {
            playable = value;
            if (!value)
            {
                CancelCharge();
                body.linearVelocity = Vector2.zero;
                body.simulated = false;
            }
        }
        private void OnValidate()
        {
            maximumChargeTime = Mathf.Max(minimumChargeTime + 0.01f, maximumChargeTime);
            maximumJumpSpeed = Mathf.Max(minimumJumpSpeed, maximumJumpSpeed);
        }
    }
}
