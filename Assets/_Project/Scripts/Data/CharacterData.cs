using UnityEngine;

namespace CloudHop
{
    [CreateAssetMenu(menuName = "Cloud Hop/Character Skin")]
    public sealed class CharacterData : ScriptableObject
    {
        public string displayName = "Prototype";
        public Sprite sprite;
        public Sprite chargeSprite;
        public Sprite jumpSprite;
        public Sprite fallSprite;
        [Tooltip("Apply skin-specific visual transform without changing the player collider.")]
        public bool overrideVisualTransform;
        public Vector3 visualScale = Vector3.one;
        public Vector3 visualOffset;
        public RuntimeAnimatorController animatorController;
        public Color tint = Color.white;
    }
}
