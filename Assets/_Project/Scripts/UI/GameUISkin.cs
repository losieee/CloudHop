using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace CloudHop
{
    public sealed partial class GameUI
    {
        private Image boostBadge;

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
            var strip = Box(hud.transform,"Stage information backdrop",new Vector2(0,308),new Vector2(360,100),new Color(.055f,.12f,.23f,.72f));
            strip.GetComponent<Image>().raycastTarget=false;
            strip.SetAsFirstSibling();
            MinimalLabel(stageLabel,new Vector2(0,340),new Vector2(230,35),22);
            MinimalLabel(runLabel,new Vector2(-415,330),new Vector2(330,44),19);
            runLabel.alignment=TextAlignmentOptions.Left;
            comboLabel.gameObject.SetActive(false);
            comboHint.gameObject.SetActive(false);
            MinimalLabel(boostLabel,new Vector2(0,-270),new Vector2(380,34),20);
            boostBadge=Art(hud.transform,"BoostBadge",new Vector2(-165,-270),new Vector2(32,32));
            MinimalLabel(comboNotice,Vector2.zero,new Vector2(310,36),22);
            progressLabel.gameObject.SetActive(false);
            CompactMeter(progressFill,new Vector2(0,310),new Vector2(220,5));
            CompactMeter(chargeFill,new Vector2(0,-320),new Vector2(280,14));
            var slider = settings.GetComponentInChildren<Slider>(true);
            var track = (RectTransform)slider.transform;
            track.sizeDelta = new Vector2(420, 55);
            var image = track.GetComponent<Image>(); image.sprite = HudArt.Get("CloudMeter"); image.color = Color.white;
            var area = Rect(track, "Volume Fill Area", Vector2.zero, Vector2.zero);
            FitMeterInterior(area);
            slider.fillRect.SetParent(area, false);
            slider.fillRect.sizeDelta = Vector2.zero;
            var fill = slider.fillRect.GetComponent<Image>(); fill.sprite = HudArt.Fill; fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.fillOrigin = 0; fill.fillAmount = volume; fill.color = new Color(.2f, .85f, 1);
            slider.value = volume;
        }
        private void UpdateHudSkin()
        {
            float charge = player.IsCharging ? player.Charge01 : 0;
            chargeFill.color = Color.Lerp(new Color(.18f, .86f, 1), new Color(1, .83f, .22f), Mathf.InverseLerp(.7f, 1, charge));
            bool ready=player.Combo.BoostReady;
            boostBadge.gameObject.SetActive(ready);
            boostLabel.gameObject.SetActive(ready || player.Combo.Count > 0);
            boostBadge.transform.localScale=Vector3.one*(1+.04f*Mathf.Sin(Time.time*5));
            boostLabel.color=new Color(1,.88f,.35f);
        }
        private void MinimalLabel(TMP_Text label,Vector2 position,Vector2 size,int fontSize)
        {
            label.rectTransform.anchoredPosition=position;label.rectTransform.sizeDelta=size;
            label.fontSize=label.fontSizeMax=fontSize;label.fontSizeMin=fontSize*.8f;
            label.color=Color.white;label.outlineWidth=.15f;
        }
        private void CompactMeter(Image fill,Vector2 position,Vector2 size)
        {
            var track=(RectTransform)fill.transform.parent;
            var frameSize=new Vector2(size.x+80,fill==chargeFill?52:34);
            track.anchoredPosition=position;track.sizeDelta=frameSize;
            var background=track.GetComponent<Image>();background.sprite=HudArt.Get("CloudMeter");background.type=Image.Type.Simple;
            background.color=Color.white;
            FitMeterInterior(fill.rectTransform);
        }

        private static void FitMeterInterior(RectTransform interior)
        {
            // Bounds of the well in the cropped 2124 x 276 frame, bottom-left origin.
            interior.anchorMin = new Vector2(257f / 2124f, 60f / 276f);
            interior.anchorMax = new Vector2(1867f / 2124f, 184f / 276f);
            interior.offsetMin = interior.offsetMax = Vector2.zero;
        }
    }
}
