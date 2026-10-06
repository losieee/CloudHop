using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
namespace CloudHop.Tests
{
 public sealed class ComboTests
 {
  [Test] public void RewardsAreBankedCappedAndCannotBeFarmed()
  {
   var c=new ComboChain(); c.Remember(0);
   Assert.AreEqual(ComboLanding.None,c.Land(0,true));
   c.Land(1,true);c.Land(2,true);Assert.IsFalse(c.BoostReady);
   c.Land(3,true);Assert.IsTrue(c.BoostReady);
   Assert.AreEqual(ComboLanding.Broken,c.Land(4,false));Assert.AreEqual(0,c.Count);Assert.IsTrue(c.BoostReady);
   c.Land(5,true);c.Land(6,true);c.Land(7,true);
   Assert.IsTrue(c.ConsumeBoost());Assert.IsFalse(c.ConsumeBoost());
   Assert.AreEqual(ComboLanding.Broken,c.Land(7,true));
   c.Reset();Assert.AreEqual(0,c.Count);Assert.IsFalse(c.BoostReady);
  }
  [UnityTest] public IEnumerator PhysicsEarnsComboAndBoostSkipsTrialPlatform()
  {
   EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   yield return new EnterPlayMode();yield return RunPhysics();
  }
  static Platform Make(float x,float top,float width)
  {
   var o=new GameObject("Combo test platform");o.transform.position=new Vector3(x,top-.3f,0);
   o.AddComponent<BoxCollider2D>().size=new Vector2(width,.6f);
   var p=o.AddComponent<Platform>();o.AddComponent<PerfectLandingZone>().Configure(.5f);return p;
  }
  static void Jump(PlayerController p,float seconds)
  {
   p.BeginCharge();typeof(PlayerController).GetField("chargeTime",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(p,seconds);p.ReleaseCharge();
  }
  static IEnumerator Wait(System.Func<bool> predicate,string message)
  {
   float end=Time.realtimeSinceStartup+4;while(!predicate()&&Time.realtimeSinceStartup<end)yield return null;
   Assert.IsTrue(predicate(),message);
  }
  static IEnumerator RunPhysics()
  {
   Time.timeScale=1;
   new GameObject("Test audio listener").AddComponent<AudioListener>();
   var platforms=new Platform[4];for(int i=0;i<4;i++)platforms[i]=Make(i*2.7f,0,2);
   var player=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/Player.prefab")).GetComponent<PlayerController>();
   player.GetComponent<ChargeInput>().enabled=false;
   player.ResetPlayer(new Vector3(0,.5f,0));yield return Wait(()=>player.IsGrounded,"spawn");
   Assert.AreEqual(0,player.Combo.Count);
   for(int i=1;i<=3;i++)
   {
    Jump(player,.1f);yield return Wait(()=>!player.IsGrounded,"takeoff");yield return Wait(()=>player.IsGrounded,"landing");
    Assert.AreEqual(platforms[i],player.GroundPlatform);Assert.AreEqual(i,player.Combo.Count);
   }
   Assert.IsTrue(player.Combo.BoostReady);Assert.IsFalse(player.TryUseBoost(),"No ground boost");
   foreach(var p in platforms)Object.Destroy(p.gameObject);
   yield return null;
   // Geometry of the first-stage trial: third, fourth, fifth platforms.
   var launch=Make(10.2f,.25f,3);var safe=Make(14,.25f,3.2f);var target=Make(17.4f,0,2.8f);
   player.ResetPlayer(new Vector3(10.2f,.75f,0));yield return Wait(()=>player.IsGrounded,"trial spawn");
   Jump(player,.4f);yield return Wait(()=>!player.IsGrounded,"normal takeoff");yield return Wait(()=>player.IsGrounded,"normal route");
   Assert.AreEqual(safe,player.GroundPlatform,"Ordinary jump preserves safe path");
   player.ResetPlayer(new Vector3(10.2f,.75f,0));yield return Wait(()=>player.IsGrounded,"boost spawn");
   for(int i=1;i<=3;i++)player.Combo.Land(i,true);
   Jump(player,.6f);yield return Wait(()=>!player.IsGrounded,"boost takeoff");float airStart=Time.fixedTime; yield return Wait(()=>Time.fixedTime>=airStart+.2f,"airborne timing");
   Time.timeScale=0;Assert.IsFalse(player.TryUseBoost());Assert.IsTrue(player.Combo.BoostReady);Time.timeScale=1;
   player.BeginCharge();Assert.IsTrue(player.IsBoosting);Assert.IsFalse(player.Combo.BoostReady);
   Assert.Greater(player.GetComponent<Rigidbody2D>().linearVelocity.x,10);Assert.IsFalse(player.TryUseBoost());
   float boostStart=Time.fixedTime; yield return Wait(()=>Time.fixedTime>=boostStart+.28f,"boost physics clock"); Assert.IsFalse(player.IsBoosting, "Boost duration="+typeof(PlayerController).GetField("boostDuration",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(player)+", remaining="+typeof(PlayerController).GetField("boostRemaining",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(player)+", elapsed="+(Time.fixedTime-boostStart));
   Assert.AreEqual(5,player.GetComponent<Rigidbody2D>().linearVelocity.x,.05f);
   yield return Wait(()=>player.IsGrounded,"shortcut landing");Assert.AreEqual(target,player.GroundPlatform);
   player.ResetPlayer(new Vector3(10.2f,.75f,0));Assert.AreEqual(0,player.Combo.Count);Assert.IsFalse(player.Combo.BoostReady);
  }
  [UnityTearDown] public IEnumerator Cleanup(){if(EditorApplication.isPlaying)yield return new ExitPlayMode();Time.timeScale=1;}
 }
}
