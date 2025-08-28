using Code.Core;
using Code.Events;
using Core;
using ExampleGame.Code.Enums;
using ExampleGame.Code.Events;
using TMPro;
using UnityEngine;

namespace ExampleGame.Code.Managers {

    public class UIManager : MonoBehaviour, IBaseEventReceiver {
        private const string WindowResourcePath = "UI/Windows/";
        public static UIManager Instance;

        [SerializeField] private Transform windowParent;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text coinText;
        [SerializeField] private TMP_Text gemsText;
        [SerializeField] private TMP_Text tokensText;
        [SerializeField] private TMP_Text stagesText;
        [SerializeField] private TMP_Text regionText;
        [SerializeField] private TMP_Text playerTypeText;

        private void Awake() {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        private void Start() {
            SubscribeToEvents();
        }

        private void OnDestroy() {
            UnsubscribeFromEvents();
        }

        public void OnEvent(IEvent @event) {
            switch (@event) {
                case OnCurrencyChanged change:
                case OnLevelComplete complete:
                case OnStageComplete stageComplete:
                    UpdateHUD();
                    break;
            }
        }

        private void SubscribeToEvents() {
            GameManager.Instance.EventBus.Register<OnCurrencyChanged>(this);
            GameManager.Instance.EventBus.Register<OnStageComplete>(this);
            GameManager.Instance.EventBus.Register<OnLevelComplete>(this);
        }

        private void UnsubscribeFromEvents() {
            GameManager.Instance.EventBus.Unregister<OnCurrencyChanged>(this);
            GameManager.Instance.EventBus.Unregister<OnStageComplete>(this);
            GameManager.Instance.EventBus.Unregister<OnLevelComplete>(this);
        }

        public void HandleGameAction(GameAction action, object data = null) {
            Debug.Log($"[UIManager][HandleGameAction] Handling GameAction: {action}");

            switch (action) {
                case GameAction.ShowSingleOffer:
                    GameManager.Instance.EventBus.Raise(new OnShowSingleOffer());
                    break;

                case GameAction.ShowChainedOffer:
                    GameManager.Instance.EventBus.Raise(new OnShowChainedOffer());
                    break;

                case GameAction.ShowEndlessOffer:
                    GameManager.Instance.EventBus.Raise(new OnShowEndlessOffer());
                    break;

                case GameAction.ShowMultipleOffer:
                    GameManager.Instance.EventBus.Raise(new OnShowMultipleOffer());
                    break;

                case GameAction.CloseWindow:
                    if (data is BaseWindowController window) window.Close();
                    break;

                case GameAction.AddCurrency:
                    GameManager.Instance.CurrencyManager.Add(CurrencyType.Coins, 100);
                    GameManager.Instance.CurrencyManager.Add(CurrencyType.Gems, 100);
                    break;

                case GameAction.LevelUp:
                    GameManager.Instance.GameplayManager.LevelUp();
                    break;

                case GameAction.StageComplete:
                    GameManager.Instance.GameplayManager.CompleteStage();
                    break;
                
                case GameAction.SwapRegion:
                    GameManager.Instance.GameplayManager.SwapRegion();
                    UpdateHUD();
                    break;
                
                case GameAction.SwapPlayerType:
                    GameManager.Instance.GameplayManager.SwapPlayerType();
                    UpdateHUD();
                    break;

                default:
                    Debug.LogWarning($"[UIManager][HandleGameAction] Unhandled action: {action}");
                    break;
            }
        }

        public void LoadPopUpWindow(WindowType windowType, object data = null) {
            var windowName = windowType.ToString();
            var prefab = Resources.Load<GameObject>($"{WindowResourcePath}{windowName}");

            if (prefab == null) {
                Debug.LogError(
                    $"[UIManager][LoadPopUpWindow] Failed to load window prefab: {WindowResourcePath}{windowName}"
                );
                return;
            }

            var instance = Instantiate(prefab, windowParent);
            instance.name = windowName;

            var controller = instance.GetComponent<BaseWindowController>();
            if (controller != null) controller.Init(data);

            Debug.Log($"[UIManager][LoadPopUpWindow] Loaded popup window: {windowName}");
        }

        private void UpdateHUD() {
            coinText.text = $"Coins: {GameManager.Instance.CurrencyManager.Get(CurrencyType.Coins)}";
            gemsText.text = $"Gems: {GameManager.Instance.CurrencyManager.Get(CurrencyType.Gems)}";
            tokensText.text = $"Tokens: {GameManager.Instance.CurrencyManager.Get(CurrencyType.Tokens)}";
            levelText.text = $"Level: {GameManager.Instance.GameplayManager.GetPlayerLevel()}";
            stagesText.text = $"Stages: {GameManager.Instance.GameplayManager.GetCompletedStages()}";
            playerTypeText.text = $"Player Type: {GameManager.Instance.GameplayManager.GetPlayerType()}";
            regionText.text = $"Region: {GameManager.Instance.GameplayManager.GetRegion()}";
        }
    }

}