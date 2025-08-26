using Code.Core;
using Core;
using ExampleGame.Code.Events;
using UnityEngine;

namespace ExampleGame.Code.Managers {

    public class GameplayManager : IBaseEventReceiver {
        private int _completedStages;
        private int _playerLevel;

        public void OnEvent(IEvent @event) {
        }

        public void Init() {
            _playerLevel = 1;
            _completedStages = 0;
            Debug.Log("[GameplayManager] Initialized. Level=1, Stages=0");
        }

        public int GetPlayerLevel() {
            return _playerLevel;
        }

        public int GetCompletedStages() {
            return _completedStages;
        }

        public void LevelUp() {
            _playerLevel++;
            GameManager.Instance.EventBus.Raise(new OnLevelComplete(_playerLevel));
            Debug.Log($"[GameplayManager][LevelUp] Player leveled up! New level = {_playerLevel}");
        }

        public void CompleteStage() {
            _completedStages++;
            GameManager.Instance.EventBus.Raise(new OnStageComplete());
            Debug.Log($"[GameplayManager][CompleteStage] Stage completed! Total completed = {_completedStages}");
        }
    }

}