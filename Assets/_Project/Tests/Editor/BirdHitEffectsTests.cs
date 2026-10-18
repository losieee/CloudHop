using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace CloudHop.Tests
{
    public sealed class BirdHitEffectsTests
    {
        [UnityTest] public IEnumerator BirdContactBurstsOnceAndResetClearsEffect()
        {
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/CloudHop.unity");
            yield return new EnterPlayMode(); yield return Run();
        }
        static IEnumerator Run()
        {
            yield return null; yield return null;
            GameObject.Find("Stages").GetComponent<Button>().onClick.Invoke();
            GameObject.Find("Stage 1").GetComponent<Button>().onClick.Invoke();
            GameObject.Find("Character Play").GetComponent<Button>().onClick.Invoke();
            var player = Object.FindFirstObjectByType<PlayerController>();
            var bird = Object.FindObjectsByType<PatrolBird>(FindObjectsSortMode.None).First();
            player.GetComponent<ChargeInput>().enabled = false;
            player.ResetPlayer(bird.transform.position);
            float deadline = Time.realtimeSinceStartup + 3;
            while (player.HitsTaken == 0 && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.AreEqual(1, player.HitsTaken, "Actual bird trigger applies knockback");
            var effect = player.GetComponent<BirdHitEffects>();
            Assert.IsNotNull(effect); Assert.IsTrue(effect.IsPlaying); Assert.AreEqual(1, effect.BurstCount);
            typeof(PatrolBird).GetMethod("Hit", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(bird, new object[] { player.GetComponent<Collider2D>() });
            Assert.AreEqual(1, effect.BurstCount, "Protection prevents repeated bursts");
            float end = Time.time + .1f;
            while (Time.time < end) yield return null;
            Object.FindFirstObjectByType<FollowCamera>().ResetPosition();
            yield return null;
            GameUITests.Capture("bird-impact", 1920, 1080);
            Time.timeScale = 0;
            for (int i = 0; i < 4; i++) yield return null;
            Assert.IsTrue(effect.IsPlaying, "Pause freezes the effect");
            Time.timeScale = 1;
            end = Time.time + .5f;
            while (Time.time < end) yield return null;
            Assert.IsFalse(effect.IsPlaying);
            effect.Play(player.transform.position);
            player.ResetPlayer(Vector3.zero);
            Assert.IsFalse(effect.IsPlaying);
            foreach (var line in effect.GetComponentsInChildren<LineRenderer>())
                if (line.name.StartsWith("Bird impact")) Assert.IsFalse(line.enabled);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        { if (EditorApplication.isPlaying) yield return new ExitPlayMode(); Time.timeScale = 1; }
    }
}
