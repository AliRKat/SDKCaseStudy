using Core;
using ExampleGame.Code.Core;
using ExampleGame.Code.Events;
using UnityEngine;

namespace ExampleGame.Code.Managers {

    public class GameplayManager : IBaseEventReceiver {
        private int _completedStages;
        private string _currentPlayerType = "FREE";
        private string _currentRegion = "EU";
        private int _playerLevel;

        public void OnEvent(IEvent @event) {
        }

        public void Init() {
            _playerLevel = 1;
            _completedStages = 0;
            _currentRegion = "EU";
            _currentPlayerType = "FREE";
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

        /// <summary>
        ///     Toggles the current region between "EU" and "US".
        /// </summary>
        public void SwapRegion() {
            _currentRegion = _currentRegion == "EU" ? "US" : "EU";
            Debug.Log($"[GameplayManager][SwapRegion] Region swapped to: {_currentRegion}");
        }

        /// <summary>
        ///     Gets the current region (e.g., "EU" or "US").
        /// </summary>
        public string GetRegion() {
            return _currentRegion;
        }

        /// <summary>
        ///     Toggles the player type between "FREE" and "PREMIUM".
        /// </summary>
        public void SwapPlayerType() {
            _currentPlayerType = _currentPlayerType == "FREE" ? "PREMIUM" : "FREE";
            Debug.Log($"[GameplayManager][SwapPlayerType] Player type swapped to: {_currentPlayerType}");
        }

        /// <summary>
        ///     Gets the current player type (e.g., "FREE" or "PREMIUM").
        /// </summary>
        public string GetPlayerType() {
            return _currentPlayerType;
        }
    }

}