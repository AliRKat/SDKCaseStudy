using Code.Core;
using Code.Events;
using ExampleGame.Code.Enums;
using ExampleGame.Code.Events;
using UnityEngine;

namespace ExampleGame.Code.Managers {

    public class UIManager : MonoBehaviour {
        private const string WindowResourcePath = "UI/Windows/";
        public static UIManager Instance;

        [SerializeField] private Transform windowParent;

        private void Awake() {
            Instance = this;
            DontDestroyOnLoad(this);
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
    }

}