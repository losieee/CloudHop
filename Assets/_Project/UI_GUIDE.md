# CLOUD HOP integrated UI
Open Assets/_Project/Scenes/CloudHop.unity and press Play.
The lobby is the entry point. Legacy prototype/test scenes are preserved.

Flow:
- Lobby: Play (nickname and timed three-stage run), Practice (individual stages), Settings, Local TOP 10.
- HUD: live score, best score, stage, progress, charge meter, pause.
- Pause/Escape: Resume, Settings, Lobby.
- Results: Retry selected stage, Lobby, Next Stage (after clear).
- Settings: master volume and mute saved with PlayerPrefs. Existing project has no new music/SFX.
- Returning to lobby abandons the current attempt; starting a stage resets it.
- Menus disable ChargeInput and freeze simulation; resume waits for input release.
- On scene teardown, previous time scale and audio listener volume are restored.

UIAssets contains original imported sprites. GameUI builds a responsive uGUI canvas at 1280x800 reference resolution.
Blank source panels have live text; baked example score/timer digits are not used.
Local time ranking/name entry and character selection are implemented. New sound assets and full UI localization are not implemented.
All original ZIP PNGs are imported; not every decorative variant is used.

Typography:
- Lilita One by Juan Montoreano (SIL OFL 1.1), from https://github.com/google/fonts/tree/main/ofl/lilitaone.
- GameUI uses TextMeshPro SDF text with a bundled, static ASCII atlas. No online font loading.
- UIAssets.uiFont is the Inspector font reference; CLOUD HOP/Install UI Typography can rebuild a missing atlas.
- Buttons use navy text and a subtle shadow. White HUD labels have navy outlines; score digits are larger and gold.
- Current copy is English. Korean/localized copy requires an additional font and glyph atlas before changing labels.
- The license is included in Assets/StreamingAssets/ThirdPartyNotices for Windows/Web builds.

The lobby footer is removed. The charge hint now reads 점프 : 스페이스바 using the bundled Jua OFL font fallback (static glyphs for this phrase). Jua license is included in StreamingAssets/ThirdPartyNotices.

Character selection:
- Lobby PLAY opens Character Selection inside the integrated CloudHop scene, then PLAY starts stage 1.
- Stage selection opens the same character screen for the chosen stage.
- Cloud Adventurer, Sky Explorer (uploaded girl), and Cloud Fox (uploaded fox) share the same player physics and collider.
- Selection persists in PlayerPrefs key CloudHop.Character; retry/next stage use the same skin.
- UIAssets.characters references the three CharacterData assets. No Inspector wiring is required.
- Girl and Fox use five supplied poses: idle, charge, jump, fall, and a 0.16-second landing recovery. See CHARACTER_ANIMATION_GUIDE.md.
- PNG originals are preserved. Sprite pivots align the visible feet; only visual scale/offset changes.

See EXHIBITION_GUIDE.md for nickname entry, timing rules, local storage and exhibition setup.