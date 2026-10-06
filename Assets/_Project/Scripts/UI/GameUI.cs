using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CloudHop
{
    // Presentation and menu flow only. GameManager remains responsible for gameplay.
    [DefaultExecutionOrder(500)]
    public sealed partial class GameUI : MonoBehaviour
    {
        [SerializeField] private UIAssets assets;
        [SerializeField] private GameManager game;
        [SerializeField] private ScoreManager scores;
        [SerializeField] private PlayerController player;
        [SerializeField] private StageDirector stages;
        private ChargeInput input;
        private TMP_FontAsset font;
        private Sprite whiteSprite;
        private RectTransform canvas;
        private GameObject lobby, selection, hud, pause, settings, result;
        private GameObject settingsReturn;
        private GameObject characterSelection;
        private Image lobbyHero;
        private TMP_Text[] characterChoices;
        private int selectedCharacter;
        private int pendingStage;
        public int SelectedCharacter => selectedCharacter;
        public bool IsSelectingCharacter => characterSelection != null && characterSelection.activeSelf;
        private TMP_Text scoreLabel, bestLabel, stageLabel, resultScore, progressLabel, audioLabel;
        private Image chargeFill, progressFill, resultBackground;
        private TMP_Text resultTitle;
        private Button continueButton;
        private bool playing;
        private bool muted;
        private float volume;
        private float previousTimeScale, previousVolume;
        public bool IsLobby => lobby != null && lobby.activeSelf;
        public bool IsPaused => pause != null && pause.activeSelf;

        private void Awake()
        {
            previousTimeScale=Time.timeScale;
            previousVolume=AudioListener.volume;
            input=player.GetComponent<ChargeInput>();
            font=assets.uiFont.fallbackFontAssetTable[0];
            selectedCharacter=Mathf.Clamp(PlayerPrefs.GetInt("CloudHop.Character",0),0,assets.characters.Length-1);
            volume=PlayerPrefs.GetFloat("CloudHop.Audio.Volume",1);
            muted=PlayerPrefs.GetInt("CloudHop.Audio.Muted",0)==1;
            whiteSprite=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),new Vector2(.5f,.5f));
            run=new TimedRun(()=>Time.realtimeSinceStartupAsDouble);
            Build();
            BuildRankingUI();
            BuildComboUI();
            HideScreens();
            ApplyAudio();
        }
        private void Start()
        {
            game.StateChanged+=StateChanged;
            ShowLobby();
        }
        private void OnDestroy()
        {
            DisposeComboUI();
            if(game!=null)game.StateChanged-=StateChanged;
            if(whiteSprite!=null)Destroy(whiteSprite);
            Time.timeScale=previousTimeScale;
            AudioListener.volume=previousVolume;
        }

        private void Update()
        {
            UpdateRunUI();
            UpdateComboUI();
            scoreLabel.text=UIText.Get("hud.score",scores.Score);
            bestLabel.text=UIText.Get("hud.best",scores.Best);
            stageLabel.text=UIText.Get("hud.stage",stages.CurrentIndex+1,stages.StageCount);
            chargeFill.fillAmount=player.IsCharging?player.Charge01:0;
            progressFill.fillAmount=(float)stages.Progress/stages.CurrentCourse.JumpCount;
            progressLabel.text=UIText.Get("hud.progress",stages.Progress,stages.CurrentCourse.JumpCount);
            if(Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if(settings.activeSelf)CloseSettings();
                else if(pause.activeSelf)Resume();
                else if(selection.activeSelf || characterSelection.activeSelf || nameEntry.activeSelf || rankingScreen.activeSelf)ShowLobby();
                else if(playing && !result.activeSelf)Pause();
            }
        }
        private void HideScreens()
        {
            foreach(var screen in new[]{lobby,selection,hud,pause,settings,result,characterSelection,nameEntry,rankingScreen})screen.SetActive(false);
        }
        private void Freeze()
        {
            input.enabled=false;
            player.CancelCharge();
            Time.timeScale=0;
        }
        public void ShowLobby()
        {
            run.Cancel();
            pendingNickname=null;
            playing=false;
            Freeze();
            player.gameObject.SetActive(false);
            foreach(var course in FindObjectsByType<StageCourse>(FindObjectsInactive.Include,FindObjectsSortMode.None))course.gameObject.SetActive(false);
            HideScreens();
            lobby.SetActive(true);
        }
        public void ShowStages()
        {
            run.Cancel(); pendingNickname=null;
            Freeze();
            HideScreens();
            selection.SetActive(true);
        }
        public void ShowCharacters(int stageIndex)
        {
            pendingStage=stageIndex;
            Freeze();
            HideScreens();
            characterSelection.SetActive(true);
            SelectCharacter(selectedCharacter);
        }
        public void SelectCharacter(int index)
        {
            selectedCharacter=Mathf.Clamp(index,0,assets.characters.Length-1);
            PlayerPrefs.SetInt("CloudHop.Character",selectedCharacter);
            PlayerPrefs.Save();
            lobbyHero.sprite=assets.characters[selectedCharacter].sprite;
            for(int i=0;i<characterChoices.Length;i++)
                characterChoices[i].text=i==selectedCharacter ? UIText.Get("character.selected") : UIText.Get("character.select");
        }
        public void StartStage(int index)
        {
            HideScreens();
            player.gameObject.SetActive(true);
            player.GetComponentInChildren<CharacterVisual>(true).Apply(assets.characters[selectedCharacter]);
            game.SelectStage(index);
            playing=true;
            Time.timeScale=1;
            input.enabled=true;
            input.ResetGesture();
            hud.SetActive(true);
        }
        public void Pause()
        {
            if(!playing || result.activeSelf)return;
            Freeze();
            pause.SetActive(true);
        }
        public void Resume()
        {
            pause.SetActive(false);
            Time.timeScale=1;
            input.enabled=true;
            input.ResetGesture();
        }
        private void ShowSettings(GameObject back)
        {
            settingsReturn=back;
            back.SetActive(false);
            settings.SetActive(true);
        }
        private void CloseSettings()
        {
            settings.SetActive(false);
            settingsReturn.SetActive(true);
        }
        private void ApplyAudio()
        {
            AudioListener.volume=muted?0:volume;
            if(audioLabel!=null)audioLabel.text=muted?UIText.Get("settings.muted"):UIText.Get("settings.sound_on");
        }
        private void SaveAudio()
        {
            PlayerPrefs.SetFloat("CloudHop.Audio.Volume",volume);
            PlayerPrefs.SetInt("CloudHop.Audio.Muted",muted?1:0);
            PlayerPrefs.Save();
            ApplyAudio();
        }
        private void StateChanged(GameState state)
        {
            if(!playing || (state!=GameState.GameOver && state!=GameState.StageClear))return;
            RecordStageClear(state);
            Freeze();
            pause.SetActive(false);
            settings.SetActive(false);
            result.SetActive(true);
            bool clear=state==GameState.StageClear;
            resultBackground.sprite=assets.Find(clear?"UI_Result_Clear":"UI_Result_GameOver");
            resultTitle.text=clear?UIText.Get("result.clear"):UIText.Get("result.over");
            resultScore.text=UIText.Get("result.score",scores.Score,scores.Best,
                clear && stages.IsLastStage?UIText.Get("result.all_clear"):"");
            continueButton.gameObject.SetActive(clear);
            continueButton.GetComponentInChildren<TMP_Text>().text=run.Completed?UIText.Get("common.rankings"):stages.IsLastStage?UIText.Get("common.home"):UIText.Get("common.next_stage");
            RenderRunResult();
        }

        private void Build()
        {
            var go=new GameObject("Cloud Hop UI",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            go.transform.SetParent(transform,false);
            go.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            go.GetComponent<Canvas>().sortingOrder=100;
            var scaler=go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1280,800);
            scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            canvas=go.GetComponent<RectTransform>();
            characterSelection=Screen("Character Selection",true);
            lobby=Screen("Lobby",false);
            selection=Screen("Stage Selection",true);
            hud=Screen("HUD",false);
            pause=Screen("Pause",true);
            settings=Screen("Settings",true);
            result=Screen("Results",true);

            Picture(lobby.transform,"UI_Banner_Cream_Blank",new Vector2(0,230),new Vector2(650,180));
            Label(lobby.transform,UIText.Get("lobby.title"),new Vector2(0,230),new Vector2(510,100),58,Ink);
            Picture(lobby.transform,"UI_Decoration_Cloud",new Vector2(-280,-180),new Vector2(360,140));
            var hero=Picture(lobby.transform,null,new Vector2(-280,-10),new Vector2(240,330));
            lobbyHero=hero;
            hero.sprite=assets.characters[selectedCharacter].sprite;
            Button(lobby.transform,"Start",UIText.Get("common.start"),new Vector2(260,30),new Vector2(370,84),"UI_Large_Cyan",ShowNameEntry);
            Button(lobby.transform,"Stages",UIText.Get("lobby.practice"),new Vector2(260,-65),new Vector2(370,84),"UI_Large_White",ShowStages);
            Button(lobby.transform,"Settings",UIText.Get("common.settings"),new Vector2(260,-160),new Vector2(370,84),"UI_Large_White",()=>ShowSettings(lobby));

            Picture(characterSelection.transform,"UI_Banner_Cream_Blank",new Vector2(0,290),new Vector2(650,125));
            Label(characterSelection.transform,UIText.Get("character.title"),new Vector2(0,290),new Vector2(550,70),32,Ink);
            characterChoices=new TMP_Text[assets.characters.Length];
            for(int i=0;i<assets.characters.Length;i++)
            {
                int index=i;
                float x=(i-1)*340;
                Picture(characterSelection.transform,"UI_Decoration_Cloud",new Vector2(x,-125),new Vector2(300,105));
                var portrait=Picture(characterSelection.transform,null,new Vector2(x,45),new Vector2(270,310));
                portrait.sprite=assets.characters[i].sprite;
                var pick=portrait.gameObject.AddComponent<Button>();
                portrait.raycastTarget=true;
                pick.onClick.AddListener(()=>SelectCharacter(index));
                var nav=pick.navigation;nav.mode=Navigation.Mode.None;pick.navigation=nav;
                Label(characterSelection.transform,UIText.Get("character.name."+i),new Vector2(x,-170),new Vector2(310,40),25,Color.white);
                var button=Button(characterSelection.transform,"Character "+i,UIText.Get("character.select"),new Vector2(x,-220),new Vector2(260,65),"UI_Large_White",()=>SelectCharacter(index));
                characterChoices[i]=button.GetComponentInChildren<TMP_Text>();
            }
            Button(characterSelection.transform,"Character Back",UIText.Get("common.back"),new Vector2(-180,-320),new Vector2(260,70),"UI_Medium_Cream",ShowLobby);
            Button(characterSelection.transform,"Character Play",UIText.Get("common.start"),new Vector2(180,-320),new Vector2(300,75),"UI_Large_Cyan",BeginCharacterPlay);

            Picture(selection.transform,"UI_Banner_Cream_Blank",new Vector2(0,265),new Vector2(650,150));
            Label(selection.transform,UIText.Get("stage.title"),new Vector2(0,265),new Vector2(500,70),36,Ink);
            string[] names={UIText.Get("stage.name.0"),UIText.Get("stage.name.1"),UIText.Get("stage.name.2")};
            string[] levels={UIText.Get("stage.level.0"),UIText.Get("stage.level.1"),UIText.Get("stage.level.2")};
            string[] cards={"UI_Card_Blue","UI_Card_Gold","UI_Card_Purple"};
            for(int i=0;i<3;i++)
            {
                int index=i;
                var card=Button(selection.transform,"Stage "+(i+1),"",new Vector2((i-1)*355,-15),new Vector2(310,370),cards[i],()=>ShowCharacters(index));
                Label(card.transform,UIText.Get("stage.number",i+1),new Vector2(0,117),new Vector2(250,55),40,Ink);
                var art=Picture(card.transform,null,new Vector2(0,10),new Vector2(245,180));
                art.sprite=assets.stageArt[i];
                Label(card.transform,names[i],new Vector2(0,-74),new Vector2(285,40),23,Ink);
                Label(selection.transform,levels[i],new Vector2((i-1)*355,-230),new Vector2(250,30),20,Color.white);
            }
            Button(selection.transform,"Back",UIText.Get("common.back"),new Vector2(0,-300),new Vector2(240,66),"UI_Medium_Cream",ShowLobby);

            Picture(hud.transform,"UI_Large_White",new Vector2(-440,320),new Vector2(330,85));
            scoreLabel=Label(hud.transform,"",new Vector2(-418,320),new Vector2(245,55),25,Color.white);
            Picture(hud.transform,"UI_Large_Cyan",new Vector2(305,320),new Vector2(330,85));
            bestLabel=Label(hud.transform,"",new Vector2(327,320),new Vector2(245,55),25,Color.white);
            stageLabel=Label(hud.transform,"",new Vector2(0,325),new Vector2(220,45),23,Color.white);
            Button(hud.transform,"Pause","",new Vector2(555,320),new Vector2(65,65),"UI_Pause_Cloud",Pause);
            Label(hud.transform,UIText.Get("hud.jump"),new Vector2(0,-260),new Vector2(350,35),20,Color.white);
            chargeFill=Meter(hud.transform,"Charge",new Vector2(0,-295),new Vector2(330,16),new Color(.15f,.9f,1));
            progressFill=Meter(hud.transform,"Progress",new Vector2(0,-345),new Vector2(520,8),new Color(1,.8f,.15f));
            progressLabel=Label(hud.transform,"",new Vector2(335,-345),new Vector2(120,30),18,Color.white);

            Picture(pause.transform,"UI_Banner_Cream_Blank",new Vector2(0,220),new Vector2(550,135));
            Label(pause.transform,UIText.Get("pause.title"),new Vector2(0,220),new Vector2(450,70),36,Ink);
            Button(pause.transform,"Resume",UIText.Get("pause.resume"),new Vector2(0,65),new Vector2(380,84),"UI_Large_Cyan",Resume);
            Button(pause.transform,"Pause Settings",UIText.Get("common.settings"),new Vector2(0,-40),new Vector2(380,84),"UI_Large_White",()=>ShowSettings(pause));
            Button(pause.transform,"Pause Home",UIText.Get("common.home"),new Vector2(0,-145),new Vector2(380,84),"UI_Large_White",ShowLobby);

            Picture(settings.transform,"UI_Banner_Cream_Blank",new Vector2(0,225),new Vector2(550,135));
            Label(settings.transform,UIText.Get("common.settings"),new Vector2(0,225),new Vector2(420,65),34,Ink);
            var sound=Button(settings.transform,"Sound","",new Vector2(0,60),new Vector2(370,84),"UI_Large_White",()=>{muted=!muted;SaveAudio();});
            audioLabel=Label(sound.transform,"",Vector2.zero,new Vector2(280,50),28,Ink);
            Label(settings.transform,UIText.Get("settings.volume"),new Vector2(0,-40),new Vector2(400,40),23,Color.white);
            var track=Box(settings.transform,"Volume Slider",new Vector2(0,-90),new Vector2(360,28),new Color(.08f,.2f,.38f));
            track.GetComponent<Image>().raycastTarget=true;
            var slider=track.gameObject.AddComponent<Slider>();
            var fill=Box(track,"Fill",Vector2.zero,new Vector2(360,28),new Color(.2f,.85f,1));
            fill.anchorMin=Vector2.zero;
            fill.anchorMax=Vector2.one;
            fill.sizeDelta=Vector2.zero;
            slider.fillRect=fill;
            slider.targetGraphic=track.GetComponent<Image>();
            slider.value=volume;
            slider.onValueChanged.AddListener(v=>{volume=v;SaveAudio();});
            Button(settings.transform,"Settings Back",UIText.Get("common.back"),new Vector2(0,-240),new Vector2(270,74),"UI_Medium_Cream",CloseSettings);

            resultBackground=Picture(result.transform,"UI_Result_GameOver",new Vector2(0,0),new Vector2(760,450));
            resultTitle=Label(result.transform,UIText.Get("result.over"),new Vector2(0,155),new Vector2(410,95),42,Color.white);
            resultScore=Label(result.transform,"",new Vector2(0,12),new Vector2(580,100),30,Ink);
            Button(result.transform,"Result Home",UIText.Get("common.home"),new Vector2(-115,-122),new Vector2(210,70),null,ShowLobby);
            Button(result.transform,"Retry",UIText.Get("common.retry"),new Vector2(115,-122),new Vector2(210,70),null,RetryOrNewRun);
            continueButton=Button(result.transform,"Next",UIText.Get("common.next_stage"),new Vector2(0,-285),new Vector2(370,85),"UI_Large_Cyan",
                ContinueFromResult);
        }
        private static Color Ink => new Color(.1f,.17f,.35f);
        private GameObject Screen(string name,bool dim)
        {
            var rect=Rect(canvas,name,Vector2.zero,Vector2.zero);
            rect.anchorMin=Vector2.zero;
            rect.anchorMax=Vector2.one;
            rect.offsetMin=rect.offsetMax=Vector2.zero;
            if(dim)
            {
                var image=rect.gameObject.AddComponent<Image>();
                image.color=new Color(.035f,.10f,.23f,.72f);
            }
            return rect.gameObject;
        }
        private RectTransform Rect(Transform parent,string name,Vector2 position,Vector2 size)
        {
            var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent,false);
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
            rect.anchoredPosition=position;rect.sizeDelta=size;
            return rect;
        }
        private Image Picture(Transform parent,string key,Vector2 position,Vector2 size)
        {
            var image=Rect(parent,key??"Artwork",position,size).gameObject.AddComponent<Image>();
            image.sprite=key==null?null:assets.Find(key);
            image.preserveAspect=true;
            image.raycastTarget=false;
            return image;
        }
        private TMP_Text Label(Transform parent,string text,Vector2 position,Vector2 size,int fontSize,Color color)
        {
            var label=Rect(parent,"Label",position,size).gameObject.AddComponent<TextMeshProUGUI>();
            label.font=font;
            label.text=text;
            label.fontSize=fontSize;
            label.color=color;
            label.alignment=TextAlignmentOptions.Center;
            label.raycastTarget=false;
            label.textWrappingMode=TextWrappingModes.NoWrap;
            label.enableAutoSizing=true;
            label.fontSizeMin=fontSize*.8f;
            label.fontSizeMax=fontSize;
            label.extraPadding=true;
            label.outlineColor=Ink;
            label.outlineWidth=color==Color.white ? .18f : 0f;
            // SDF underlay gives a clean, subtle shadow without duplicate text meshes.
            var material=label.fontMaterial;
            material.EnableKeyword("OUTLINE_ON");
            material.EnableKeyword("UNDERLAY_ON");
            material.SetColor("_UnderlayColor",new Color(.035f,.075f,.17f,color==Color.white ? .65f : .25f));
            material.SetFloat("_UnderlayOffsetX",.12f);
            material.SetFloat("_UnderlayOffsetY",-.2f);
            material.SetFloat("_UnderlaySoftness",.15f);
            return label;
        }
        private Button Button(Transform parent,string name,string text,Vector2 position,Vector2 size,string sprite,Action action)
        {
            var image=Picture(parent,sprite,position,size);
            image.gameObject.name=name;
            if(sprite==null)image.color=Color.clear;
            image.raycastTarget=true;
            var button=image.gameObject.AddComponent<Button>();
            button.targetGraphic=image;
            var colors=button.colors;
            colors.highlightedColor=new Color(.88f,.97f,1);
            colors.pressedColor=new Color(.7f,.85f,.95f);
            button.colors=colors;
            var navigation=button.navigation;navigation.mode=Navigation.Mode.None;button.navigation=navigation;
            button.onClick.AddListener(()=>action());
            if(!string.IsNullOrEmpty(text))Label(image.transform,text,new Vector2(12,0),size*.8f,30,Ink);
            return button;
        }
        private RectTransform Box(Transform parent,string name,Vector2 position,Vector2 size,Color color)
        {
            var rect=Rect(parent,name,position,size);
            var image=rect.gameObject.AddComponent<Image>();
            image.color=color;image.raycastTarget=false;
            return rect;
        }
        private Image Meter(Transform parent,string name,Vector2 position,Vector2 size,Color color)
        {
            var track=Box(parent,name,position,size+new Vector2(8,8),new Color(.07f,.14f,.28f,.85f));
            var fill=Box(track,"Fill",Vector2.zero,size,color).GetComponent<Image>();
            fill.sprite=whiteSprite;
            fill.type=Image.Type.Filled;fill.fillMethod=Image.FillMethod.Horizontal;fill.fillAmount=0;
            return fill;
        }
    }
}
