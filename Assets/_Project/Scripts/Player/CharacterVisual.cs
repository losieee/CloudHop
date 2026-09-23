using UnityEngine;

namespace CloudHop
{
    public sealed class CharacterVisual : MonoBehaviour
    {
        [SerializeField] private CharacterData character;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        private void Awake() => Apply(character);
        public void Apply(CharacterData data)
        {
            if (data == null) return;
            character = data;
            spriteRenderer.sprite = data.sprite;
            spriteRenderer.color = data.tint;
            animator.runtimeAnimatorController = data.animatorController;
            animator.enabled = data.animatorController != null;
        }
    }
}
