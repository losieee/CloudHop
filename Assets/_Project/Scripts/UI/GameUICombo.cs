using TMPro;
using UnityEngine;

namespace CloudHop
{
    public sealed partial class GameUI
    {
        private TMP_Text comboLabel, boostLabel, comboNotice, comboHint;
        private float noticeUntil;
        private readonly TMP_Text[] routeLabels = new TMP_Text[3];
        private readonly Transform[] routePlatforms = new Transform[3];
        private void BuildComboUI()
        {
            comboLabel = Label(hud.transform, "", new Vector2(-430, 250), new Vector2(340, 42), 23, Color.white);
            comboLabel.gameObject.name = "Combo Count";
            boostLabel = Label(hud.transform, "", new Vector2(410, 250), new Vector2(370, 45), 21, Color.white);
            boostLabel.gameObject.name = "Boost Status";
            comboNotice = Label(hud.transform, "", new Vector2(0, 190), new Vector2(650, 48), 28, Color.white);
            comboNotice.gameObject.name = "Combo Feedback";
            comboHint = Label(hud.transform, "", new Vector2(0, -215), new Vector2(1060, 38), 19, Color.white);
            comboHint.gameObject.name = "Combo Hint";
            player.ComboLanded += OnComboLanding;
            player.BoostUsed += OnBoostUsed;
            player.ResetPerformed += ClearComboNotice;
            foreach (var course in FindObjectsByType<StageCourse>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                float fraction = course.Definition.difficulty == "EASY" ? .5f : course.Definition.difficulty == "NORMAL" ? .4f : .32f;
                foreach (var platform in course.Platforms)
                {
                    var zone = platform.GetComponent<PerfectLandingZone>();
                    if (zone == null) zone = platform.gameObject.AddComponent<PerfectLandingZone>();
                    zone.Configure(fraction);
                }
                if (course.Definition.choiceRoutes.Length > 0)
                {
                    var guide = course.GetComponent<ComboShortcutGuide>();
                    if (guide == null) guide = course.gameObject.AddComponent<ComboShortcutGuide>();
                    guide.Configure(course);
                }
            }
            var keys = new[] { "combo.shortcut", "combo.safe_route", "combo.rejoin" };
            for (int i = 0; i < 3; i++)
            {
                routeLabels[i] = Label(hud.transform, UIText.Get(keys[i]), Vector2.zero, new Vector2(250, 32), 16, Color.white);
                routeLabels[i].gameObject.name = "Route Caption " + i;
                routeLabels[i].color = i == 1 ? new Color(.6f, 1, .8f) : new Color(1, .93f, .5f);
            }
        }
        private void UpdateComboUI()
        {
            comboLabel.text = UIText.Get("combo.count", player.Combo.Count);
            boostLabel.text = player.Combo.BoostReady ? UIText.Get("combo.ready") + " ×" + player.Combo.BoostCharges : UIText.Get("combo.progress", player.Combo.Count % ComboChain.Required, ComboChain.Required);
            boostLabel.color = player.Combo.BoostReady ? new Color(1, .87f, .25f) : Color.white;
            comboHint.text = player.Combo.BoostReady ? UIText.Get(PlatformEntry.IsMobile ? "mobile.boost_hint" : "combo.use_hint") : UIText.Get("combo.landing_hint");
            if (Time.time >= noticeUntil) comboNotice.text = "";
            float remaining = Mathf.Max(0, noticeUntil - Time.time);
            comboNotice.transform.localScale = Vector3.one * (1 + .12f * Mathf.Clamp01((remaining - .65f) / .25f));
        }
        private void LateUpdate()
        {
            var camera = Camera.main;
            if (camera == null) return;
            var feedbackPoint=camera.WorldToViewportPoint(player.transform.position+Vector3.up*1.4f);
            var feedbackCanvasPoint=new Vector3((Mathf.Clamp(feedbackPoint.x,.12f,.88f)-.5f)*canvas.rect.width,(Mathf.Clamp(feedbackPoint.y,.2f,.78f)-.5f)*canvas.rect.height,0);
            comboNotice.rectTransform.position=canvas.TransformPoint(feedbackCanvasPoint);
            var course = stages.CurrentCourse;
            float bestDistance = float.PositiveInfinity;
            bool found = false;
            foreach (var route in course.Definition.choiceRoutes)
            {
                var launch = course.Platforms[route.launchIndex].transform;
                var target = course.Platforms[route.rejoinIndex].transform;
                if (player.transform.position.x > target.position.x + 1.5f) continue;
                float distance = Mathf.Abs(player.transform.position.x - launch.position.x);
                if (distance >= bestDistance) continue;
                bestDistance = distance; found = true;
                routePlatforms[0] = launch;
                routePlatforms[1] = course.Platforms[route.safeIndex].transform;
                routePlatforms[2] = target;
            }
            for (int i = 0; i < routeLabels.Length; i++)
            {
                if (routeLabels[i] == null) continue;
                if (!found) { routeLabels[i].gameObject.SetActive(false); continue; }
                var point = camera.WorldToViewportPoint(routePlatforms[i].position + Vector3.up * 1.6f);
                bool visible = (player.Combo.BoostReady || player.IsBoosting) && routePlatforms[i].gameObject.activeInHierarchy && point.z > 0 && point.x > .08f && point.x < .92f && point.y > .25f && point.y < .75f;
                routeLabels[i].gameObject.SetActive(visible);
                var canvasPoint = new Vector3((point.x - .5f) * canvas.rect.width, (point.y - .5f) * canvas.rect.height, 0);
                routeLabels[i].rectTransform.position = canvas.TransformPoint(canvasPoint);
            }
        }
        private void OnComboLanding(ComboLanding result)
        {
            if (result == ComboLanding.None) return;
            comboNotice.text = result == ComboLanding.Perfect ? UIText.Get("combo.perfect", player.Combo.Count) : UIText.Get("combo.broken");
            noticeUntil = Time.time + .9f;
        }
        private void OnBoostUsed() { comboNotice.text = UIText.Get("combo.used"); noticeUntil = Time.time + .9f; }
        private void ClearComboNotice() { if (comboNotice != null) comboNotice.text = ""; noticeUntil = 0; }
        private void DisposeComboUI()
        {
            if (player == null) return;
            player.ComboLanded -= OnComboLanding;
            player.BoostUsed -= OnBoostUsed;
            player.ResetPerformed -= ClearComboNotice;
        }
    }
}
