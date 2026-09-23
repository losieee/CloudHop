using UnityEngine;
using UnityEngine.UI;

namespace CloudHop
{
    public sealed class PrototypeHUD : MonoBehaviour
    {
        [SerializeField] private GameManager game;
        [SerializeField] private ScoreManager scores;
        [SerializeField] private PlayerController player;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text resultText;
        [SerializeField] private Text chargeText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Button retryButton;
        [Header("Optional fixed stages")]
        [SerializeField] private StageDirector stages;
        [SerializeField] private Text stageText;
        [SerializeField] private Text progressText;
        [SerializeField] private Button nextStageButton;
        [SerializeField] private Text nextStageLabel;
        private void OnEnable()
        {
            scores.Changed += RefreshScore;
            game.StateChanged += RefreshState;
            retryButton.onClick.AddListener(game.Retry);
            if (stages != null) stages.Changed += RefreshStage;
            if (nextStageButton != null) nextStageButton.onClick.AddListener(game.ContinueStages);
            RefreshScore();
            RefreshState(game.State);
        }
        private void OnDisable()
        {
            scores.Changed -= RefreshScore;
            game.StateChanged -= RefreshState;
            retryButton.onClick.RemoveListener(game.Retry);
            if (stages != null) stages.Changed -= RefreshStage;
            if (nextStageButton != null) nextStageButton.onClick.RemoveListener(game.ContinueStages);
        }
        private void Update() => chargeText.text = player.IsCharging ? $"CHARGE  {player.Charge01:P0}" : "HOLD SPACE / LEFT CLICK  -  RELEASE TO JUMP";
        private void RefreshScore()
        {
            scoreText.text = $"SCORE : {scores.Score}";
            resultText.text = $"GAME OVER\n\nSCORE   {scores.Score}\nBEST   {scores.Best}";
        }
        private void RefreshStage()
        {
            var course = stages.CurrentCourse;
            stageText.text = $"{course.Definition.stageName}  /  {course.Definition.difficulty}";
            progressText.text = $"{stages.Progress} / {course.JumpCount}   -   {stages.SectionName}";
        }
        private void RefreshState(GameState state)
        {
            bool clear = state == GameState.StageClear;
            gameOverPanel.SetActive(state == GameState.GameOver || clear);
            if (nextStageButton != null)
            {
                nextStageButton.gameObject.SetActive(clear);
                nextStageLabel.text = stages.IsLastStage ? "RESTART ALL" : "NEXT STAGE";
            }
            if (clear)
                resultText.text = $"{(stages.IsLastStage ? "ALL STAGES CLEAR" : "STAGE CLEAR")}\n\nSCORE   {scores.Score}\nBEST   {scores.Best}";
            else RefreshScore();
        }
    }
}
