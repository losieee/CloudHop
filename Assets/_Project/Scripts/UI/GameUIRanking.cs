using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CloudHop
{
    // Exhibition UI; timing rules and persistence live in separate, testable classes.
    public sealed partial class GameUI
    {
        private TimedRun run;
        private readonly LocalRankingStore rankings=new LocalRankingStore();
        private GameObject nameEntry,rankingScreen;
        private TMP_InputField nicknameInput;
        private TMP_Text nameError,runLabel,rankingHeading;
        private TMP_Text[] rankNames,rankTimes;
        private string pendingNickname;
        private int finalRank;
        public TimedRun CurrentRun => run;

        private void ShowNameEntry()
        {
            run.Cancel();pendingNickname=null;
            Freeze();HideScreens();nameEntry.SetActive(true);
            nicknameInput.text="";nameError.text="";
            nicknameInput.Select();nicknameInput.ActivateInputField();
        }
        private void ConfirmNickname()
        {
            if(!LocalRankingStore.TryNickname(nicknameInput.text,out var valid))
            {
                nameError.text=UIText.Get("name.error");
                return;
            }
            pendingNickname=valid;
            nicknameInput.DeactivateInputField();
            ShowCharacters(0);
        }
        private void BeginCharacterPlay()
        {
            if(pendingNickname!=null)
            {
                run.Begin(pendingNickname);pendingNickname=null;finalRank=0;
            }
            StartStage(pendingStage);
        }
        private void UpdateRunUI()
        {
            runLabel.text=run.Active || run.Completed ? UIText.Get("hud.run",run.Nickname,TimedRun.Format(run.Seconds)) : UIText.Get("hud.practice");
        }
        private void RecordStageClear(GameState state)
        {
            if(state==GameState.StageClear && run.ClearStage(stages.CurrentIndex))
                finalRank=rankings.Register(run);
        }
        private void RenderRunResult()
        {
            var retry=result.transform.Find("Retry").gameObject;
            retry.SetActive(true);
            retry.GetComponentInChildren<TMP_Text>().text=run.Completed ? UIText.Get("result.new_run") : UIText.Get("common.retry");
            resultScore.richText=false;
            if(run.Active || run.Completed)
                resultScore.text=UIText.Get("result.run",run.Nickname,TimedRun.Format(run.Seconds),
                    run.Completed ? finalRank>0 ? UIText.Get("result.rank",finalRank) : UIText.Get("result.outside_top") : UIText.Get("result.retry_hint"));
        }
        private void RetryOrNewRun()
        {
            if(run.Completed){ShowLobby();ShowNameEntry();}
            else StartStage(stages.CurrentIndex);
        }
        private void ContinueFromResult()
        {
            if(run.Completed){ShowRankings();return;}
            if(stages.IsLastStage)ShowLobby();else StartStage(stages.CurrentIndex+1);
        }
        private void ShowRankings()
        {
            Freeze();HideScreens();rankingScreen.SetActive(true);
            var entries=rankings.Load();
            rankingHeading.text=UIText.Get("ranking.title");
            for(int i=0;i<10;i++)
            {
                rankNames[i].text=i<entries.Count ? entries[i].nickname : UIText.Get("ranking.empty_name");
                rankTimes[i].text=i<entries.Count ? TimedRun.Format(entries[i].milliseconds/1000.0) : UIText.Get("ranking.empty_time");
                bool highlight=i<entries.Count && run.Completed && entries[i].id==run.Id;
                rankNames[i].color=rankTimes[i].color=highlight ? new Color(.85f,.43f,.02f) : Ink;
            }
        }
        private void BuildRankingUI()
        {
            Button(lobby.transform,"Ranking",UIText.Get("lobby.ranking"),new Vector2(260,-255),new Vector2(370,84),"UI_Large_White",ShowRankings);
            runLabel=Label(hud.transform,"",new Vector2(0,265),new Vector2(600,45),24,Color.white);
            runLabel.richText=false;
            nameEntry=Screen("Nickname Entry",true);
            Picture(nameEntry.transform,"UI_Banner_Cream_Blank",new Vector2(0,235),new Vector2(620,125));
            Label(nameEntry.transform,UIText.Get("name.title"),new Vector2(0,235),new Vector2(520,65),32,Ink);
            Label(nameEntry.transform,UIText.Get("name.subtitle"),new Vector2(0,130),new Vector2(800,45),26,Color.white);
            var field=Box(nameEntry.transform,"Nickname",new Vector2(0,35),new Vector2(530,75),Color.white);
            field.GetComponent<Image>().raycastTarget=true;
            nicknameInput=field.gameObject.AddComponent<TMP_InputField>();
            var viewport=Rect(field,"Viewport",Vector2.zero,new Vector2(490,65));
            viewport.gameObject.AddComponent<RectMask2D>();
            var text=Label(viewport,"",Vector2.zero,new Vector2(480,60),30,Ink);
            text.font=font;text.fontSharedMaterial=text.font.material;
            text.richText=false;text.enableAutoSizing=false;
            text.alignment=TextAlignmentOptions.MidlineLeft;
            nicknameInput.textViewport=viewport;
            nicknameInput.textComponent=(TextMeshProUGUI)text;
            nicknameInput.fontAsset=text.font;
            nicknameInput.targetGraphic=field.GetComponent<Image>();
            nicknameInput.lineType=TMP_InputField.LineType.SingleLine;
            // Validate on confirmation so Korean IME composition is not interrupted.
            nicknameInput.characterLimit=24;
            nicknameInput.richText=false;
            nameError=Label(nameEntry.transform,"",new Vector2(0,-30),new Vector2(960,38),19,Color.white);
            Label(nameEntry.transform,UIText.Get("name.rules"),new Vector2(0,-115),new Vector2(1000,120),22,Color.white);
            Button(nameEntry.transform,"Name Back",UIText.Get("common.back"),new Vector2(-180,-260),new Vector2(260,75),"UI_Medium_Cream",ShowLobby);
            Button(nameEntry.transform,"Name Next",UIText.Get("common.next"),new Vector2(180,-260),new Vector2(300,75),"UI_Large_Cyan",ConfirmNickname);

            rankingScreen=Screen("Local Ranking",true);
            Picture(rankingScreen.transform,"UI_Banner_Cream_Blank",new Vector2(0,310),new Vector2(620,110));
            rankingHeading=Label(rankingScreen.transform,UIText.Get("ranking.title"),new Vector2(0,310),new Vector2(530,65),32,Ink);
            Box(rankingScreen.transform,"Ranking Panel",new Vector2(0,5),new Vector2(830,480),new Color(.95f,.98f,1,.96f));
            Label(rankingScreen.transform,UIText.Get("ranking.rank"),new Vector2(-340,220),new Vector2(100,35),20,Ink);
            Label(rankingScreen.transform,UIText.Get("ranking.nickname"),new Vector2(-90,220),new Vector2(380,35),20,Ink);
            Label(rankingScreen.transform,UIText.Get("ranking.time"),new Vector2(265,220),new Vector2(250,35),20,Ink);
            rankNames=new TMP_Text[10];rankTimes=new TMP_Text[10];
            for(int i=0;i<10;i++)
            {
                float y=175-i*40;
                Label(rankingScreen.transform,UIText.Get("ranking.number",i+1),new Vector2(-340,y),new Vector2(80,35),24,Ink);
                rankNames[i]=Label(rankingScreen.transform,"",new Vector2(-90,y),new Vector2(380,35),24,Ink);
                rankNames[i].richText=false;
                rankTimes[i]=Label(rankingScreen.transform,"",new Vector2(265,y),new Vector2(250,35),24,Ink);
            }
            Label(rankingScreen.transform,UIText.Get("ranking.note"),new Vector2(0,-260),new Vector2(1000,35),19,Color.white);
            Button(rankingScreen.transform,"Ranking Home",UIText.Get("common.home"),new Vector2(0,-325),new Vector2(300,70),"UI_Large_Cyan",ShowLobby);
        }
    }
}
