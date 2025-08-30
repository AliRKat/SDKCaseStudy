using Code.Core;
using ExampleGame.Code.Enums;
using ExampleGame.Code.Managers;
using UnityEngine;

namespace ExampleGame.Code.Core {

    public class GameManager : MonoBehaviour {
        public static GameManager Instance;
        public CurrencyManager CurrencyManager;
        public EventBus EventBus;
        public GameplayManager GameplayManager;
        public SDKManager SDKManager;

        private void Awake() {
            Instance = this;
            DontDestroyOnLoad(this);
            EventBus = new EventBus();
        }

        private void Start() {
            Init();
        }

        private void OnDestroy() {
            SDKManager.UnsubscribeFromEvents();
        }

        private void Init() {
            CreateObjects();
            InitObjects();
            Debug.Log("[GameManager][Init] Initialized successfully!");

            StartCoroutine(
                SceneHandler.LoadSceneAsync(
                    GameScene.Home,
                    () => { Debug.Log("[GameManager][Init] Home scene loaded."); }
                )
            );
        }

        private void CreateObjects() {
            CurrencyManager = new CurrencyManager();
            GameplayManager = new GameplayManager();
            SDKManager = new SDKManager(EventBus, CurrencyManager, GameplayManager);
        }

        private void InitObjects() {
            CurrencyManager.Init();
            GameplayManager.Init();
            SDKManager.Init();
        }
    }

}