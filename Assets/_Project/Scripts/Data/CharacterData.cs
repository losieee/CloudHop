using UnityEngine;

namespace CloudHop
{
    [CreateAssetMenu(menuName = "Cloud Hop/Character Skin")]
    public sealed class CharacterData : ScriptableObject
    {
        public string displayName = "Prototype";
        public Sprite sprite;
        public RuntimeAnimatorController animatorController;
        public Color tint = Color.white;
    }
}
