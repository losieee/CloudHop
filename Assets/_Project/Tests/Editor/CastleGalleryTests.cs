using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CloudHop.Tests
{
    public sealed class CastleGalleryTests
    {
        [UnityTest]
        public IEnumerator ThicknessSealsAndRetryStayConsistent()
        {
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/CloudHop.unity");
            yield return new EnterPlayMode();
            yield return null; yield return null;
            var game = Object.FindFirstObjectByType<GameManager>();
            var director = Object.FindFirstObjectByType<StageDirector>();
            var course = Object.FindObjectsByType<StageCourse>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Single(c => c.Definition.difficulty == "HARD");
            var tops = course.Platforms.Select(p => {
                var b = p.GetComponent<BoxCollider2D>();
                return b.offset.y + b.size.y * .5f;
            }).ToArray();
            GameObject.Find("Stages").GetComponent<Button>().onClick.Invoke();
            GameObject.Find("Stage 3").GetComponent<Button>().onClick.Invoke();
            GameObject.Find("Character Play").GetComponent<Button>().onClick.Invoke();
            yield return null; yield return null;
            int thick = 0, thin = 0;
            for (int i = 0; i < course.Platforms.Length; i++)
            {
                var p = course.Platforms[i];
                var b = p.GetComponent<BoxCollider2D>();
                Assert.That(b.offset.y + b.size.y * .5f, Is.EqualTo(tops[i]).Within(.0001f), "Landing top " + i);
                float depth = b.size.y * Mathf.Abs(p.transform.lossyScale.y);
                if (Mathf.Abs(depth - .8f) < .001f) thick++;
                else { Assert.That(depth, Is.EqualTo(.28f).Within(.001f)); thin++; }
            }
            Assert.Greater(thick, 10); Assert.Greater(thin, 10);
            Assert.IsEmpty(course.GetComponentsInChildren<LightningZone>(true));
            var seals = course.GetComponentsInChildren<MagicSealZone>(true);
            Assert.AreEqual(4, seals.Length);
            var clock = course.GetComponent<StageMechanics>();
            var elapsed = typeof(StageMechanics).GetField("<Elapsed>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            var player = Object.FindFirstObjectByType<PlayerController>();
            player.GetComponent<ChargeInput>().enabled = false;
            player.ResetPlayer(seals[0].transform.position);
            var hit = typeof(MagicSealZone).GetMethod("OnTriggerStay2D", BindingFlags.Instance | BindingFlags.NonPublic);
            var contact = new object[] { player.GetComponent<Collider2D>() };
            clock.SetEnabled(true);
            elapsed.SetValue(clock, 4f);
            Assert.AreEqual(HazardPhase.Warning, seals[0].Phase);
            hit.Invoke(seals[0], contact);
            Assert.AreEqual(0, player.HitsTaken, "Warning is safe");
            elapsed.SetValue(clock, 4.8f);
            Assert.AreEqual(HazardPhase.Active, seals[0].Phase);
            hit.Invoke(seals[0], contact);
            Assert.AreEqual(1, player.HitsTaken, "Active curtain repels the player");
            hit.Invoke(seals[0], contact);
            Assert.AreEqual(1, player.HitsTaken, "Hit protection prevents repeated damage");
            clock.SetEnabled(false);
            Assert.IsFalse(seals[0].IsActive);
            clock.SetEnabled(true);
            Assert.AreEqual(HazardPhase.Safe, seals[0].Phase);
            game.Retry();
            game.SelectStage(0); game.SelectStage(2);
            yield return null;
            Assert.AreEqual(4, course.GetComponentsInChildren<MagicSealZone>(true).Length);
            Assert.AreEqual(1, course.GetComponents<CastleGallery>().Length);
            Assert.That(course.Platforms[0].GetComponent<BoxCollider2D>().bounds.size.y, Is.EqualTo(.8f).Within(.001f));
            Object.FindFirstObjectByType<FollowCamera>().enabled = false;
            Time.timeScale = 0;
            Camera.main.transform.position = new Vector3(seals[0].transform.position.x, 1.8f, -10);
            GameUITests.Capture("castle-seal-safe", 1920, 1080);
            elapsed.SetValue(clock, 4f); yield return null;
            GameUITests.Capture("castle-seal-warning", 1920, 1080);
            elapsed.SetValue(clock, 4.8f); yield return null;
            GameUITests.Capture("castle-seal-active", 1920, 1080);
            Time.timeScale = 1;
            yield return new ExitPlayMode();
        }
    }
}
