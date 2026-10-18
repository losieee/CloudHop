using System;
using UnityEngine;
namespace CloudHop
{
    [Serializable] public struct StageCoinLayout { public Vector2 position; public int value; }
    public sealed class StageCollectibles : MonoBehaviour
    {
        private CloudCoin[] coins;
        public int Collected { get; private set; }
        public int Total { get; private set; }
        private void Awake() => EnsureCreated();
        private void EnsureCreated()
        {
            if (coins != null) return;
            var layouts = GetComponent<StageCourse>().Definition.coins;
            coins = new CloudCoin[layouts.Length];
            for (int i = 0; i < layouts.Length; i++)
            {
                var coin = new GameObject("Cloud Coin " + i).AddComponent<CloudCoin>();
                coin.transform.SetParent(transform, false); coin.transform.position = layouts[i].position;
                int value = Mathf.Max(1, layouts[i].value);
                coin.Configure(this, value, i * .7f); coins[i] = coin; Total += value;
            }
        }
        internal void Collect(int value) => Collected += value;
        public void ResetCollection()
        {
            EnsureCreated(); Collected = 0;
            foreach (var coin in coins) coin.Restore();
        }
    }
}
