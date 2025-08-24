using Code.Core;
using UnityEngine;

namespace Code.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public CurrencyManager CurrencyManager;
        public EventBus EventBus;
        public SDKManager SDKManager;
        public UIManager UIManager;

        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        void Start()
        {
            Init();
        }

        void Init()
        {
            EventBus = new EventBus();
            CreateObjects();
            InitObjects();
            Debug.Log("[GameManager][Init] Initialized successfully!");
        }

        void CreateObjects()
        {
            CurrencyManager = new CurrencyManager();
            SDKManager = new SDKManager(EventBus);
            UIManager = new UIManager(EventBus);
        }

        void InitObjects()
        {
            CurrencyManager.Init();
            SDKManager.Init();
            UIManager.Init();

            SubscribeToEvents();
        }

        void OnDestroy()
        {
            SDKManager.UnsubscribeFromEvents();
        }

        void SubscribeToEvents()
        {
            SDKManager.SubscribeToEvents();
        }
    }
}
