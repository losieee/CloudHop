using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace CloudHop.Tests
{
    public sealed class GimmickCaptureTests
    {
        [UnityTest]
        public IEnumerator CaptureReadableGimmickStates()
        {
            var args = Environment.GetCommandLineArgs();
            int option = Array.IndexOf(args,"-gimmickCaptureDir");
            if (option < 0) Assert.Ignore("Run explicitly with -gimmickCaptureDir and a graphics device.");
            SessionState.SetString("GimmickCapture.Folder",args[option+1]);
            Directory.CreateDirectory(args[option+1]);
            SessionState.SetBool("GimmickCapture.HadBest",PlayerPrefs.HasKey("CloudHop.BestScore.v1"));
            SessionState.SetInt("GimmickCapture.Best",PlayerPrefs.GetInt("CloudHop.BestScore.v1"));
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/CloudHopGimmickLab.unity");
            yield return new EnterPlayMode();
            string folder = SessionState.GetString("GimmickCapture.Folder","");
            Assert.AreNotEqual(UnityEngine.Rendering.GraphicsDeviceType.Null,SystemInfo.graphicsDeviceType);
            Object.FindFirstObjectByType<ChargeInput>().enabled = false;
            var game = Object.FindFirstObjectByType<GameManager>();
            var player = Object.FindFirstObjectByType<PlayerController>();
            var stages = Object.FindFirstObjectByType<StageDirector>();
            Camera.main.GetComponent<FollowCamera>().enabled = false;
            yield return Wait(0.25f);
            Camera.main.transform.position = new Vector3(9,2,-10);
            Capture(folder,"01-bird-wind.png");
            game.SelectStage(1);
            yield return Wait(0.3f);
            var cloud = stages.CurrentCourse.GetComponentInChildren<CrumblingPlatform>();
            player.ResetPlayer(cloud.transform.position+Vector3.up*0.8f);
            Camera.main.transform.position = new Vector3(13,2,-10);
            yield return Wait(0.5f);
            Capture(folder,"02-crumble-moving.png");
            game.SelectStage(2);
            Camera.main.transform.position = new Vector3(8,2,-10);
            var bolt = stages.CurrentCourse.GetComponentInChildren<LightningZone>();
            while (bolt.Phase != HazardPhase.Warning) yield return null;
            yield return Wait(0.1f);
            Capture(folder,"03-lightning-warning.png");
            while (bolt.Phase != HazardPhase.Active) yield return null;
            yield return Wait(0.06f);
            Capture(folder,"04-lightning-active.png");
        }
        [UnityTearDown]
        public IEnumerator Restore()
        {
            if (EditorApplication.isPlaying) yield return new ExitPlayMode();
            if (string.IsNullOrEmpty(SessionState.GetString("GimmickCapture.Folder",""))) yield break;
            if (SessionState.GetBool("GimmickCapture.HadBest",false)) PlayerPrefs.SetInt("CloudHop.BestScore.v1",SessionState.GetInt("GimmickCapture.Best",0));
            else PlayerPrefs.DeleteKey("CloudHop.BestScore.v1");
            PlayerPrefs.Save();
            SessionState.EraseBool("GimmickCapture.HadBest"); SessionState.EraseInt("GimmickCapture.Best");
            SessionState.EraseString("GimmickCapture.Folder");
        }
        private static void Capture(string folder,string name)
        {
            var camera = Camera.main;
            var canvas = Object.FindFirstObjectByType<Canvas>();
            var target = new RenderTexture(1280,800,24);
            target.Create();
            camera.targetTexture = target;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1;
            Canvas.ForceUpdateCanvases();
            camera.Render();
            var previous = RenderTexture.active;
            RenderTexture.active = target;
            var texture = new Texture2D(1280,800,TextureFormat.RGB24,false);
            texture.ReadPixels(new Rect(0,0,1280,800),0,0);
            texture.Apply();
            File.WriteAllBytes(Path.Combine(folder,name),texture.EncodeToPNG());
            RenderTexture.active = previous;
            camera.targetTexture = null;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            target.Release();
            Object.Destroy(texture); Object.Destroy(target);
        }
        private static IEnumerator Wait(float seconds)
        {
            float until = Time.time+seconds;
            while (Time.time < until) yield return null;
        }
    }
}
