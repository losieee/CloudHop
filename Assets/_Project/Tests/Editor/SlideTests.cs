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
    public sealed class SlideTests
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        [UnityTest] public IEnumerator SlopeMotionJumpExitAndAllCharacterPoses()
        {
            SessionState.SetInt("Slide.Best",PlayerPrefs.GetInt("CloudHop.BestScore.v1",0));
            SessionState.SetBool("Slide.HasBest",PlayerPrefs.HasKey("CloudHop.BestScore.v1"));
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
            var visual=player.GetComponentInChildren<CharacterVisual>();
            yield return Jump(game,player,course.Platforms[28],course.Platforms[29]);
            foreach(string skin in new[]{"CloudAdventurer","GirlCharacter","FoxCharacter"})
            {
                game.Retry(); Time.timeScale=1; player.GetComponent<ChargeInput>().enabled=false;
                visual.Apply(AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/ScriptableObjects/"+skin+".asset"));
                player.ResetPlayer(new Vector3(115.7f,2.1f,0));
                deadline=Time.realtimeSinceStartup+5;
                while(!player.IsSliding && Time.realtimeSinceStartup<deadline)yield return null;
                Assert.IsTrue(player.IsSliding,"Slope entered: "+skin);
                while(player.transform.position.x<118.4f && Time.realtimeSinceStartup<deadline)yield return null;
                Assert.IsTrue(player.IsSliding,"Stays attached on downhill: "+skin);
                Assert.Less(player.GetComponent<Rigidbody2D>().linearVelocity.y,0);
                var data=AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/ScriptableObjects/"+skin+".asset");
                Assert.AreEqual(data.chargeSprite,visual.GetComponent<SpriteRenderer>().sprite);
                typeof(GameUI).GetField("playing",Private).SetValue(ui,true);
                GameUITests.Capture("slide-"+skin,1920,1080);
                typeof(GameUI).GetField("playing",Private).SetValue(ui,false);
                deadline=Time.realtimeSinceStartup+5;
                while(player.GroundPlatform!=course.Platforms[30] && Time.realtimeSinceStartup<deadline)yield return null;
                Assert.AreEqual(course.Platforms[30],player.GroundPlatform,"Flat exit catches the slide");
                yield return null; yield return null;
                Assert.IsFalse(player.IsSliding);
                Assert.Less(player.GetComponent<Rigidbody2D>().linearVelocity.magnitude,.3f);
                game.Retry(); player.ResetPlayer(new Vector3(115.7f,2.1f,0));
                deadline=Time.realtimeSinceStartup+4;
                while(!player.IsSliding && Time.realtimeSinceStartup<deadline)yield return null;
                player.BeginCharge();typeof(PlayerController).GetField("chargeTime",Private).SetValue(player,.4f);player.ReleaseCharge();
                float until=Time.fixedTime+.1f;
                while(Time.fixedTime<until && Time.realtimeSinceStartup<deadline)yield return null;
                Assert.IsFalse(player.IsSliding);Assert.Greater(player.GetComponent<Rigidbody2D>().linearVelocity.y,0,"Jump out of slide");
                game.Retry();yield return null;Assert.IsFalse(player.IsSliding);
                Assert.Less(Quaternion.Angle(visual.transform.localRotation,Quaternion.identity),.1f);
            }
            yield return Jump(game,player,course.Platforms[30],course.Platforms[31]);
            yield return Jump(game,player,course.Platforms[31],course.Platforms[32]);
        }

        private static IEnumerator Jump(GameManager game,PlayerController player,Platform from,Platform to)
        {
            game.Retry();Time.timeScale=1;player.GetComponent<ChargeInput>().enabled=false;
            var start=from.GetComponent<Collider2D>().bounds;var end=to.GetComponents<Collider2D>().First(c=>c.enabled).bounds;
            if(to.GetComponent<SlidingSlope>()!=null)end=new Bounds(new Vector3(to.transform.position.x,to.transform.position.y,0),new Vector3(2,.6f,1));
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
            if(SessionState.GetBool("Slide.HasBest",false))PlayerPrefs.SetInt("CloudHop.BestScore.v1",SessionState.GetInt("Slide.Best",0));
            else PlayerPrefs.DeleteKey("CloudHop.BestScore.v1");PlayerPrefs.Save();
        }
    }
}

