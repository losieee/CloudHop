using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.EventSystems;

namespace CloudHop.Tests
{
    public sealed class ExhibitionUITests
    {
        private Keyboard menuKeyboard;
        private InputSettings.BackgroundBehavior backgroundBefore;
        private InputSettings.EditorInputBehaviorInPlayMode routingBefore;
        private IEnumerator Press(Key key)
        {
            InputSystem.QueueStateEvent(menuKeyboard, new KeyboardState(key));
            yield return null;
            yield return null;
            InputSystem.QueueStateEvent(menuKeyboard, new KeyboardState());
            yield return null;
            yield return null;
        }
        private static readonly string[] Keys={LocalRankingStore.DefaultKey,LocalRankingStore.DefaultKey+".backup"};
        [UnityTest] public IEnumerator FullExhibitionRunRegistersOnlyAfterThreeStages()
        {
            foreach(string key in Keys)
            {
                SessionState.SetBool("ExTest.Has."+key,PlayerPrefs.HasKey(key));
                SessionState.SetString("ExTest.Value."+key,PlayerPrefs.GetString(key,""));
                PlayerPrefs.DeleteKey(key);
            }
            SessionState.SetInt("ExTest.Character",PlayerPrefs.GetInt("CloudHop.Character",0));
            SessionState.SetBool("ExTest.HasCharacter",PlayerPrefs.HasKey("CloudHop.Character"));
            SessionState.SetInt("ExTest.Best",PlayerPrefs.GetInt("CloudHop.BestScore.v1",0));
            SessionState.SetBool("ExTest.HasBest",PlayerPrefs.HasKey("CloudHop.BestScore.v1"));
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/CloudHop.unity");
            yield return new EnterPlayMode();yield return null;yield return null;
            var ui=Object.FindFirstObjectByType<GameUI>();
            var game=Object.FindFirstObjectByType<GameManager>();
            var stages=Object.FindFirstObjectByType<StageDirector>();
            var player=Object.FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
            backgroundBefore = InputSystem.settings.backgroundBehavior;
            routingBefore = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            menuKeyboard = InputSystem.AddDevice<Keyboard>();
            yield return null;
            Assert.AreEqual("Start", EventSystem.current.currentSelectedGameObject.name);
            yield return Press(Key.Enter);
            var field=GameObject.Find("Nickname").GetComponent<TMP_InputField>();
            field.text="   ";Click("Name Next");Assert.IsFalse(ui.IsSelectingCharacter);
            field.text="구름여우1";
            yield return null;GameUITests.Capture("08-nickname");
            yield return Press(Key.Enter);Assert.IsTrue(ui.IsSelectingCharacter);Assert.IsFalse(ui.CurrentRun.Active);
            yield return Press(Key.RightArrow);
            yield return Press(Key.RightArrow);
            Assert.AreEqual("Character 2", EventSystem.current.currentSelectedGameObject.name);
            yield return Press(Key.Enter);
            Assert.AreEqual(2,ui.SelectedCharacter);
            yield return Press(Key.DownArrow);
            Assert.AreEqual("Character Play", EventSystem.current.currentSelectedGameObject.name);
            yield return Press(Key.Enter);
            yield return new WaitForSeconds(.4f);
            Assert.IsTrue(ui.CurrentRun.Active);
            Click("Pause");double pausedAt=ui.CurrentRun.Seconds;
            yield return new WaitForSecondsRealtime(.2f);Assert.Greater(ui.CurrentRun.Seconds,pausedAt+.15);Click("Resume");
            player.GetComponent<Rigidbody2D>().position=new Vector2(0,-9);
            yield return WaitFor(game,GameState.GameOver);
            double beforeRetry=ui.CurrentRun.Seconds;Click("Retry");yield return new WaitForSeconds(.2f);
            Assert.Greater(ui.CurrentRun.Seconds,beforeRetry);
            for(int i=0;i<3;i++)
            {
                Assert.AreEqual(i,stages.CurrentIndex);
                var final=stages.CurrentCourse.Platforms[stages.CurrentCourse.JumpCount].GetComponent<BoxCollider2D>().bounds;
                player.ResetPlayer(new Vector3(final.center.x,final.max.y+.48f,0));
                yield return WaitFor(game,GameState.StageClear);
                if(i==1)yield return new WaitForSecondsRealtime(1.15f);
                if(i<2){Assert.AreEqual(0,new LocalRankingStore().Load().Count);Click("Next");yield return new WaitForSeconds(.25f);}
            }
            Assert.IsTrue(ui.CurrentRun.Completed);
            Assert.AreEqual(1,new LocalRankingStore().Load().Count);
            Assert.AreEqual("구름여우1",new LocalRankingStore().Load()[0].nickname);
            double completedAt=ui.CurrentRun.Seconds;yield return new WaitForSecondsRealtime(.2f);Assert.AreEqual(completedAt,ui.CurrentRun.Seconds);
            GameUITests.Capture("09-ranked-clear");
            Assert.AreEqual("새 도전",GameObject.Find("Retry").GetComponentInChildren<TMP_Text>().text);
            Click("Next");yield return null;GameUITests.Capture("10-ranking");
            Click("Ranking Home");Assert.IsTrue(ui.IsLobby);
            Click("Start");Assert.AreEqual("",GameObject.Find("Nickname").GetComponent<TMP_InputField>().text);
            Click("Name Back");Click("Stages");Click("Stage 3");Click("Character Play");yield return new WaitForSeconds(.3f);
            Assert.IsFalse(ui.CurrentRun.Active);Assert.AreEqual(1,new LocalRankingStore().Load().Count);
        }
        private static void Click(string name)=>GameObject.Find(name).GetComponent<Button>().onClick.Invoke();
        private static IEnumerator WaitFor(GameManager game,GameState expected)
        {
            float deadline=Time.realtimeSinceStartup+4;
            while(game.State!=expected && Time.realtimeSinceStartup<deadline)yield return null;
            Assert.AreEqual(expected,game.State);yield return null;
        }
        [UnityTearDown] public IEnumerator Restore()
        {
            if(menuKeyboard != null && menuKeyboard.added)
            {
                InputSystem.RemoveDevice(menuKeyboard);
                InputSystem.settings.backgroundBehavior = backgroundBefore;
                InputSystem.settings.editorInputBehaviorInPlayMode = routingBefore;
            }
            if(EditorApplication.isPlaying)yield return new ExitPlayMode();
            if(SessionState.GetBool("ExTest.HasCharacter",false))PlayerPrefs.SetInt("CloudHop.Character",SessionState.GetInt("ExTest.Character",0));else PlayerPrefs.DeleteKey("CloudHop.Character");
            foreach(string key in Keys)
                if(SessionState.GetBool("ExTest.Has."+key,false))PlayerPrefs.SetString(key,SessionState.GetString("ExTest.Value."+key,""));else PlayerPrefs.DeleteKey(key);
            if(SessionState.GetBool("ExTest.HasBest",false))PlayerPrefs.SetInt("CloudHop.BestScore.v1",SessionState.GetInt("ExTest.Best",0));else PlayerPrefs.DeleteKey("CloudHop.BestScore.v1");
            PlayerPrefs.Save();Time.timeScale=1;
        }
    }
}
