using System;
using System.Collections.Generic;
using Code.Core;
using Code.Events;
using Core;
using ExampleGame.Code.Enums;
using ExampleGame.Code.Events;
using SDK.Code.Core;
using SDK.Code.Core.Enums;
using SDK.Code.Core.Strategy;
using SDK.Code.Interfaces;
using SDK.Code.Models;
using SDK.Code.Utils;
using UnityEngine;

namespace ExampleGame.Code.Managers {

    public class SDKManager : IBaseEventReceiver, IGameStateProvider {
        private readonly string AppKey = "VoodooSDKAppKey";
        private readonly string ServerURL = "http://localhost:5000/";
        private readonly int sessionTimeout = 60; // seconds
        private CurrencyManager currencyManager;
        private EventBus eventBus;
        private GameplayManager gameplayManager;
        private VoodooSDK voodooSDKInstance;

        public SDKManager(EventBus eventBus, CurrencyManager currencyManager, GameplayManager gameplayManager) {
            this.eventBus = eventBus;
            this.currencyManager = currencyManager;
            this.gameplayManager = gameplayManager;
        }

        public void OnEvent(IEvent @event) {
            switch (@event) {
                case OnShowSingleOffer _:
                    voodooSDKInstance.OfferSystem.GetSingleOfferManual(this, offer => {
                        if (offer != null) {
                            Debug.Log($"[SDKManager] Showing manual offer: {offer.Id}");
                            UIManager.Instance.LoadPopUpWindow(WindowType.SingleOffer, offer);
                        }
                        else {
                            Debug.Log("[SDKManager] No manual offer found");
                        }
                    });
                    break;
                case OnShowChainedOffer _:
                    UIManager.Instance.LoadPopUpWindow(WindowType.ChainedOffer);
                    Debug.Log($"[SDKManager][OnEvent] Listened {@event}");
                    voodooSDKInstance.OfferSystem.GetChainedOffers();
                    break;
                case OnShowEndlessOffer _:
                    UIManager.Instance.LoadPopUpWindow(WindowType.EndlessOffer);
                    Debug.Log($"[SDKManager][OnEvent] Listened {@event}");
                    voodooSDKInstance.OfferSystem.GetEndlessOffers();
                    break;
                case OnShowMultipleOffer _:
                    Debug.Log($"[SDKManager][OnEvent] Listened {@event}");
                    break;
                case OnStageComplete _:
                    Debug.Log($"[SDKManager][OnEvent] Listened {@event}");
                    voodooSDKInstance.OfferSystem.GetMultipleOffers(SDKEventKeys.StageComplete,
                        this,
                        selected => {
                            if (selected != null)
                                UIManager.Instance.LoadPopUpWindow(WindowType.MultipleOffer, selected);
                            else
                                Debug.LogWarning("[SDKManager] No eligible multiple offers to show");
                        }
                    );
                    break;
                case OnLevelComplete _:
                    voodooSDKInstance.OfferSystem.GetSingleOffer(SDKEventKeys.LevelComplete, this, offer => {
                        if (offer != null) {
                            Debug.Log($"[SDKManager] Showing level complete offer: {offer.Id}");
                            UIManager.Instance.LoadPopUpWindow(WindowType.SingleOffer, offer);
                        }
                        else {
                            Debug.Log("[SDKManager] No eligible offer found for LEVEL_COMPLETE");
                        }
                    }, GetUserSegmentation());
                    break;
            }
        }

        public int GetPlayerLevel() {
            return gameplayManager.GetPlayerLevel();
        }

        public int GetCompletedStages() {
            return gameplayManager.GetCompletedStages();
        }

        public int GetCurrency(string currency) {
            if (Enum.TryParse<CurrencyType>(currency, out var type)) return currencyManager.Get(type);

            Debug.LogWarning($"[IGameStateProvider][GetCurrency] Unknown currency type: {currency}");
            return 0;
        }

