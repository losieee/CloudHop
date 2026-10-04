using UnityEngine;

namespace CloudHop
{
    public sealed class CharacterVisual : MonoBehaviour
    {
        [SerializeField] private CharacterData character;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        private PlayerController player;
        private Rigidbody2D body;
        private Vector3 originalScale;
        private Vector3 originalPosition;
        private bool initialized;
        private bool touchedGround;
        private bool airborne;
        private float landingUntil;

        private void OnEnable()
        {
            Initialize();
            if (player == null) return;
            player.Landed += OnLanded;
            player.ResetPerformed += ResetPose;
            ResetPose();
        }

        private void OnDisable()
        {
            if (player == null) return;
            player.Landed -= OnLanded;
            player.ResetPerformed -= ResetPose;
        }

        private void ResetPose()
        {
            touchedGround = false;
            airborne = false;
            landingUntil = 0;
            if (character != null && spriteRenderer != null) spriteRenderer.sprite = character.sprite;
        }

        private void OnLanded(Platform platform)
        {
            // Spawn/reset contacts are not landings from a jump.
            if (touchedGround && airborne && character != null && character.animatorController == null)
            {
                landingUntil = Time.time + character.landingPoseDuration;
                if (character.landingSprite != null && character.landingPoseDuration > 0)
                    spriteRenderer.sprite = character.landingSprite;
            }
            touchedGround = true;
            airborne = false;
        }

        private void Awake()
        {
            Initialize();
            Apply(character);
        }

        private void Initialize()
        {
            if (initialized) return;
            initialized = true;
            originalScale = transform.localScale;
            originalPosition = transform.localPosition;
            player = GetComponentInParent<PlayerController>();
            body = GetComponentInParent<Rigidbody2D>();
        }

        public void Apply(CharacterData data)
        {
            if (data == null) return;
            Initialize();
            character = data;
            spriteRenderer.sprite = data.sprite;
            spriteRenderer.color = data.tint;
            transform.localScale = data.overrideVisualTransform ? data.visualScale : originalScale;
            transform.localPosition = data.overrideVisualTransform ? data.visualOffset : originalPosition;
            if (animator != null)
            {
                animator.runtimeAnimatorController = data.animatorController;
                animator.enabled = data.animatorController != null;
            }
            ResetPose();
        }

        private void LateUpdate()
        {
            if (character == null || player == null || body == null ||
                character.animatorController != null || !body.simulated) return;
            Sprite pose = character.sprite;
            if (player.IsCharging)
            {
                landingUntil = 0;
                pose = character.chargeSprite;
            }
            else if (!player.IsGrounded)
            {
                airborne = true;
                landingUntil = 0;
                pose = body.linearVelocity.y < -0.1f ? character.fallSprite : character.jumpSprite;
            }
            else if (Time.time < landingUntil) pose = character.landingSprite;
            spriteRenderer.sprite = pose != null ? pose : character.sprite;
        }
    }
}
