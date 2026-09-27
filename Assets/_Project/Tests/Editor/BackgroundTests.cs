using System.Collections;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace CloudHop.Tests
{
    public sealed class BackgroundTests
    {
        [UnityTest]
        public IEnumerator LayersScrollAtDifferentSpeedsAndReturnAfterTeleport()
        {
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/CloudHopGimmickLab.unity");
            yield return new EnterPlayMode();
            var camera=Camera.main;
            camera.GetComponent<FollowCamera>().enabled=false;
            var layers=Object.FindObjectsByType<BackgroundLayer>(FindObjectsSortMode.None);
            Assert.AreEqual(6,layers.Length);
            var origin=camera.transform.position;
            foreach(var layer in layers)
            {
                Assert.AreEqual(0,layer.GetComponentsInChildren<Collider2D>().Length);
                foreach(var sprite in layer.GetComponentsInChildren<SpriteRenderer>())
                    Assert.Less(sprite.sortingOrder,0);
                camera.transform.position=origin;
                layer.Refresh();
                float first=layer.transform.GetChild(3).position.x;
                camera.transform.position=origin+Vector3.right*.1f;
                layer.Refresh();
                Assert.AreEqual(.1f*(1-layer.ScrollSpeed),
                    layer.transform.GetChild(3).position.x-first,.001f);
                camera.transform.position=origin+Vector3.right*230;
                layer.Refresh();
                float nearest=float.MaxValue;
                foreach(Transform tile in layer.transform)
                    nearest=Mathf.Min(nearest,Mathf.Abs(tile.position.x-camera.transform.position.x));
                Assert.Less(nearest,24);
                camera.transform.position=origin;
                layer.Refresh();
                Assert.AreEqual(first,layer.transform.GetChild(3).position.x,.001f);
            }
            yield return new ExitPlayMode();
        }
        [UnityTearDown]
        public IEnumerator Restore()
        {
            if(UnityEditor.EditorApplication.isPlaying)yield return new ExitPlayMode();
        }
    }
}
