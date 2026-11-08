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
        [UnityTest]
        public IEnumerator FirstStageUsesDistantCastle()
        {
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/CloudHop.unity");
            yield return new EnterPlayMode();
            var layer = GameObject.Find("02 Distant Castle").GetComponent<BackgroundLayer>();
            var stages = Object.FindFirstObjectByType<StageDirector>();
            var original = layer.transform.localScale;
            var view = Camera.main.transform;
            stages.ActivateStage(1);
            view.position = stages.CurrentCourse.SpawnPosition;
            layer.Refresh();
            Assert.AreEqual(original.x * 5.8f, layer.transform.localScale.x, 0.001f);
            stages.ActivateStage(0);
            layer.Refresh();
            Assert.AreEqual(original.x * 0.43f, layer.transform.localScale.x, 0.001f);
            var camera = Camera.main.transform;
            float relativeX = layer.transform.GetChild(3).position.x - camera.position.x;
            camera.position += Vector3.right;
            layer.Refresh();
            Assert.AreEqual(relativeX - 0.018f, layer.transform.GetChild(3).position.x - camera.position.x, 0.001f);
            stages.ActivateStage(1);
            layer.Refresh();
            view.position = stages.CurrentCourse.Platforms[stages.CurrentCourse.JumpCount].transform.position;
            layer.Refresh();
            Assert.AreEqual(original.x * 6.6f, layer.transform.localScale.x, 0.001f);
            stages.ActivateStage(2);
            var wall = stages.CurrentCourse.transform.Find("Gallery Wall").GetComponent<SpriteRenderer>();
            Assert.IsNotNull(wall.sprite);
            Assert.Greater(wall.bounds.size.y, Camera.main.orthographicSize * 2);
            stages.ActivateStage(0);
            Assert.IsFalse(wall.gameObject.activeInHierarchy);
            yield return new ExitPlayMode();
        }

        [UnityTearDown]
        public IEnumerator Restore()
        {
            if(UnityEditor.EditorApplication.isPlaying)yield return new ExitPlayMode();
        }
    }
}
