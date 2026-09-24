using System;
using UnityEngine;

namespace CloudHop
{
    public enum GameState { Ready, Playing, GameOver, StageClear }

    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private ScoreManager scores;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private FollowCamera followCamera;
        [SerializeField] private StageDirector stages;
        public GameState State { get; private set; }
        public event Action<GameState> StateChanged;

        private void OnEnable()
        {
            player.ChargeStarted += StartPlaying;
            player.Landed += OnLanded;
            player.Fell += EndGame;
        }
        private void OnDisable()
        {
            player.ChargeStarted -= StartPlaying;
            player.Landed -= OnLanded;
            player.Fell -= EndGame;
        }
        private void Start()
        {
            if (stages != null) stages.ActivateStage(stages.StartingStageIndex);
            Retry();
        }
        private void StartPlaying() { if (State == GameState.Ready) SetState(GameState.Playing); }
        private void OnLanded(Platform platform)
        {
            if (State == GameState.GameOver || State == GameState.StageClear) return;
            scores.RegisterLanding(platform);
            if (stages != null && stages.RegisterLanding(platform))
            {
                player.SetPlayable(false);
                scores.SaveBest();
                SetState(GameState.StageClear);
            }
        }
        private void EndGame()
        {
            if (State == GameState.GameOver) return;
            scores.SaveBest();
            SetState(GameState.GameOver);
        }
        public void Retry()
        {
            scores.ResetScore();
            if (stages != null)
            {
                stages.ResetProgress();
                var mechanics = stages.CurrentCourse.GetComponent<StageMechanics>();
                if (mechanics != null) mechanics.ResetCourse();
            }
            player.ResetPlayer(stages != null ? stages.CurrentCourse.SpawnPosition : spawnPoint.position);
            followCamera.ResetPosition();
            SetState(GameState.Ready);
        }
        public void SelectStage(int index)
        {
            if (stages == null || index < 0 || index >= stages.StageCount) return;
            scores.SaveBest();
            stages.ActivateStage(index);
            Retry();
        }
        public void ContinueStages()
        {
            if (stages == null || State != GameState.StageClear) return;
            SelectStage(stages.IsLastStage ? 0 : stages.CurrentIndex + 1);
        }
        private void SetState(GameState state) { State = state; StateChanged?.Invoke(state); }
    }
}
