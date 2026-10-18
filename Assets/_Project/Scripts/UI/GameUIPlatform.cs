using UnityEngine;
using UnityEngine.UI;

namespace CloudHop
{
    public sealed partial class GameUI
    {
        private TouchJumpButton touchJump;
        private void BuildPlatformUI()
        {
            if (!PlatformEntry.IsMobile) return;
            var button = Button(hud.transform, "Touch Jump", UIText.Get("mobile.jump_button"), new Vector2(475, -260), new Vector2(245, 125), "UI_Large_Cyan", () => { });
            touchJump = button.gameObject.AddComponent<TouchJumpButton>();
            touchJump.Configure(input);
            foreach (var screen in new[] { lobby, selection, hud, pause, settings, result, characterSelection, nameEntry, rankingScreen })
                screen.AddComponent<MobileSafeArea>().Configure((RectTransform)screen.transform);
        }
        private void UpdatePlatformUI()
        {
            if (touchJump == null) return;
            bool enabled = playing && input.isActiveAndEnabled && !pause.activeSelf && !settings.activeSelf && !result.activeSelf;
            if (touchJump.gameObject.activeSelf != enabled) touchJump.gameObject.SetActive(enabled);
        }
        private void OnApplicationPause(bool paused)
        {
            if (paused && playing && !IsPaused && !settings.activeSelf && !result.activeSelf) Pause();
        }
    }
}
