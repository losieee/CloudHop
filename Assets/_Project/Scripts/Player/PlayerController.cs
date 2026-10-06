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
        [Header("Hazards")]
        [SerializeField, Min(0.1f)] private float hitProtectionTime = 1f;

        public event Action ChargeStarted;
        public event Action<Platform> Landed;
        public event Action Fell;
        public event Action ResetPerformed;
        public event Action<ComboLanding> ComboLanded;
        public event Action BoostUsed;
        public ComboChain Combo { get; } = new ComboChain();
        public bool IsBoosting => boostRemaining > 0;
        [Header("Combo boost")]
        [SerializeField, Min(0.05f)] private float boostDuration = .24f;
        [SerializeField, Min(5)] private float boostSpeed = 14f;
        private float boostRemaining;
        private bool comboJump;
        private bool boostedThisJump;
        public bool IsGrounded { get; private set; }
        public bool IsCharging { get; private set; }
        public Platform GroundPlatform => ground;
        public float HitProtectionRemaining { get; private set; }
        public int HitsTaken { get; private set; }
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
            if (GetComponent<ComboEffects>() == null) gameObject.AddComponent<ComboEffects>();
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
            if (!playable || Time.timeScale <= 0) return;
            if (!IsGrounded) { TryUseBoost(); return; }
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

        public bool TryUseBoost()
        {
            if (!playable || Time.timeScale <= 0 || IsGrounded || jumpQueued || boostedThisJump || !comboJump) return false;
            if (!Combo.ConsumeBoost()) return false;
            boostedThisJump = true;
            boostRemaining = boostDuration;
            body.linearVelocity = new Vector2(boostSpeed, Mathf.Max(body.linearVelocity.y, 2.4f));
            BoostUsed?.Invoke();
            return true;
        }

        private void FixedUpdate()
        {
            if (!playable) return;
            if (boostRemaining > 0)
            {
                boostRemaining = Mathf.Max(0, boostRemaining - Time.fixedDeltaTime);
                if (boostRemaining == 0 && !IsGrounded)
                    body.linearVelocity = new Vector2(horizontalSpeed, body.linearVelocity.y);
            }
            HitProtectionRemaining = Mathf.Max(0, HitProtectionRemaining - Time.fixedDeltaTime);
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
                boostRemaining = 0;
                var zone = support.GetComponent<PerfectLandingZone>();
                if (comboJump && zone != null)
                {
                    var feedback = Combo.Land(support.GetInstanceID(), zone.Contains(body.position.x));
                    if (feedback == ComboLanding.Perfect) zone.Pulse();
                    ComboLanded?.Invoke(feedback);
                }
                else Combo.Remember(support.GetInstanceID());
                comboJump = false;
                boostedThisJump = false;
                body.linearVelocity = SupportVelocity(support);
                Landed?.Invoke(support);
                if (!playable) return;
            }
            ground = support;
            if (!IsGrounded && IsCharging) CancelCharge();
            if (jumpQueued)
            {
                jumpQueued = false;
                if (!IsGrounded) return;
                body.linearVelocity = new Vector2(horizontalSpeed, Mathf.Lerp(minimumJumpSpeed, maximumJumpSpeed, queuedCharge));
                comboJump = true;
                boostedThisJump = false;
                IsGrounded = false;
                ground = null;
                awaitingSeparation = true;
            }
            else if (IsGrounded) body.linearVelocity = SupportVelocity(support);
        }
        private static Vector2 SupportVelocity(Platform platform)
        {
            if (platform != null && platform.TryGetComponent<MovingPlatform>(out var moving)) return moving.Velocity;
            return Vector2.zero;
        }
        private Platform FindSupport()
        {
            int count = body.GetContacts(contacts);
            for (int i = 0; i < count; i++)
            {
                if (contacts[i].normal.y < minimumGroundNormal) continue;
                var platform = contacts[i].collider.GetComponentInParent<Platform>();
                if (platform != null && body.linearVelocity.y - SupportVelocity(platform).y <= 0.2f) return platform;
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
            boostRemaining = 0;
            comboJump = false;
            boostedThisJump = false;
            Combo.Reset();
            HitProtectionRemaining = 0;
            HitsTaken = 0;
            CancelCharge();
            input.ResetGesture();
            playable = true;
            body.simulated = true;
            ResetPerformed?.Invoke();
        }
        public void SetPlayable(bool value)
        {
            playable = value;
            if (!value)
            {
                boostRemaining = 0;
                CancelCharge();
                body.linearVelocity = Vector2.zero;
                body.simulated = false;
            }
        }
        public bool TryKnockback(Vector2 velocity)
        {
            if (!playable || HitProtectionRemaining > 0) return false;
            boostRemaining = 0;
            comboJump = false;
            ComboLanded?.Invoke(Combo.Break());
            CancelCharge();
            IsGrounded = false;
            ground = null;
            awaitingSeparation = true;
            body.linearVelocity = velocity;
            HitProtectionRemaining = hitProtectionTime;
            HitsTaken++;
            return true;
        }
        // Wind supports the player even during hit protection; horizontal momentum is preserved.
        public bool ApplyUpdraft(float liftSpeed)
        {
            if (!playable) return false;
            CancelCharge();
            IsGrounded = false;
            ground = null;
            awaitingSeparation = true;
            Vector2 velocity = body.linearVelocity;
            velocity.y = Mathf.Max(velocity.y, liftSpeed);
            body.linearVelocity = velocity;
            return true;
        }
        private void OnValidate()
        {
            maximumChargeTime = Mathf.Max(minimumChargeTime + 0.01f, maximumChargeTime);
            maximumJumpSpeed = Mathf.Max(minimumJumpSpeed, maximumJumpSpeed);
        }
    }
}
