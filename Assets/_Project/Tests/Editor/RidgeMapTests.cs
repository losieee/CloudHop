using System.Collections;
using System.Linq;
using System.Reflection;
using CloudHop.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;
namespace CloudHop.Tests
{
    public sealed class RidgeMapTests
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        [UnityTest] public IEnumerator BothRoutesCoinsAndRetryWorkOnSharedMap()
        {
            SessionState.SetInt("Ridge.Best",PlayerPrefs.GetInt("CloudHop.BestScore.v1",0));
            SessionState.SetBool("Ridge.HasBest",PlayerPrefs.HasKey("CloudHop.BestScore.v1"));
            EditorSceneManager.OpenScene(PlatformBuilds.AndroidScene);
            yield return new EnterPlayMode(); yield return Run();
        }
        private static IEnumerator Run()
        {
            float deadline=Time.realtimeSinceStartup+15;
            while(Object.FindFirstObjectByType<GameUI>()==null && Time.realtimeSinceStartup<deadline)yield return null;
            yield return null;yield return null;
            var ui=Object.FindFirstObjectByType<GameUI>();
            Click("Stages");Click("Stage 2");Click("Character Play");
            typeof(GameUI).GetField("playing",Private).SetValue(ui,false);
            var player=Object.FindFirstObjectByType<PlayerController>();
            var game=Object.FindFirstObjectByType<GameManager>();
            var stages=Object.FindFirstObjectByType<StageDirector>();
            var course=stages.CurrentCourse;
            var collection=course.GetComponent<StageCollectibles>(); course.GetComponent<StageMechanics>().SetEnabled(false);
            Assert.AreEqual(136,course.Definition.coins.Length);
            Assert.AreEqual(152,collection.Total);
            for(int i=12;i<48;i++)yield return Jump(game,player,course.Platforms[i],course.Platforms[i+1]);
            var upper=course.GetComponentsInChildren<BranchLanding>().OrderBy(x=>x.transform.position.x).ToArray();
            Assert.AreEqual(3,upper.Length);
            yield return Jump(game,player,course.Platforms[16],upper[0].GetComponent<Platform>());
            float cameraEnd=Time.time+1; deadline=Time.realtimeSinceStartup+4;
            while(Time.time<cameraEnd && Time.realtimeSinceStartup<deadline)yield return null;
            Assert.Greater(Camera.main.transform.position.y,2.1f,"Camera follows the upper route");
            yield return Jump(game,player,upper[0].GetComponent<Platform>(),upper[1].GetComponent<Platform>());
            yield return Jump(game,player,upper[1].GetComponent<Platform>(),upper[2].GetComponent<Platform>());
            yield return Jump(game,player,upper[2].GetComponent<Platform>(),course.Platforms[20]); Assert.AreEqual(20,stages.Progress);
            game.Retry();Time.timeScale=1;
            course.GetComponent<StageMechanics>().SetEnabled(true); foreach(int i in new[] {34,35,42,43}) yield return Jump(game,player,course.Platforms[i],course.Platforms[i+1]); game.Retry(); var coin=course.GetComponentsInChildren<CloudCoin>().First(c=>c.Value==5);
            player.ResetPlayer(coin.transform.position);
            float until=Time.fixedTime+.08f;deadline=Time.realtimeSinceStartup+3;
            while(Time.fixedTime<until && Time.realtimeSinceStartup<deadline)yield return null;
            Assert.IsTrue(coin.Collected); int collected=course.GetComponentsInChildren<CloudCoin>().Where(c=>c.Collected).Sum(c=>c.Value); Assert.AreEqual(collected,collection.Collected); Assert.GreaterOrEqual(collected,5);
            coin.SendMessage("OnTriggerEnter2D",player.GetComponent<Collider2D>());
            Assert.AreEqual(collected,collection.Collected,"A coin can only be collected once");
            game.Retry();Assert.AreEqual(0,collection.Collected);Assert.IsFalse(coin.Collected);
            var start=course.Platforms[16].GetComponent<Collider2D>().bounds;
            player.ResetPlayer(new Vector3(start.center.x,start.max.y+.48f,0));
            until=Time.time+1.4f;deadline=Time.realtimeSinceStartup+4;
            while(Time.time<until && Time.realtimeSinceStartup<deadline)yield return null;
            player.GetComponent<ChargeInput>().enabled=true; typeof(GameUI).GetField("playing",Private).SetValue(ui,true);
            yield return null;yield return null;
            GameUITests.Capture("ridge-branch-phone",2340,1080);
            GameUITests.Capture("ridge-branch-tablet",1600,1200);
            Assert.Less(Camera.main.transform.position.y,2.2f,"Keep the lower branch above the bottom HUD");
            typeof(GameUI).GetField("playing",Private).SetValue(ui,true);
            player.ResetPlayer(new Vector3(start.center.x,-7,0));
            deadline=Time.realtimeSinceStartup+4;
            while(game.State!=GameState.GameOver && Time.realtimeSinceStartup<deadline)yield return null;
            yield return null;yield return null;
            Assert.AreEqual(GameState.GameOver,game.State);
            GameUITests.Capture("ridge-result-mobile",2340,1080);
            Click("Retry");yield return null;yield return null;
            Assert.AreEqual(0,collection.Collected);
        }
        private static IEnumerator Jump(GameManager game,PlayerController player,Platform from,Platform to)
        {
            game.Retry();Time.timeScale=1;player.GetComponent<ChargeInput>().enabled=false;
            var start=from.GetComponent<Collider2D>().bounds;var end=to.GetComponent<Collider2D>().bounds;
            player.ResetPlayer(new Vector3(start.center.x,start.max.y+.48f,0));
            float deadline=Time.realtimeSinceStartup+4;
            while(!player.IsGrounded && Time.realtimeSinceStartup<deadline)yield return null;
            Assert.AreEqual(from,player.GroundPlatform);
            float t=(end.center.x-start.center.x)/5;
            float v=(end.max.y-start.max.y+.5f*19.62f*t*t)/t+19.62f*Time.fixedDeltaTime*.5f;
            float charge=Mathf.Clamp01((v-5)/6);
            player.BeginCharge();typeof(PlayerController).GetField("chargeTime",Private).SetValue(player,charge);player.ReleaseCharge();
            bool airborne=false;float begin=Time.fixedTime;deadline=Time.realtimeSinceStartup+5;
            while(Time.fixedTime-begin<3 && Time.realtimeSinceStartup<deadline)
            {
                if(!player.IsGrounded)airborne=true;
                if(airborne&&player.IsGrounded || game.State==GameState.GameOver)break;
                yield return null;
            }
            if(to.name=="FINISH") { Assert.AreEqual(GameState.StageClear,game.State); yield break; } Assert.AreEqual(to,player.GroundPlatform,from.name+" -> "+to.name+", charge="+charge);
        }
        private static void Click(string name)=>GameObject.Find(name).GetComponent<Button>().onClick.Invoke();
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(EditorApplication.isPlaying)yield return new ExitPlayMode();Time.timeScale=1;
            if(SessionState.GetBool("Ridge.HasBest",false))PlayerPrefs.SetInt("CloudHop.BestScore.v1",SessionState.GetInt("Ridge.Best",0));
            else PlayerPrefs.DeleteKey("CloudHop.BestScore.v1");PlayerPrefs.Save();
        }
    }
}

