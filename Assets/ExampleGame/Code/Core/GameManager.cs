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
            EventBus = new EventBus();
            CreateObjects();
            InitObjects();
        }

        void CreateObjects()
        {
            CurrencyManager = new CurrencyManager();
        }

        void InitObjects()
        {
            CurrencyManager.Init();
        }
    }
}
