using System.Collections.Generic;
using System.Linq;
using Code.Events;
using Core;
using ExampleGame.Code.Core;
using ExampleGame.Code.Enums;
using ExampleGame.Code.Events;
using ExampleGame.Code.UI;
using SDK.Code.Models;
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

        private readonly Queue<(WindowType, object)> _windowQueue = new();
        private GameObject _activeWindow;

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
                case OnCurrencyChanged:
                case OnLevelComplete:
                case OnStageComplete:
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
                    switch (data) {
                        case SingleOfferWindowController single:
                            single.Close();
                            break;

                        case MultipleOfferWindowController multi:
                            multi.Close();
                            break;

                        default:
                            Debug.LogWarning("[UIManager] CloseWindow called with unsupported window type");
                            break;
                    }

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

                case GameAction.BuyOffer:
                    var sdkManager = GameManager.Instance.SDKManager;
                    switch (data) {
                        // --- Single / --- Chained --- / Endless ---
                        case SingleOfferWindowController singleWindow: {
                            var initData = singleWindow.GetInitData<SingleOfferWindowInitData>();
                            if (initData != null)
                                sdkManager.HandleBuyOffer(initData.offerId, offer => {
                                    if (offer != null) {
                                        Debug.Log($"Player bought {offer.Id}, Rewards: {offer.GetRewardsString()}");

                                        var isChained = offer.Type == OfferType.Chained;
                                        var isEndless = offer.Type == OfferType.Endless;

                                        singleWindow.Close();

                                        if (isChained && !string.IsNullOrEmpty(offer.NextOfferId)) {
                                            Debug.Log($"[Chained] Next offer in chain: {offer.NextOfferId}");
                                            sdkManager.GetChainedOfferWrapper(next => {
                                                if (next != null)
                                                    LoadPopUpWindow(WindowType.ChainedOffer, next);
                                                else
                                                    Debug.Log("[Chained] No further chained offer found.");
                                            });
                                        }
                                        else if (isEndless) {
                                            Debug.Log(
                                                $"[Endless] Player purchased {offer.Id}, cycling to next offer...");
                                            sdkManager.GetEndlessOfferWrapper(offer, next => {
                                                if (next != null)
                                                    LoadPopUpWindow(WindowType.EndlessOffer, next);
                                                else
                                                    Debug.Log("[Endless] No eligible endless offer found.");
                                            });
                                        }
                                    }
                                    else {
                                        Debug.LogWarning("Purchase failed");
                                        singleWindow.Close();
                                    }
                                });
                            break;
                        }

                        case SingleOfferWindowInitData subOfferData: {
                            sdkManager.HandleBuyOffer(subOfferData.offerId, offer => {
                                if (offer != null)
                                    Debug.Log($"Player bought {offer.Id}, Rewards: {offer.GetRewardsString()}");
                                else
                                    Debug.LogWarning("Purchase failed");

                                if (windowParent.childCount > 0) {
                                    var topWindow = windowParent.GetChild(windowParent.childCount - 1)
                                        .GetComponent<MultipleOfferWindowController>();
                                    topWindow?.Close();
                                }
                            });
                            break;
                        }

                        default:
                            Debug.LogWarning("[UIManager] BuyOffer action received with unsupported data type");
                            break;
                    }

                    break;
            }
        }

        public void LoadPopUpWindow(WindowType windowType, object data = null) {
            switch (windowType) {
                case WindowType.SingleOffer:
                case WindowType.ChainedOffer:
                case WindowType.EndlessOffer:
                    if (data is Offer offer) {
                        var windowInitData = new SingleOfferWindowInitData {
                            offerId = offer.Id,
                            price = offer.Price.ToString(),
                            reward = offer.GetRewardsString()
                        };

                        if (_activeWindow != null) {
                            _windowQueue.Enqueue((windowType, windowInitData));
                            Debug.Log($"[UIManager] Queued popup window: {windowType}");
                            return;
                        }

                        ShowWindow(WindowType.SingleOffer, windowInitData);
                    }

                    break;

                case WindowType.MultipleOffer:
                    if (data is MultipleOffer multipleOffer) {
                        var list = multipleOffer.Offers
                            .Select(o => new SingleOfferWindowInitData {
                                offerId = o.Id,
                                price = o.Price.ToString(),
                                reward = o.GetRewardsString()
                            })
                            .ToList();

                        var windowInitData = new MultipleOfferWindowInitData {
                            offerList = list
                        };

                        if (_activeWindow != null) {
                            _windowQueue.Enqueue((windowType, windowInitData));
                            Debug.Log($"[UIManager] Queued popup window: {windowType}");
                            return;
                        }

                        ShowWindow(windowType, windowInitData);
                    }

                    break;
            }
        }

        private void ShowWindow(WindowType windowType, object data) {
            var windowName = windowType.ToString();
            var prefab = Resources.Load<GameObject>($"{WindowResourcePath}{windowName}");

            if (prefab == null) {
                Debug.LogError(
                    $"[UIManager][LoadPopUpWindow] Failed to load window prefab: {WindowResourcePath}{windowName}");
                return;
            }

            _activeWindow = Instantiate(prefab, windowParent);
            _activeWindow.name = windowName;

            switch (windowType) {
                case WindowType.SingleOffer:
                    var singleCtrl = _activeWindow.GetComponent<SingleOfferWindowController>();
                    if (singleCtrl != null) {
                        singleCtrl.Init(data);
                        singleCtrl.OnClosed += HandleWindowClosed;
                    }

                    break;

                case WindowType.MultipleOffer:
                    var multiCtrl = _activeWindow.GetComponent<MultipleOfferWindowController>();
                    if (multiCtrl != null) {
                        multiCtrl.Init(data);
                        multiCtrl.OnClosed += HandleWindowClosed;
                    }

                    break;

                default:
                    Debug.LogWarning($"[UIManager] No controller found for {windowType}");
                    break;
            }

            Debug.Log($"[UIManager][LoadPopUpWindow] Loaded popup window: {windowName}");
        }

        private void HandleWindowClosed() {
            if (_activeWindow != null) {
                var singleCtrl = _activeWindow.GetComponent<SingleOfferWindowController>();
                if (singleCtrl != null) singleCtrl.OnClosed -= HandleWindowClosed;

                var multiCtrl = _activeWindow.GetComponent<MultipleOfferWindowController>();
                if (multiCtrl != null) multiCtrl.OnClosed -= HandleWindowClosed;

                Destroy(_activeWindow);
                _activeWindow = null;
            }

            if (_windowQueue.Count > 0)
                _windowQueue.Dequeue();
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