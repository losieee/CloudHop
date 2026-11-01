using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
namespace CloudHop.Tests
{
 public sealed class PlatformEdgeTests
 {
  [Test] public void AllSharedStageEdgesHaveAnOrdinaryJumpExit()
  {
   EditorSceneManager.OpenScene("Assets/_Project/Scenes/CloudHop.unity");
   var courses=Object.FindObjectsByType<StageCourse>(FindObjectsInactive.Include,FindObjectsSortMode.None);
   Assert.AreEqual(3,courses.Length);
   foreach(var zone in Object.FindObjectsByType<PerfectLandingZone>(FindObjectsInactive.Include,FindObjectsSortMode.None))Object.DestroyImmediate(zone);
   foreach(var course in courses)course.gameObject.SetActive(false);
   var mode=Physics2D.simulationMode;Physics2D.simulationMode=SimulationMode2D.Script;
   var go=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/Player.prefab"));
   var p=go.GetComponent<PlayerController>();
   var flags=BindingFlags.Instance|BindingFlags.NonPublic;
   typeof(PlayerController).GetMethod("Awake",flags).Invoke(p,null);
   var step=typeof(PlayerController).GetMethod("FixedUpdate",flags);
   var charge=typeof(PlayerController).GetField("chargeTime",flags);
   var failures=new System.Collections.Generic.List<string>();int checkedEdges=0;
   try {
    foreach(var course in courses) {
     course.gameObject.SetActive(true);
     for(int i=0;i<course.Platforms.Length-1;i++) {
      var from=course.Platforms[i];var to=course.Platforms[i+1];
      if(from.GetComponent<SlidingSlope>()!=null || to.GetComponent<SlidingSlope>()!=null)continue;
      var bounds=from.GetComponent<BoxCollider2D>().bounds;
      foreach(float offset in new[]{-bounds.extents.x+.08f,0,bounds.extents.x-.08f}) {
       bool reached=false;
       for(int attempt=0;attempt<=40&&!reached;attempt++) {
        p.ResetPlayer(new Vector3(bounds.center.x+offset,bounds.max.y+.46f,0));Physics2D.SyncTransforms();
        for(int k=0;k<8;k++){step.Invoke(p,null);Physics2D.Simulate(.02f);}
        if(!p.IsGrounded)continue;
        p.BeginCharge();charge.SetValue(p,.05f+.95f*attempt/40);p.ReleaseCharge();
        bool airborne=false;
        for(int k=0;k<110;k++) {
         step.Invoke(p,null);Physics2D.Simulate(.02f);
         if(!p.IsGrounded)airborne=true;
         if(airborne&&p.IsGrounded){reached=p.GroundPlatform==to;break;}
        }
       }
       checkedEdges++;
       if(!reached)failures.Add(course.name+" "+i+" -> "+(i+1)+" offset "+offset);
      }
     }
     course.gameObject.SetActive(false);
    }
    Debug.Log("EDGE_AUDIT: "+checkedEdges+" takeoff positions");
    Assert.IsEmpty(failures,string.Join("\n",failures));
   } finally {Object.DestroyImmediate(go);Physics2D.simulationMode=mode;}
  }
  [Test] public void RaisedEdgeDoesNotPermanentlyCancelForwardJump()
  {
   EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   var mode=Physics2D.simulationMode;Physics2D.simulationMode=SimulationMode2D.Script;
   var a=new GameObject("Lower platform");a.transform.position=new Vector3(0,-.3f,0);a.AddComponent<BoxCollider2D>().size=new Vector2(3,.6f);a.AddComponent<Platform>();
   var b=new GameObject("Raised platform");b.transform.position=new Vector3(3.3f,.6f,0);b.AddComponent<BoxCollider2D>().size=new Vector2(3,.6f);b.AddComponent<Platform>();
   var go=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/Player.prefab"));
   var p=go.GetComponent<PlayerController>();
   var flags=BindingFlags.Instance|BindingFlags.NonPublic;
   var awake=typeof(PlayerController).GetMethod("Awake",flags);awake.Invoke(p,null);
   var step=typeof(PlayerController).GetMethod("FixedUpdate",flags);
   try {
    p.ResetPlayer(new Vector3(1.45f,.46f,0));Physics2D.SyncTransforms();
    for(int i=0;i<10;i++){step.Invoke(p,null);Physics2D.Simulate(.02f);}
    Assert.IsTrue(p.IsGrounded);
    p.BeginCharge();typeof(PlayerController).GetField("chargeTime",flags).SetValue(p,1f);p.ReleaseCharge();
    for(int i=0;i<40;i++){step.Invoke(p,null);Physics2D.Simulate(.02f);}
    Assert.Greater(p.transform.position.x,2.5f,"Forward travel must recover after hitting the raised edge");
   } finally {Object.DestroyImmediate(go);Object.DestroyImmediate(a);Object.DestroyImmediate(b);Physics2D.simulationMode=mode;}
  }
 }
}
