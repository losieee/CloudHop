using System.Collections;
using CloudHop.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CloudHop.Tests
{
    public sealed class PlatformTests
    {
        [Test] public void BuildsHaveSeparateEntriesAndSharedGameplay()
        {
            CollectionAssert.AreEqual(new[] { PlatformBuilds.PCScene, PlatformBuilds.SharedScene }, PlatformBuilds.ScenesFor(GamePlatform.PC));
            CollectionAssert.AreEqual(new[] { PlatformBuilds.AndroidScene, PlatformBuilds.SharedScene }, PlatformBuilds.ScenesFor(GamePlatform.Android));
        }
        [UnityTest] public IEnumerator AndroidEntrySupportsTouchAndPause()
        {
            EditorSceneManager.OpenScene(PlatformBuilds.AndroidScene);
            yield return new EnterPlayMode();
            yield return AndroidRun();
        }
        private static IEnumerator WaitForGame()
        {
            float deadline = Time.realtimeSinceStartup + 15;
            while ((SceneManager.GetActiveScene().name != "CloudHop" || Object.FindFirstObjectByType<GameUI>() == null) && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.AreEqual("CloudHop", SceneManager.GetActiveScene().name);
            yield return null; yield return null;
        }
        private static void Click(string name) => GameObject.Find(name).GetComponent<Button>().onClick.Invoke();
        private static IEnumerator AndroidRun()
        {
            yield return WaitForGame();
            Assert.IsTrue(PlatformEntry.IsMobile);
            var ui = Object.FindFirstObjectByType<GameUI>();
            Assert.IsTrue(ui.IsLobby);
            GameUITests.Capture("android-lobby-phone", 2340, 1080);
            Click("Stages"); Click("Stage 1"); Click("Character Play");
            var player = Object.FindFirstObjectByType<PlayerController>();
            float deadline = Time.realtimeSinceStartup + 5;
            while (!player.IsGrounded && Time.realtimeSinceStartup < deadline) yield return null;
            yield return null; yield return null;
            Assert.IsTrue(player.IsGrounded);
            var touch = Object.FindFirstObjectByType<TouchJumpButton>();
            Assert.IsNotNull(touch);
            var first = new PointerEventData(EventSystem.current) { pointerId = 11 };
            var second = new PointerEventData(EventSystem.current) { pointerId = 12 };
            touch.OnPointerDown(first);
            Assert.IsTrue(player.IsCharging);
            touch.OnPointerDown(second); touch.OnPointerUp(second);
            Assert.IsTrue(player.IsCharging, "Another finger cannot release the held jump");
            float end = Time.time + .5f;
            while (Time.time < end && Time.realtimeSinceStartup < deadline) yield return null;
            GameUITests.Capture("android-charge-phone", 2340, 1080);
            GameUITests.Capture("android-charge-tablet", 1600, 1200);
            touch.OnPointerUp(first);
            Assert.IsFalse(player.IsCharging);
            for (int i = 0; i < 5; i++) yield return null;
            Assert.Greater(player.GetComponent<Rigidbody2D>().linearVelocity.y, 0);
            player.Combo.Land(100, true); player.Combo.Land(101, true); player.Combo.Land(102, true);
            touch.OnPointerDown(first);
            Assert.IsTrue(player.IsBoosting);
            touch.OnPointerUp(first);
            Click("Pause"); yield return null; yield return null;
            Assert.IsTrue(ui.IsPaused);
            Assert.IsFalse(touch.gameObject.activeInHierarchy);
            GameUITests.Capture("android-pause-phone", 2340, 1080);
            var safeContent = touch.transform.parent;
            Assert.Greater(safeContent.localScale.x, .8f, "Changing aspect ratio must not shrink mobile UI twice");
            Click("Resume"); yield return null; yield return null;
            Assert.IsTrue(touch.gameObject.activeInHierarchy);
            touch.OnPointerDown(first);
            ui.SendMessage("OnApplicationPause", true);
            yield return null; yield return null;
            Assert.IsTrue(ui.IsPaused);
            Assert.IsFalse(player.IsCharging);
            Click("Resume"); yield return null; yield return null;
            touch.OnPointerUp(first);
            Assert.IsFalse(player.IsCharging, "Old touch does not survive pause");
        }
        [UnityTest] public IEnumerator PCEntryKeepsDesktopUI()
        {
            EditorSceneManager.OpenScene(PlatformBuilds.PCScene);
            yield return new EnterPlayMode();
            yield return WaitForGame();
            Assert.IsFalse(PlatformEntry.IsMobile);
            Assert.IsNull(Object.FindFirstObjectByType<TouchJumpButton>(FindObjectsInactive.Include));
            Assert.IsTrue(Object.FindFirstObjectByType<GameUI>().IsLobby);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if (EditorApplication.isPlaying) yield return new ExitPlayMode();
            Time.timeScale = 1;
        }
    }
}
