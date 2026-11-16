using UnityEngine;

namespace CloudHop
{
    // Shared PNG sprite resources for world collectibles and the HUD.
    public static class CloudCoinArt
    {
        private static Sprite gold, silver, crystal;
        public static Sprite Sprite => Load(ref gold, "GoldCoin");
        public static Sprite Silver => Load(ref silver, "SilverCoin");
        public static Sprite Crystal => Load(ref crystal, "Crystal");

        private static Sprite Load(ref Sprite cached, string name)
        {
            if (cached != null) return cached;
            cached = Resources.Load<Sprite>("Collectibles/" + name);
            if (cached == null)
                Debug.LogError("Missing collectible sprite resource: Collectibles/" + name);
            return cached;
        }
    }
}
