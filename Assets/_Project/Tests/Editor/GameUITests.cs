using System.Collections;
using System.IO;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace CloudHop.Tests
{
    public sealed class GameUITests
    {
        [UnityTest]
        public IEnumerator LobbyGameplayPauseResultsAndRetryWork()
        {
            SessionState.SetInt("UITest.Character",PlayerPrefs.GetInt("CloudHop.Character",0));
            SessionState.SetBool("UITest.HadCharacter",PlayerPrefs.HasKey("CloudHop.Character"));
            SessionState.SetBool("UITest.HasBest",PlayerPrefs.HasKey("CloudHop.BestScore.v1"));
            SessionState.SetInt("UITest.Best",PlayerPrefs.GetInt("CloudHop.BestScore.v1"));
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/CloudHop.unity");
            yield return new EnterPlayMode();
            yield return null;yield return null;
            var ui=Object.FindFirstObjectByType<GameUI>();
            var player=Object.FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
            var stages=Object.FindFirstObjectByType<StageDirector>();
            var game=Object.FindFirstObjectByType<GameManager>();
            float startDeadline=Time.realtimeSinceStartup+3;
            while((!ui.IsLobby || Time.timeScale!=0) && Time.realtimeSinceStartup<startDeadline)yield return null;
            Assert.IsTrue(ui.IsLobby);Assert.AreEqual(0,Time.timeScale);
            Assert.IsFalse(player.GetComponent<ChargeInput>().enabled);
            Capture("01-lobby");
            Click("Stages");Click("Stage 1");yield return null;
            Assert.IsTrue(ui.IsSelectingCharacter);
            var colliderSize=player.GetComponent<BoxCollider2D>().size;
            var uiAssets=AssetDatabase.LoadAssetAtPath<UIAssets>("Assets/_Project/ScriptableObjects/GameUIAssets.asset");
            for(int i=0;i<3;i++)
            {
                Click("Character "+i);yield return null;
                Capture("character-"+i);
                Click("Character Play");yield return new WaitForSeconds(.3f);
                Assert.AreEqual(i,ui.SelectedCharacter);
                Assert.AreEqual(colliderSize,player.GetComponent<BoxCollider2D>().size);
                var skin=uiAssets.characters[i];
                CollectionAssert.Contains(new[]{skin.sprite,skin.chargeSprite,skin.jumpSprite,skin.fallSprite,skin.landingSprite},player.GetComponentInChildren<SpriteRenderer>().sprite);
                Click("Pause");Click("Pause Home");Click("Stages");Click("Stage 1");yield return null;
            }
            Click("Character Back");
            Click("Stages");yield return null;
            Capture("02-stage-select");
            Click("Stage 2");yield return null; Click("Character Play");yield return new WaitForSeconds(.4f);
            Assert.AreEqual(1,stages.CurrentIndex);Assert.AreEqual(1,Time.timeScale);
            Assert.IsTrue(player.GetComponent<ChargeInput>().enabled);
            Capture("03-gameplay");
            Click("Pause");yield return null;
            Assert.IsTrue(ui.IsPaused);Assert.AreEqual(0,Time.timeScale);
            Capture("04-pause");
            Capture("pause-1920x1080",1920,1080);
            Capture("pause-2560x1080",2560,1080);
            Capture("pause-1024x768",1024,768);
            Click("Pause Settings");yield return null;
            Capture("05-settings");
            Assert.IsNotNull(GameObject.Find("Volume Slider").GetComponent<Slider>());
            Click("Settings Back");Click("Resume");
            Assert.AreEqual(1,Time.timeScale);
            player.GetComponent<Rigidbody2D>().position=new Vector2(0,-8);
            float deadline=Time.realtimeSinceStartup+3;
            while(game.State!=GameState.GameOver && Time.realtimeSinceStartup<deadline)yield return null;
            Assert.AreEqual(GameState.GameOver,game.State);
            yield return null;Capture("06-game-over");
            Click("Retry");yield return new WaitForSeconds(.4f);
            Assert.AreEqual(GameState.Ready,game.State);Assert.AreEqual(1,stages.CurrentIndex);
            var retrySkin = uiAssets.characters[2];
            CollectionAssert.Contains(new[]{retrySkin.sprite,retrySkin.chargeSprite,retrySkin.jumpSprite,retrySkin.fallSprite,retrySkin.landingSprite},player.GetComponentInChildren<SpriteRenderer>().sprite);
            var final=stages.CurrentCourse.Platforms[stages.CurrentCourse.JumpCount].GetComponent<BoxCollider2D>().bounds;
            player.ResetPlayer(new Vector3(final.center.x,final.max.y+.48f,0));
            deadline=Time.realtimeSinceStartup+3;
            while(game.State!=GameState.StageClear && Time.realtimeSinceStartup<deadline)yield return null;
            Assert.AreEqual(GameState.StageClear,game.State);
            yield return null;Capture("07-clear");
            Click("Next");yield return new WaitForSeconds(.3f);
            Assert.AreEqual(2,stages.CurrentIndex);
            Click("Pause");Click("Pause Home");
            startDeadline=Time.realtimeSinceStartup+3;
            while((!ui.IsLobby || Time.timeScale!=0) && Time.realtimeSinceStartup<startDeadline)yield return null;
            Assert.IsTrue(ui.IsLobby);Assert.AreEqual(0,Time.timeScale);
        }
        [UnityTearDown]
        public IEnumerator Restore()
        {
            if(EditorApplication.isPlaying)yield return new ExitPlayMode();
            Time.timeScale=1;
            if(SessionState.GetBool("UITest.HasBest",false))PlayerPrefs.SetInt("CloudHop.BestScore.v1",SessionState.GetInt("UITest.Best",0));
            else PlayerPrefs.DeleteKey("CloudHop.BestScore.v1");
            if(SessionState.GetBool("UITest.HadCharacter",false))PlayerPrefs.SetInt("CloudHop.Character",SessionState.GetInt("UITest.Character",0));
            else PlayerPrefs.DeleteKey("CloudHop.Character");
            PlayerPrefs.Save();
        }
        private static void Click(string name)=>GameObject.Find(name).GetComponent<Button>().onClick.Invoke();
        internal static void Capture(string name,int width=1280,int height=800)
        {
            if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
            var camera=Camera.main;
            var canvas=Object.FindFirstObjectByType<Canvas>();
            var target=new RenderTexture(width,height,24);
            target.Create();camera.targetTexture=target;
            canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            Canvas.ForceUpdateCanvases();
            foreach(var label in canvas.GetComponentsInChildren<TMP_Text>())
            {
                Assert.IsNotNull(label.font, "Every UI label must have its bundled font.");
                label.ForceMeshUpdate();
                Assert.IsFalse(label.isTextOverflowing, "Text overflow: "+label.text);
            }
            foreach(RectTransform screen in canvas.transform)
            {
                if(!screen.gameObject.activeSelf || screen.GetComponent<Image>()==null)continue;
                var parentCorners=new Vector3[4];var screenCorners=new Vector3[4];
                ((RectTransform)canvas.transform).GetWorldCorners(parentCorners);screen.GetWorldCorners(screenCorners);
                for(int i=0;i<4;i++)Assert.Less(Vector3.Distance(parentCorners[i],screenCorners[i]),.01f,"Modal must cover the entire canvas: "+screen.name);
            }
            camera.Render();
            var previous=RenderTexture.active;RenderTexture.active=target;
            var image=new Texture2D(width,height,TextureFormat.RGB24,false);
            image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
            Directory.CreateDirectory("UIPreview");
            File.WriteAllBytes("UIPreview/"+name+".png",image.EncodeToPNG());
            RenderTexture.active=previous;camera.targetTexture=null;
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            target.Release();Object.Destroy(image);Object.Destroy(target);
        }
    }
}