        public bool HasPurchased(string offerId) {
            throw new NotImplementedException();
        }

        public DateTime GetLastShown(string offerId) {
            throw new NotImplementedException();
        }

        public string GetRegion() {
            return gameplayManager.GetRegion();
        }

        public string GetPlayerType() {
            return gameplayManager.GetPlayerType();
        }

        public Dictionary<string, string> GetUserSegmentation() {
            var userSegments = new Dictionary<string, string> {
                ["geo"] = GetRegion(),
                ["playerType"] = GetPlayerType()
            };
            return userSegments;
        }

        public void HandleBuyOffer(string offerId, Action<Offer> callback) {
            voodooSDKInstance.OfferSystem.GetOfferById(offerId, offer => {
                if (offer == null) {
                    Debug.LogWarning($"[SDKManager] Offer not found: {offerId}");
                    callback?.Invoke(null);
                    return;
                }

                var type = Converters.ToCurrencyType(offer.Price.Currency);
                var amount = offer.Price.Amount;

                if (!GameManager.Instance.CurrencyManager.TrySpend(type, amount)) {
                    Debug.LogWarning($"[SDKManager] Not enough currency for offer {offer.Id}");
                    callback?.Invoke(null);
                    return;
                }

                voodooSDKInstance.OfferSystem.BuyOfferWithId(offerId, purchasedOffer => {
                    if (purchasedOffer != null) {
                        Debug.Log($"[SDKManager] Offer purchased successfully: {purchasedOffer.Id}");
                        RegisterRewards(purchasedOffer.Rewards);
                        callback?.Invoke(purchasedOffer);
                    }
                    else {
                        Debug.LogWarning($"[SDKManager] Failed to finalize purchase of offer {offerId}");
                        callback?.Invoke(null);
                    }
                });
            });
        }

        private void RegisterRewards(List<OfferReward> rewards) {
            if (rewards == null || rewards.Count == 0) {
                Debug.LogWarning("[SDKManager] No rewards to register.");
                return;
            }

            foreach (var reward in rewards) {
                var type = Converters.ToCurrencyType(reward.ItemId);
                if (type == CurrencyType.None) {
                    Debug.LogWarning($"[SDKManager] Unsupported reward type: {reward.ItemId}");
                    continue;
                }

                GameManager.Instance.CurrencyManager.Add(type, reward.Amount);
                Debug.Log($"[SDKManager] Granted {reward.Amount} {type}");
            }
        }

        public void Init() {
            voodooSDKInstance = VoodooSDK.Instance;
            SubscribeToEvents();

            var sdkConfiguration = new VoodooSDKConfiguration(AppKey, ServerURL)
                .EnableLogging()
                .SetSessionTimeout(sessionTimeout)
                .SetGameStateProvider(this)
                .SetOfferSelectionStrategy(new RotationOfferSelectionStrategy())
                .SetOfferReadyAction(offer => { UIManager.Instance.LoadPopUpWindow(WindowType.SingleOffer, offer); });
            voodooSDKInstance.Init(sdkConfiguration);
        }

        private void SubscribeToEvents() {
            eventBus.Register<OnShowSingleOffer>(this);
            eventBus.Register<OnShowChainedOffer>(this);
            eventBus.Register<OnShowEndlessOffer>(this);
            eventBus.Register<OnShowMultipleOffer>(this);
            eventBus.Register<OnStageComplete>(this);
            eventBus.Register<OnLevelComplete>(this);
        }

        public void UnsubscribeFromEvents() {
            eventBus.Unregister<OnShowSingleOffer>(this);
            eventBus.Unregister<OnShowChainedOffer>(this);
            eventBus.Unregister<OnShowEndlessOffer>(this);
            eventBus.Unregister<OnShowMultipleOffer>(this);
            eventBus.Unregister<OnStageComplete>(this);
            eventBus.Unregister<OnLevelComplete>(this);
        }
    }

}