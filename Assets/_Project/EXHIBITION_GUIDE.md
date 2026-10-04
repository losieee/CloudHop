# CLOUD HOP exhibition time ranking

Open `Scenes/CloudHop.unity` and press Play.

## Visitor flow
- PLAY → enter a nickname → choose a character → PLAY starts the clock and stage 1.
- Nicknames: 1–12 Korean/English letters, digits, spaces, hyphen or underscore. No account or password.
- Clear stages 1, 2 and 3 in order. NEXT STAGE advances after each clear.
- The final landing in stage 3 stops the clock and saves the result automatically.
- Results offer LOCAL RANK, RANKING, NEW RUN (fresh nickname) and LOBBY.
- Lobby LOCAL TOP 10 shows the fastest ten completed attempts. Equal times keep registration order. Names need not be unique.
- PRACTICE allows individual stages and never registers a ranking time.

## Timing rules
The clock uses monotonic real time, not scaled game time. Pause, settings, retry, game-over waiting and the time between stages all count. Retry resets only the current stage. Returning to the lobby abandons an unfinished attempt. Closing the game abandons an unfinished attempt; completed records remain saved. Character selection and nickname entry do not count.

## Local storage / exhibition setup
Records use PlayerPrefs + versioned JSON, with a previous-save backup. There is no server or network dependency. On Windows, records belong to the current Windows user and the game's CompanyName/ProductName. They are not stored beside the executable. Copying the build to a different laptop does not copy existing records. Keep those identifiers stable for exhibition updates. Browser storage belongs to the browser/profile/site and is separate from Windows builds; clearing site data removes it.

Key: `CloudHop.TimeRanking.v1` (backup: `.backup`). Nicknames and times are public on this device's ranking screen. No personal information other than the chosen nickname is requested. This is an offline exhibition leaderboard, not tamper-proof competitive storage.

## Files / Inspector
- `TimedRun`: full-run clock, ordered stage progress, completion state.
- `LocalRankingStore`: nickname validation, JSON save/load and TOP 10 ordering.
- `GameUIRanking`: nickname entry, timer display, result and leaderboard UI.
- Existing GameUI and CharacterData references remain connected; no manual Inspector setup.
- Jua TMP fallback is dynamic so visitor-chosen Hangul names can render offline from the bundled font.

Before the exhibition, smoke-test nickname typing with the laptop's Korean IME, a complete run, a restart of the Windows build and a second visitor. Automated Editor tests simulate landing at the final platform to verify integration; they do not constitute a manual full-course playthrough or a Windows/WebGL build test.
