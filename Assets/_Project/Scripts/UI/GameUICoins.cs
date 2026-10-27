using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace CloudHop
{
    public sealed partial class GameUI
    {
        private TMP_Text coinCount, resultCoins;
        private GameObject coinPanel, resultCoinPanel;
        private GameObject CoinPanel(Transform parent, Vector2 position, out TMP_Text count)
        {
            var background = Art(parent, "InfoPanel", position, new Vector2(175, 42));
            var panel = background.rectTransform; panel.name = "Cloud Coin Counter";
            background.raycastTarget = false;
            var icon = Picture(panel, null, new Vector2(-53, 0), new Vector2(34, 34));
            icon.sprite = CloudCoinArt.Sprite;
            count = Label(panel, "", new Vector2(20, 0), new Vector2(105, 34), 20, Ink);
            return panel.gameObject;
        }
        private void BuildCoinUI()
        {
            coinPanel = CoinPanel(hud.transform, new Vector2(430, 330), out coinCount);
            coinPanel.GetComponent<Image>().enabled=false;
            coinCount.color=Color.white;coinCount.outlineWidth=.15f;
            resultCoinPanel = CoinPanel(result.transform, new Vector2(0, -62), out resultCoins);
            resultCoinPanel.transform.localScale = Vector3.one * .85f;
        }
        private void UpdateCoinUI()
        {
            var collection = stages.CurrentCourse.GetComponent<StageCollectibles>();
            bool visible = collection != null && collection.Total > 0;
            coinPanel.SetActive(visible); resultCoinPanel.SetActive(visible);
            if (visible) { coinCount.text=collection.Collected.ToString(); resultCoins.text=UIText.Get("hud.progress",collection.Collected,collection.Total); }
        }
    }
}
