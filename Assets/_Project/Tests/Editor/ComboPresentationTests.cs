using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;
namespace CloudHop.Tests
{
 public sealed class ComboPresentationTests
 {
  [UnityTest] public IEnumerator ReadyAndBoostEffectsDisplayInActualCourse()
  {
   SessionState.SetInt("ComboTest.Character",PlayerPrefs.GetInt("CloudHop.Character",0));
   SessionState.SetBool("ComboTest.HasCharacter",PlayerPrefs.HasKey("CloudHop.Character"));
   SessionState.SetInt("ComboTest.Best",PlayerPrefs.GetInt("CloudHop.BestScore.v1",0));
   SessionState.SetBool("ComboTest.HasBest",PlayerPrefs.HasKey("CloudHop.BestScore.v1"));
   EditorSceneManager.OpenScene("Assets/_Project/Scenes/CloudHop.unity");
   yield return new EnterPlayMode();yield return Run();
  }
  static IEnumerator Delay(float seconds) { float end=Time.time+seconds; while(Time.time<end)yield return null; }
  static void Click(string name)=>GameObject.Find(name).GetComponent<Button>().onClick.Invoke();
  static IEnumerator Run()
  {
   yield return null;yield return null;
   Click("Stages");Click("Stage 1");Click("Character 0");Click("Character Play");
   yield return Delay(.3f);
   var p=Object.FindFirstObjectByType<PlayerController>();var stages=Object.FindFirstObjectByType<StageDirector>();
   var zone=stages.CurrentCourse.Platforms[3].GetComponent<PerfectLandingZone>();Assert.IsNotNull(zone);
   var bounds=zone.GetComponent<BoxCollider2D>().bounds;
   p.ResetPlayer(new Vector3(bounds.center.x,bounds.max.y+.48f,0));
   yield return Delay(.6f);Assert.IsTrue(p.IsGrounded);
   p.GetComponent<ChargeInput>().enabled=false;
   for(int i=1;i<=3;i++)p.Combo.Land(i,true);
   yield return null;GameUITests.Capture("combo-ready",1920,1080);
   p.BeginCharge();yield return Delay(.6f);p.ReleaseCharge();
   yield return Delay(.2f);Assert.IsTrue(p.TryUseBoost());
   yield return Delay(.1f);
   yield return null;GameUITests.Capture("combo-boost",1920,1080);
   Assert.IsNotNull(GameObject.Find("Route Caption 1"), "Safe route caption remains visible while boosting");
   var trail=p.GetComponentInChildren<TrailRenderer>();Assert.IsNotNull(trail);Assert.Greater(trail.positionCount,1);
   Assert.IsTrue(p.TryKnockback(new Vector2(-2,4)));Assert.AreEqual(0,p.Combo.Count);Assert.IsFalse(p.IsBoosting);
   p.ResetPlayer(stages.CurrentCourse.SpawnPosition);Assert.IsFalse(p.Combo.BoostReady);Assert.AreEqual(0,trail.positionCount);
  }
  [UnityTearDown] public IEnumerator Cleanup()
  {
   if(EditorApplication.isPlaying)yield return new ExitPlayMode();Time.timeScale=1;
   if(SessionState.GetBool("ComboTest.HasBest",false))PlayerPrefs.SetInt("CloudHop.BestScore.v1",SessionState.GetInt("ComboTest.Best",0));else PlayerPrefs.DeleteKey("CloudHop.BestScore.v1");
   if(SessionState.GetBool("ComboTest.HasCharacter",false))PlayerPrefs.SetInt("CloudHop.Character",SessionState.GetInt("ComboTest.Character",0));else PlayerPrefs.DeleteKey("CloudHop.Character");PlayerPrefs.Save();
  }
 }
}
