using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace CloudHop.Tests
{
    public sealed class HudSkinTests
    {
        [UnityTest] public IEnumerator ChargeArtworkAndLandingTargetsFollowGameplay()
        {
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/CloudHop.unity");
            yield return new EnterPlayMode(); yield return Run();
        }
        static IEnumerator Wait(float seconds)
        {
            float end = Time.time + seconds, deadline = Time.realtimeSinceStartup + 4;
            while (Time.time < end && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.GreaterOrEqual(Time.time, end);
        }
        static IEnumerator Run()
        {
            yield return null; yield return null;
            GameObject.Find("Stages").GetComponent<Button>().onClick.Invoke();
            GameObject.Find("Stage 1").GetComponent<Button>().onClick.Invoke();
            GameObject.Find("Character Play").GetComponent<Button>().onClick.Invoke();
            var player = Object.FindFirstObjectByType<PlayerController>();
            player.GetComponent<ChargeInput>().enabled = false;
            yield return Wait(.2f);
            Assert.IsTrue(player.IsGrounded);
            var meter = GameObject.Find("Charge").GetComponent<Image>();
            Assert.IsNotNull(meter.sprite);
            var fill = meter.transform.Find("Fill").GetComponent<Image>();
            Assert.AreEqual(0, fill.fillAmount);
            player.BeginCharge(); yield return Wait(.55f);
            Assert.That(fill.fillAmount, Is.InRange(.4f, .8f));
            GameUITests.Capture("hud-half-charge", 1920, 1080);
            yield return Wait(.55f);
            Assert.AreEqual(1, fill.fillAmount, .01f);
            Assert.Greater(fill.color.r, .9f);
            GameUITests.Capture("hud-full-charge", 1920, 1080);
            GameUITests.Capture("hud-small-screen", 1024, 768);
            player.CancelCharge(); yield return null; yield return null;
            Assert.AreEqual(0, fill.fillAmount);
            var course = Object.FindFirstObjectByType<StageDirector>().CurrentCourse;
            var zone = course.Platforms[2].GetComponent<PerfectLandingZone>();
            var target = zone.transform.Find("Landing target artwork").GetComponent<SpriteRenderer>();
            Assert.IsNotNull(target.sprite);
            Assert.AreEqual(zone.HalfWidth * 2, target.bounds.size.x, .01f, "Artwork matches the actual landing zone width");
            zone.Pulse(); yield return null;
            Assert.AreEqual(1, target.color.a, .01f);
            zone.GetComponent<BoxCollider2D>().enabled = false;
            yield return null; yield return null;
            Assert.IsFalse(target.enabled, "Disabled platforms hide their marker");
        }
        [UnityTearDown] public IEnumerator Cleanup()
        { if (EditorApplication.isPlaying) yield return new ExitPlayMode(); Time.timeScale = 1; }
    }
}
