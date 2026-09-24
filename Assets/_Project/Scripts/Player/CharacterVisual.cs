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
        }

        private void LateUpdate()
        {
            if (character == null || player == null || body == null ||
                character.animatorController != null || !body.simulated) return;
            Sprite pose = character.sprite;
            if (player.IsCharging) pose = character.chargeSprite;
            else if (!player.IsGrounded)
                pose = body.linearVelocity.y < -0.1f ? character.fallSprite : character.jumpSprite;
            spriteRenderer.sprite = pose != null ? pose : character.sprite;
        }
    }
}