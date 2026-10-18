using System.Collections;
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
    public sealed class ChoiceRouteTests
    {
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        [UnityTest]
        public IEnumerator SafeRoutesAndBoostShortcutsWorkWithLiveGimmicks()
        {
            SessionState.SetInt("Choice.Best", PlayerPrefs.GetInt("CloudHop.BestScore.v1", 0));
            SessionState.SetBool("Choice.HasBest", PlayerPrefs.HasKey("CloudHop.BestScore.v1"));
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/CloudHop.unity");
            yield return new EnterPlayMode();
            yield return Run();
        }
        static IEnumerator Run()
        {
            yield return null; yield return null;
            GameObject.Find("Stages").GetComponent<Button>().onClick.Invoke();
            GameObject.Find("Stage 2").GetComponent<Button>().onClick.Invoke();
            GameObject.Find("Character Play").GetComponent<Button>().onClick.Invoke();
            var ui = Object.FindFirstObjectByType<GameUI>();
            // Keep failure UI from stopping the physics clock; assertions detect failed landings.
            typeof(GameUI).GetField("playing", Private).SetValue(ui, false);
            var game = Object.FindFirstObjectByType<GameManager>();
            var stages = Object.FindFirstObjectByType<StageDirector>();
            var player = Object.FindFirstObjectByType<PlayerController>();
            player.GetComponent<ChargeInput>().enabled = false;
            float[] shortcutCharge = { .5f, .3f, .05f, .25f };
            float[] shortcutDelay = { .55f, .55f, .15f, .55f };
            float[] safeExitCharge = { .8f, .7f, .9f, .7f };
            int n = 0;
            for (int stage = 1; stage <= 2; stage++)
            {
                game.SelectStage(stage);
                var course = stages.CurrentCourse;
                Assert.AreEqual(2, course.Definition.choiceRoutes.Length);
                foreach (var route in course.Definition.choiceRoutes)
                {
                    // Basic landing is also the recovery when the player does not spend a boost.
                    yield return Jump(game, stages, player, route.launchIndex, route.safeIndex, .2f, -1, 6);
                    yield return Jump(game, stages, player, route.safeIndex, route.rejoinIndex, safeExitCharge[n], -1, 6);
                    yield return Jump(game, stages, player, route.launchIndex, route.rejoinIndex, shortcutCharge[n], shortcutDelay[n], 0, "choice-stage-" + (stage + 1) + "-" + route.launchIndex);
                    Assert.AreEqual(route.rejoinIndex, stages.Progress);
                    Assert.AreNotEqual(GameState.StageClear, game.State);
                    game.Retry();
                    Assert.AreEqual(0, stages.Progress);
                    Assert.IsFalse(player.Combo.BoostReady);
                    n++;
                }
            }
        }
        static IEnumerator Jump(GameManager game, StageDirector stages, PlayerController player, int from, int to, float charge, float boostDelay, float phase, string capture = null)
        {
            game.Retry(); Time.timeScale = 1;
            var course = stages.CurrentCourse;
            var clock = course.GetComponent<StageMechanics>();
            typeof(StageMechanics).GetField("<Elapsed>k__BackingField", Private).SetValue(clock, phase);
            var bounds = course.Platforms[from].GetComponent<Collider2D>().bounds;
            player.ResetPlayer(new Vector3(bounds.center.x, bounds.max.y + .48f, 0));
            float deadline = Time.realtimeSinceStartup + 4;
            while (!player.IsGrounded && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.AreEqual(course.Platforms[from], player.GroundPlatform, "Starting platform");
            if (boostDelay >= 0) for (int i = 1; i <= 3; i++) player.Combo.Land(-100 - i, true);
            if (capture != null)
            {
                Object.FindFirstObjectByType<FollowCamera>().ResetPosition();
                yield return null; yield return null;
                GameUITests.Capture(capture, 1920, 1080);
            }
            player.BeginCharge();
            typeof(PlayerController).GetField("chargeTime", Private).SetValue(player, charge);
            player.ReleaseCharge();
            float start = Time.fixedTime;
            bool airborne = false, used = false;
            deadline = Time.realtimeSinceStartup + 5;
            while (Time.fixedTime - start < 3 && Time.realtimeSinceStartup < deadline)
            {
                if (!player.IsGrounded) airborne = true;
                if (boostDelay >= 0 && airborne && !used && Time.fixedTime - start >= boostDelay) used = player.TryUseBoost();
                if (airborne && player.IsGrounded || game.State == GameState.GameOver) break;
                yield return null;
            }
            Assert.AreEqual(course.Platforms[to], player.GroundPlatform, $"Stage {stages.CurrentIndex + 1}: {from} -> {to}, charge {charge}, boost {boostDelay}");
            Assert.AreEqual(0, player.HitsTaken, "Safe timing avoids lightning");
            if (boostDelay >= 0) Assert.IsTrue(used);
        }
        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (EditorApplication.isPlaying) yield return new ExitPlayMode();
            Time.timeScale = 1;
            if (SessionState.GetBool("Choice.HasBest", false)) PlayerPrefs.SetInt("CloudHop.BestScore.v1", SessionState.GetInt("Choice.Best", 0));
            else PlayerPrefs.DeleteKey("CloudHop.BestScore.v1");
            PlayerPrefs.Save();
        }
    }
}
