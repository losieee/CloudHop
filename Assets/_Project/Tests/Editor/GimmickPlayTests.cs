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
    public sealed class GimmickPlayTests
    {
        private GameManager game;
        private StageDirector stages;
        private PlayerController player;
        private Rigidbody2D body;
        private StageMechanics Clock => stages.CurrentCourse.GetComponent<StageMechanics>();

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SessionState.SetBool("GimmickTests.HadBest", PlayerPrefs.HasKey("CloudHop.BestScore.v1"));
            SessionState.SetInt("GimmickTests.Best", PlayerPrefs.GetInt("CloudHop.BestScore.v1"));
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/CloudHopGimmickLab.unity");
            yield return new EnterPlayMode();
            game = Object.FindFirstObjectByType<GameManager>();
            stages = Object.FindFirstObjectByType<StageDirector>();
            player = Object.FindFirstObjectByType<PlayerController>();
            body = player.GetComponent<Rigidbody2D>();
            player.GetComponent<ChargeInput>().enabled = false;
            Time.timeScale = 2;
            yield return Wait(0.3f);
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1;
            yield return new ExitPlayMode();
            if (SessionState.GetBool("GimmickTests.HadBest", false)) PlayerPrefs.SetInt("CloudHop.BestScore.v1", SessionState.GetInt("GimmickTests.Best",0));
            else PlayerPrefs.DeleteKey("CloudHop.BestScore.v1");
            PlayerPrefs.Save();
            SessionState.EraseBool("GimmickTests.HadBest"); SessionState.EraseInt("GimmickTests.Best");
        }
        [UnityTest]
        public IEnumerator BirdPatrolHitsOnceDuringProtectionAndRetryResets()
        {
            var bird = stages.CurrentCourse.GetComponentInChildren<PatrolBird>();
            Vector2 before = bird.GetComponent<Rigidbody2D>().position;
            yield return Wait(0.4f);
            Assert.Greater(Vector2.Distance(before,bird.GetComponent<Rigidbody2D>().position),0.05f);
            player.ResetPlayer(bird.transform.position);
            yield return Wait(0.06f);
            Assert.AreEqual(1,player.HitsTaken);
            Assert.Less(body.linearVelocity.x,0);
            Assert.Greater(player.HitProtectionRemaining,0);
            Assert.IsFalse(player.TryKnockback(Vector2.left*10),"Protection prevents repeated knockback.");
            Assert.AreEqual(1,player.HitsTaken);
            game.Retry();
            yield return Wait(0.3f);
            Assert.AreEqual(0,player.HitsTaken);
            Assert.AreEqual(0,player.HitProtectionRemaining);
            Assert.IsTrue(player.IsGrounded);
            Assert.Less(Clock.Elapsed,0.5f);
        }
        [UnityTest]
        public IEnumerator UpdraftLiftsAndRepeatsFiveSecondsOnFiveSecondsOff()
        {
            var wind = stages.CurrentCourse.GetComponentInChildren<WindZone>();
            Assert.IsTrue(wind.IsBlowing);
            body.gravityScale = 0;
            player.ResetPlayer(wind.transform.position);
            body.linearVelocity = new Vector2(5,-3);
            yield return Wait(0.08f);
            Assert.GreaterOrEqual(body.linearVelocity.y,6.99f);
            Assert.AreEqual(5,body.linearVelocity.x,0.001f);
            Assert.IsTrue(wind.IsPushing);
            Assert.That(wind.Status, Does.Contain("LIFTING"));
            player.ResetPlayer(stages.CurrentCourse.SpawnPosition);
            yield return Until(() => Clock.Elapsed >= 4.8f,6);
            Assert.IsTrue(wind.IsBlowing);
            yield return Until(() => Clock.Elapsed >= 5.05f,1);
            Assert.IsFalse(wind.IsBlowing);
            player.ResetPlayer(wind.transform.position);
            body.linearVelocity = new Vector2(5,-3);
            yield return Wait(0.08f);
            Assert.AreEqual(-3,body.linearVelocity.y,0.001f);
            player.ResetPlayer(stages.CurrentCourse.SpawnPosition);
            yield return Until(() => Clock.Elapsed >= 9.8f,6);
            Assert.IsFalse(wind.IsBlowing);
            yield return Until(() => Clock.Elapsed >= 10.05f,1);
            Assert.IsTrue(wind.IsBlowing);
            body.gravityScale = 2;
            game.Retry();
            Assert.IsTrue(wind.IsBlowing);
            Assert.IsFalse(wind.IsPushing);
            yield return Wait(0.3f);
            Assert.IsTrue(player.IsGrounded);
            player.BeginCharge();
            Assert.IsTrue(player.ApplyUpdraft(7));
            Assert.IsFalse(player.IsGrounded);
            Assert.IsFalse(player.IsCharging);
            Assert.GreaterOrEqual(body.linearVelocity.y,7);
            player.SetPlayable(false);
            Assert.IsFalse(player.ApplyUpdraft(7));
        }
        [UnityTest]
        public IEnumerator LightningWarnsThenHitsAndClearOrGameOverFreezesClock()
        {
            game.SelectStage(2);
            yield return Wait(0.3f);
            var bolt = stages.CurrentCourse.GetComponentInChildren<LightningZone>();
            body.gravityScale = 0;
            // A stationary zero-gravity test body would otherwise sleep and stop trigger-stay callbacks.
            body.sleepMode = RigidbodySleepMode2D.NeverSleep;
            player.ResetPlayer(bolt.transform.position);
            yield return Until(() => bolt.Phase == HazardPhase.Warning,5);
            Assert.AreEqual(0,player.HitsTaken);
            Assert.GreaterOrEqual(bolt.Remaining,0.9f,"Warning must be readable before a strike.");
            yield return Until(() => player.HitsTaken > 0,3);
            Assert.AreEqual(HazardPhase.Active,bolt.Phase);
            Assert.Less(body.linearVelocity.x,0);
            body.gravityScale = 2;
            body.position = new Vector2(0,-7);
            yield return Wait(0.1f);
            Assert.AreEqual(GameState.GameOver,game.State);
            float frozen = Clock.Elapsed;
            yield return Wait(0.3f);
            Assert.AreEqual(frozen,Clock.Elapsed);
            game.Retry();
            Assert.AreEqual(HazardPhase.Safe,bolt.Phase);
        }
        [UnityTest]
        public IEnumerator CrumbleArmsOnLandingDisappearsRespawnsAndResets()
        {
            game.SelectStage(1);
            yield return Wait(0.3f);
            var cloud = stages.CurrentCourse.GetComponentInChildren<CrumblingPlatform>();
            var surface = cloud.GetComponent<Collider2D>();
            player.ResetPlayer(new Vector3(surface.bounds.center.x,surface.bounds.max.y+0.48f,0));
            yield return Until(() => cloud.Phase == CloudPhase.Crumbling,1);
            Assert.IsTrue(player.IsGrounded);
            yield return Until(() => cloud.Phase == CloudPhase.Gone,3);
            Assert.IsFalse(surface.enabled);
            yield return Wait(0.1f);
            Assert.IsFalse(player.IsGrounded,"A vanished cloud must release its passenger.");
            // Stay alive to observe its respawn; the timer intentionally freezes at Game Over.
            player.ResetPlayer(stages.CurrentCourse.SpawnPosition);
            yield return Until(() => cloud.Phase == CloudPhase.Solid,3);
            Assert.IsTrue(surface.enabled);
            player.ResetPlayer(new Vector3(surface.bounds.center.x,surface.bounds.max.y+0.48f,0));
            yield return Until(() => cloud.Phase == CloudPhase.Crumbling,1);
            game.Retry();
            Assert.AreEqual(CloudPhase.Solid,cloud.Phase);
            Assert.IsTrue(surface.enabled);
        }
        [UnityTest]
        public IEnumerator MovingPlatformCarriesPlayerThroughFullCycleAndAllowsChargeJump()
        {
            game.SelectStage(1);
            yield return Wait(0.3f);
            var moving = stages.CurrentCourse.GetComponentInChildren<MovingPlatform>();
            var surface = moving.GetComponent<Collider2D>();
            player.ResetPlayer(new Vector3(surface.bounds.center.x,surface.bounds.max.y+0.7f,0));
            yield return Until(() => player.IsGrounded,1.5f);
            int score = Object.FindFirstObjectByType<ScoreManager>().Score;
            for (int i = 0; i < 45; i++)
            {
                yield return Wait(0.1f);
                Assert.IsTrue(player.IsGrounded,"Passenger detached during vertical travel.");
                Assert.IsTrue(player.GroundPlatform == moving.GetComponent<Platform>(), "Ground must remain the moving platform.");
                float feet = body.position.y-player.GetComponent<BoxCollider2D>().bounds.extents.y;
                Assert.Less(Mathf.Abs(feet-surface.bounds.max.y),0.12f);
            }
            Assert.AreEqual(score,Object.FindFirstObjectByType<ScoreManager>().Score);
            player.BeginCharge();
            yield return Wait(0.45f);
            Assert.IsTrue(player.IsCharging);
            player.ReleaseCharge();
            yield return Wait(0.08f);
            Assert.IsFalse(player.IsGrounded);
            Assert.Greater(body.linearVelocity.x,0);
        }
        [UnityTest]
        public IEnumerator ToggleDisablesEffectsAndKeepsSelectedStage()
        {
            game.SelectStage(1);
            yield return Wait(0.3f);
            var moving = stages.CurrentCourse.GetComponentInChildren<MovingPlatform>();
            GameObject.Find("Toggle Gimmicks").GetComponent<Button>().onClick.Invoke();
            yield return Wait(0.1f);
            Vector2 position = moving.GetComponent<Rigidbody2D>().position;
            Assert.AreEqual(1,stages.CurrentIndex);
            Assert.IsFalse(Clock.Enabled);
            yield return Wait(0.5f);
            Assert.AreEqual(position,moving.GetComponent<Rigidbody2D>().position);
            Assert.AreEqual(0,Clock.Elapsed);
            GameObject.Find("Toggle Gimmicks").GetComponent<Button>().onClick.Invoke();
            yield return Wait(0.2f);
            Assert.IsTrue(Clock.Enabled);
            Assert.Greater(Vector2.Distance(position,moving.GetComponent<Rigidbody2D>().position),0.01f);
        }
        private static IEnumerator Until(System.Func<bool> predicate,float timeout)
        {
            float end = Time.time+timeout;
            while (!predicate() && Time.time < end) yield return null;
            Assert.IsTrue(predicate(),"Timed out waiting for gimmick state.");
        }
        private static IEnumerator Wait(float seconds)
        {
            float end = Time.time+seconds;
            double timeout = EditorApplication.timeSinceStartup+15;
            while (Time.time < end)
            {
                Assert.Less(EditorApplication.timeSinceStartup,timeout);
                yield return null;
            }
        }
    }
}
