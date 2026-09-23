using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace CloudHop.Tests
{
    public sealed class StagePlayTests
    {
        private const string BestKey = "CloudHop.BestScore.v1";
        private const string BackupKey = "CloudHop.StageTests.Best";
        private const string ExistsKey = "CloudHop.StageTests.Exists";

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SessionState.SetBool(ExistsKey, PlayerPrefs.HasKey(BestKey));
            SessionState.SetInt(BackupKey, PlayerPrefs.GetInt(BestKey));
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/CloudHopStages.unity");
            yield return new EnterPlayMode();
            // Map tests drive the controller. Keyboard/mouse are covered by PrototypePlayTests.
            Object.FindFirstObjectByType<ChargeInput>().enabled = false;
            Time.timeScale = 3f;
            yield return Wait(0.3f);
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1;
            yield return new ExitPlayMode();
            if (SessionState.GetBool(ExistsKey, false)) PlayerPrefs.SetInt(BestKey, SessionState.GetInt(BackupKey, 0));
            else PlayerPrefs.DeleteKey(BestKey);
            PlayerPrefs.Save();
            SessionState.EraseBool(ExistsKey);
            SessionState.EraseInt(BackupKey);
        }

        [UnityTest, Timeout(240000)]
        public IEnumerator EveryLandingInAllThreeStagesIsReachableAndClearAdvances()
        {
            var director = Object.FindFirstObjectByType<StageDirector>();
            var player = Object.FindFirstObjectByType<PlayerController>();
            var game = Object.FindFirstObjectByType<GameManager>();
            var scores = Object.FindFirstObjectByType<ScoreManager>();
            var body = player.GetComponent<Rigidbody2D>();
            var settings = new SerializedObject(player);
            float horizontal = settings.FindProperty("horizontalSpeed").floatValue;
            float minimum = settings.FindProperty("minimumJumpSpeed").floatValue;
            float maximum = settings.FindProperty("maximumJumpSpeed").floatValue;
            float gravity = -Physics2D.gravity.y * body.gravityScale;
            int[] expectedCounts = { 40, 48, 56 };
            for (int stage = 0; stage < 3; stage++)
            {
                Assert.AreEqual(stage, director.CurrentIndex);
                var course = director.CurrentCourse;
                Assert.AreEqual(expectedCounts[stage], course.JumpCount);
                Assert.AreEqual(0, scores.Score);
                for (int i = 1; i <= course.JumpCount; i++)
                {
                    Assert.IsTrue(player.IsGrounded, $"Stage {stage+1}, before jump {i}");
                    var target = course.Platforms[i].GetComponent<BoxCollider2D>().bounds;
                    float travel = (target.center.x - body.position.x) / horizontal;
                    float landingY = target.max.y + player.GetComponent<BoxCollider2D>().bounds.extents.y + 0.01f;
                    // Account for Unity's fixed-step gravity integration, then drive normal charge/release.
                    float launch = (landingY - body.position.y) / travel + 0.5f * gravity * (travel + Time.fixedDeltaTime);
                    Assert.That(launch, Is.InRange(minimum, maximum), $"Stage {stage+1}, jump {i}: center is outside charge range");
                    Assert.Less(launch - gravity * travel, 0, "Target must be reached while descending.");
                    float charge = Mathf.InverseLerp(minimum, maximum, launch);
                    player.BeginCharge();
                    float chargeDeadline = Time.time + 1.5f;
                    while (player.Charge01 < charge && Time.time < chargeDeadline) yield return null;
                    Assert.IsTrue(player.IsCharging);
                    player.ReleaseCharge();
                    yield return Wait(0.06f);
                    float deadline = Time.time + 2f;
                    while (!player.IsGrounded && game.State != GameState.StageClear && game.State != GameState.GameOver && Time.time < deadline)
                        yield return null;
                    Assert.AreNotEqual(GameState.GameOver, game.State, $"Stage {stage+1}, jump {i} fell at {body.position}");
                    Assert.AreEqual(i, director.Progress, $"Stage {stage+1}, jump {i}: player {body.position}, target {target}");
                    Assert.AreEqual(i, scores.Score, "Every authored landing awards exactly one point.");
                }
                Assert.AreEqual(GameState.StageClear, game.State);
                Assert.IsFalse(body.simulated, "Clear must stop the player on the finish platform.");
                Assert.IsTrue(GameObject.Find("Result").GetComponent<Text>().text.Contains(stage == 2 ? "ALL STAGES CLEAR" : "STAGE CLEAR"));
                Debug.Log($"STAGE_ROUTE_PASS: stage {stage+1}, {course.JumpCount} consecutive jumps, length {course.Definition.platforms[course.JumpCount].position.x:F1}");
                GameObject.Find("Next Stage").GetComponent<Button>().onClick.Invoke();
                yield return Wait(0.3f);
            }
            Assert.AreEqual(0, director.CurrentIndex, "Restart All returns to the first course.");
            Assert.AreEqual(GameState.Ready, game.State);
            Assert.AreEqual(0, scores.Score);
        }

        [UnityTest]
        public IEnumerator StageSelectionAndRetryKeepTheChosenMap()
        {
            var game = Object.FindFirstObjectByType<GameManager>();
            var director = Object.FindFirstObjectByType<StageDirector>();
            var player = Object.FindFirstObjectByType<PlayerController>();
            GameObject.Find("Select Stage 3").GetComponent<Button>().onClick.Invoke();
            yield return Wait(0.3f);
            Assert.AreEqual(2, director.CurrentIndex);
            Assert.IsTrue(player.IsGrounded);
            Assert.AreEqual(1, Object.FindObjectsByType<StageCourse>(FindObjectsSortMode.None).Length);
            player.GetComponent<Rigidbody2D>().position = new Vector2(-6, 1);
            yield return Wait(1.2f);
            Assert.AreEqual(GameState.GameOver, game.State);
            GameObject.Find("Retry").GetComponent<Button>().onClick.Invoke();
            yield return Wait(0.3f);
            Assert.AreEqual(2, director.CurrentIndex);
            Assert.AreEqual(0, director.Progress);
            Assert.AreEqual(GameState.Ready, game.State);
            Assert.IsTrue(player.IsGrounded);
            Assert.AreEqual(3f, Camera.main.transform.position.x, 0.05f);
        }
        private static IEnumerator Wait(float seconds)
        {
            float end = Time.time + seconds;
            double timeout = EditorApplication.timeSinceStartup + 15;
            while (Time.time < end)
            {
                Assert.Less(EditorApplication.timeSinceStartup, timeout);
                yield return null;
            }
        }
    }
}
