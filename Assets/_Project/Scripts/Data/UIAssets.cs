using UnityEngine;
using TMPro;

namespace CloudHop
{
    [CreateAssetMenu(menuName="Cloud Hop/UI Assets")]
    public sealed class UIAssets : ScriptableObject
    {
        public TMP_FontAsset uiFont;
        public Sprite[] sprites;
        public Sprite character;
        public CharacterData[] characters;
        public Sprite[] stageArt;
        public Sprite Find(string name)
        {
            foreach(var sprite in sprites)if(sprite!=null && sprite.name==name)return sprite;
            return null;
        }
    }
}
