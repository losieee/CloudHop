using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace CloudHop
{
    public sealed partial class GameUI
    {
        private Image boostBadge, feedbackPanel;
        private readonly Image[] comboPips = new Image[3];
        private Image Art(Transform parent, string key, Vector2 position, Vector2 size)
        {
            var image = Rect(parent, key, position, size).gameObject.AddComponent<Image>();
            image.sprite = HudArt.Get(key); image.raycastTarget = false;
            if (key == "InfoPanel") { image.type = Image.Type.Sliced; image.pixelsPerUnitMultiplier = image.sprite.rect.height / size.y; }
            return image;
        }
        private Image Plate(TMP_Text label, Vector2 position, Vector2 size, int fontSize)
        {
            label.rectTransform.anchoredPosition = position;
            label.rectTransform.sizeDelta = size - new Vector2(55, 10);
            label.fontSize = label.fontSizeMax = fontSize; label.fontSizeMin = fontSize * .8f;
            label.color = Ink; label.outlineWidth = 0;
            label.fontMaterial.SetColor("_UnderlayColor", new Color(0, 0, 0, 0));
            var panel = Art(label.transform.parent, "InfoPanel", position, size);
            panel.transform.SetSiblingIndex(label.transform.GetSiblingIndex());
            return panel;
        }
        private void BuildHudSkin()
        {
            Plate(stageLabel, new Vector2(0, 325), new Vector2(270, 48), 22);
            Plate(runLabel, new Vector2(0, 265), new Vector2(580, 55), 21);
            Plate(comboLabel, new Vector2(-455, 250), new Vector2(260, 53), 22);
            Plate(boostLabel, new Vector2(455, 250), new Vector2(270, 53), 18);
            boostLabel.rectTransform.anchoredPosition += Vector2.right * 16;
            boostLabel.rectTransform.sizeDelta = new Vector2(200, 38);
            boostBadge = Art(hud.transform, "BoostBadge", new Vector2(347, 250), new Vector2(44, 44));
            for (int i = 0; i < 3; i++)
                comboPips[i] = Art(hud.transform, "BoostBadge", new Vector2(-481 + i * 26, 213), new Vector2(21, 21));
            feedbackPanel = Plate(comboNotice, new Vector2(0, 190), new Vector2(450, 58), 25);
            Plate(comboHint, new Vector2(0, -215), new Vector2(760, 46), 18);
            Plate(progressLabel, new Vector2(370, -345), new Vector2(125, 36), 17);
            var slider = settings.GetComponentInChildren<Slider>(true);
            var track = (RectTransform)slider.transform;
            track.sizeDelta = new Vector2(420, 55);
            var image = track.GetComponent<Image>(); image.sprite = HudArt.Get("CloudMeter"); image.color = Color.white;
            var area = Rect(track, "Volume Fill Area", new Vector2(0, 1), new Vector2(309, 20));
            slider.fillRect.SetParent(area, false);
            slider.fillRect.sizeDelta = Vector2.zero;
            var fill = slider.fillRect.GetComponent<Image>(); fill.sprite = HudArt.Fill; fill.color = new Color(.2f, .85f, 1);
            slider.value = volume;
        }
        private void UpdateHudSkin()
        {
            float charge = player.IsCharging ? player.Charge01 : 0;
            chargeFill.color = Color.Lerp(new Color(.18f, .86f, 1), new Color(1, .83f, .22f), Mathf.InverseLerp(.7f, 1, charge));
            boostBadge.color = player.Combo.BoostReady ? Color.white : new Color(.6f, .7f, .8f, .65f);
            boostBadge.transform.localScale = Vector3.one * (player.Combo.BoostReady ? 1 + .06f * Mathf.Sin(Time.time * 5) : 1);
            int pips = player.Combo.BoostReady ? 3 : player.Combo.Count % ComboChain.Required;
            for (int i = 0; i < 3; i++) comboPips[i].color = i < pips ? Color.white : new Color(.55f, .65f, .75f, .55f);
            feedbackPanel.enabled = !string.IsNullOrEmpty(comboNotice.text);
            boostLabel.color = player.Combo.BoostReady ? new Color(.58f, .28f, .015f) : Ink;
        }
    }
}
