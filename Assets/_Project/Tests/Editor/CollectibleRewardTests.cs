using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace CloudHop.Tests {
 public sealed class CollectibleRewardTests {
  [UnityTest] public IEnumerator AllStagesRewardGoldRequireCrystalsAndReset() {
   SessionState.SetInt("Collect.best",PlayerPrefs.GetInt("CloudHop.BestScore.v1",0));
   SessionState.SetBool("Collect.exists",PlayerPrefs.HasKey("CloudHop.BestScore.v1"));
   EditorSceneManager.OpenScene("Assets/_Project/Scenes/CloudHop.unity");
   yield return new EnterPlayMode();yield return Run();
  }
  static IEnumerator Wait(float seconds){float end=Time.time+seconds;float timeout=Time.realtimeSinceStartup+5;while(Time.time<end && Time.realtimeSinceStartup<timeout)yield return null;}
  static IEnumerator Run(){
   yield return null;yield return null;
   GameObject.Find("Stages").GetComponent<Button>().onClick.Invoke();
   GameObject.Find("Stage 1").GetComponent<Button>().onClick.Invoke();
   GameObject.Find("Character Play").GetComponent<Button>().onClick.Invoke();
   var game=Object.FindFirstObjectByType<GameManager>();var stages=Object.FindFirstObjectByType<StageDirector>();
   var player=Object.FindFirstObjectByType<PlayerController>();player.GetComponent<ChargeInput>().enabled=false;
   for(int stage=0;stage<3;stage++){
    game.SelectStage(stage);Time.timeScale=1;yield return Wait(.2f);
    var c=stages.CurrentCourse.GetComponent<StageCollectibles>();
    Assert.IsNotNull(c);Assert.IsFalse(c.CanFinish);Assert.Greater(c.Total,0);
    var items=c.GetComponentsInChildren<CloudCoin>();
    Assert.AreEqual(3,items.Count(x=>x.IsCrystal));
    var gold=items.First(x=>x.Value==5);var collider=player.GetComponent<Collider2D>();
    gold.SendMessage("OnTriggerEnter2D",collider);gold.SendMessage("OnTriggerEnter2D",collider);
    Assert.AreEqual(1,player.Combo.BoostCharges);Assert.AreEqual(1,c.GoldCount);Assert.AreEqual(5,c.Collected);
    var silver=items.First(x=>x.Value==1);
    Time.timeScale=0;silver.SendMessage("OnTriggerEnter2D",collider);Assert.IsFalse(silver.Collected);Time.timeScale=1;
    silver.SendMessage("OnTriggerEnter2D",collider);silver.SendMessage("OnTriggerEnter2D",collider);
    Assert.AreEqual(1,c.SilverCount);Assert.AreEqual(6,c.Collected);
    Assert.AreEqual(1,c.Medal);
    foreach(var coin in items.Where(x=>x.Value>0&&!x.Collected)){coin.SendMessage("OnTriggerEnter2D",collider);if(c.Collected>=c.Total*.5f)break;} Assert.AreEqual(2,c.Medal);
    foreach(var coin in items.Where(x=>x.Value>0&&!x.Collected)){coin.SendMessage("OnTriggerEnter2D",collider);if(c.Collected>=c.Total*.8f)break;} Assert.AreEqual(3,c.Medal);
    foreach(var gem in items.Where(x=>x.IsCrystal))gem.SendMessage("OnTriggerEnter2D",collider);
    Assert.AreEqual(3,c.Crystals);Assert.IsTrue(c.CompleteCollection);
    yield return null;GameUITests.Capture("collect-rewards-"+stage,1920,1080);
    game.Retry();Assert.AreEqual(0,c.Collected);Assert.AreEqual(0,c.Crystals);Assert.AreEqual(0,player.Combo.BoostCharges);
    if(stage==0){
     var gem=items.First(x=>x.IsCrystal);
     player.ResetPlayer(gem.transform.position+Vector3.left*1.4f);
     Object.FindFirstObjectByType<FollowCamera>().ResetPosition();
     yield return null;GameUITests.Capture("collect-crystal-world",1920,1080);
    }
    var finish=stages.CurrentCourse.Platforms.Last().GetComponent<Collider2D>().bounds;
    player.ResetPlayer(new Vector3(finish.center.x,finish.max.y+.48f,0));
    yield return Wait(.6f);
    Assert.AreEqual(1,c.Crystals,"Missed required crystal returns to the goal without restarting");
    Assert.AreEqual(GameState.StageClear,game.State);
    yield return null;GameUITests.Capture("collect-result-"+stage,1920,1080);
    if(stage==2)GameUITests.Capture("collect-result-phone",2340,1080);
   }
  }
  [UnityTearDown] public IEnumerator Cleanup(){if(EditorApplication.isPlaying)yield return new ExitPlayMode();Time.timeScale=1;
   if(SessionState.GetBool("Collect.exists",false))PlayerPrefs.SetInt("CloudHop.BestScore.v1",SessionState.GetInt("Collect.best",0));else PlayerPrefs.DeleteKey("CloudHop.BestScore.v1");PlayerPrefs.Save();
  }
 }
}