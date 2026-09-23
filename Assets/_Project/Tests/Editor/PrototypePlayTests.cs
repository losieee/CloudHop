using System.Collections;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace CloudHop.Tests
{
    public sealed class PrototypePlayTests
    {
        private Keyboard keyboard;
        private Mouse mouse;
        private InputSettings originalInputSettings;
        private InputSettings testInputSettings;
        private const string BestKey = "CloudHop.BestScore.v1";
        private const string BackupExistsKey = "CloudHop.Tests.BestExisted";
        private const string BackupValueKey = "CloudHop.Tests.BestValue";

        // EditMode tests keep their editor coroutine runner after EnterPlayMode.
        // Wait using actual game time instead of player-only yield instructions.
        private static IEnumerator WaitForGameTime(float seconds)
        {
            float end = Time.time + seconds;
            double timeout = UnityEditor.EditorApplication.timeSinceStartup + 15;
            while (Time.time < end)
            {
                Assert.Less(UnityEditor.EditorApplication.timeSinceStartup, timeout, "Play Mode stopped advancing.");
                yield return null;
            }
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // SessionState survives both EnterPlayMode and ExitPlayMode domain reloads.
            UnityEditor.SessionState.SetBool(BackupExistsKey, PlayerPrefs.HasKey(BestKey));
            UnityEditor.SessionState.SetInt(BackupValueKey, PlayerPrefs.GetInt(BestKey));
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/CloudHopPrototype.unity");
            yield return new EnterPlayMode();
            // Batchmode has no focused Game view. Keep synthetic device input in the player.
            originalInputSettings = InputSystem.settings;
            testInputSettings = Object.Instantiate(originalInputSettings);
            testInputSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            testInputSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            InputSystem.settings = testInputSettings;
            keyboard = InputSystem.AddDevice<Keyboard>();
            mouse = InputSystem.AddDevice<Mouse>();
            yield return WaitForGameTime(0.25f);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            if (mouse != null) InputSystem.RemoveDevice(mouse);
            if (originalInputSettings != null) InputSystem.settings = originalInputSettings;
            if (testInputSettings != null) Object.DestroyImmediate(testInputSettings);
            yield return new ExitPlayMode();
            if (UnityEditor.SessionState.GetBool(BackupExistsKey, false))
                PlayerPrefs.SetInt(BestKey, UnityEditor.SessionState.GetInt(BackupValueKey, 0));
            else PlayerPrefs.DeleteKey(BestKey);
            PlayerPrefs.Save();
            UnityEditor.SessionState.EraseBool(BackupExistsKey);
            UnityEditor.SessionState.EraseInt(BackupValueKey);
        }

        [UnityTest]
        public IEnumerator SpaceLandingScoreFallRetryLoop()
        {
            var player = Object.FindFirstObjectByType<PlayerController>();
            var scores = Object.FindFirstObjectByType<ScoreManager>();
            var game = Object.FindFirstObjectByType<GameManager>();
            Assert.IsTrue(player.IsGrounded, "Player must settle on the start platform.");
            Assert.AreEqual(0, scores.Score);
            Assert.AreEqual(GameState.Ready, game.State);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
            yield return WaitForGameTime(0.5f);
            Assert.IsTrue(player.IsCharging);
            Assert.AreEqual(GameState.Playing, game.State);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return WaitForGameTime(0.95f);
            Assert.IsTrue(player.IsGrounded, "Half charge should reach the first scored platform.");
            Assert.AreEqual(1, scores.Score);
            yield return WaitForGameTime(0.2f);
            Assert.AreEqual(1, scores.Score, "Persistent contacts must not award repeated points.");
            var platform = GameObject.Find("Platform 1").GetComponent<Platform>();
            scores.RegisterLanding(platform);
            Assert.AreEqual(1, scores.Score, "Returning to a visited platform must not score again.");
            player.GetComponent<Rigidbody2D>().position = new Vector2(5.7f, 1.1f);
            yield return WaitForGameTime(1.2f);
            Assert.AreEqual(GameState.GameOver, game.State);
            var button = Object.FindFirstObjectByType<Button>();
            Assert.IsNotNull(button, "Game Over panel must expose the Retry button.");
            Assert.IsTrue(GameObject.Find("Result").GetComponent<Text>().text.Contains("SCORE   1"));
            Assert.GreaterOrEqual(new LocalScoreStore().LoadBest(), 1);
            Vector2 retryPosition = RectTransformUtility.WorldToScreenPoint(null, button.transform.position);
            InputSystem.QueueStateEvent(mouse, new MouseState { position = retryPosition });
            yield return WaitForGameTime(0.05f);
            InputSystem.QueueStateEvent(mouse, new MouseState { position = retryPosition, buttons = 1 });
            yield return WaitForGameTime(0.05f);
            InputSystem.QueueStateEvent(mouse, new MouseState { position = retryPosition });
            yield return WaitForGameTime(0.25f);
            Assert.AreEqual(GameState.Ready, game.State);
            Assert.AreEqual(0, scores.Score);
            Assert.IsTrue(player.IsGrounded);
            Assert.Less(Mathf.Abs(player.transform.position.x), 0.01f);
            Assert.IsFalse(button.gameObject.activeInHierarchy);
            Assert.AreEqual(3f, Camera.main.transform.position.x, 0.05f);
            // Verify a second run, not just a visual reset.
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
            yield return WaitForGameTime(0.5f);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return WaitForGameTime(0.95f);
            Assert.AreEqual(1, scores.Score);
        }

        [UnityTest]
        public IEnumerator MouseChargeControlsJumpStrengthAndAirInputIsIgnored()
        {
            var player = Object.FindFirstObjectByType<PlayerController>();
            var game = Object.FindFirstObjectByType<GameManager>();
            var body = player.GetComponent<Rigidbody2D>();
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            yield return WaitForGameTime(0.08f);
            Assert.IsTrue(player.IsCharging);
            InputSystem.QueueStateEvent(mouse, new MouseState());
            yield return WaitForGameTime(0.04f);
            yield return WaitForGameTime(0.04f);
            float shortVelocity = body.linearVelocity.y;
            Assert.Greater(body.linearVelocity.x, 0);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
            yield return null;
            yield return null;
            Assert.IsFalse(player.IsCharging, "Airborne presses cannot charge another jump.");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            game.Retry();
            yield return WaitForGameTime(0.25f);
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            yield return WaitForGameTime(1.2f);
            Assert.AreEqual(1f, player.Charge01, 0.001f);
            InputSystem.QueueStateEvent(mouse, new MouseState());
            yield return WaitForGameTime(0.04f);
            yield return WaitForGameTime(0.04f);
            Assert.Greater(body.linearVelocity.y, shortVelocity + 3f);
        }

        [UnityTest]
        public IEnumerator CombinedInputRequiresAllReleasedAndCancellationDoesNotJump()
        {
            var player = Object.FindFirstObjectByType<PlayerController>();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            yield return WaitForGameTime(0.15f);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return WaitForGameTime(0.1f);
            Assert.IsTrue(player.IsCharging);
            Assert.IsTrue(player.IsGrounded);
            player.GetComponent<ChargeInput>().ResetGesture();
            InputSystem.QueueStateEvent(mouse, new MouseState());
            yield return WaitForGameTime(0.1f);
            Assert.IsFalse(player.IsCharging);
            Assert.IsTrue(player.IsGrounded);
        }
    }
}
