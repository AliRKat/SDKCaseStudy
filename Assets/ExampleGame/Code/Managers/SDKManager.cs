using System;
using System.Collections.Generic;
using System.IO;
using Code.Core;
using Code.Events;
using Core;
using ExampleGame.Code.Core;
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
        private string _lastEndlessOfferId;
        private CurrencyManager currencyManager;
        private EventBus eventBus;
        private GameplayManager gameplayManager;
        private VoodooSDK voodooSDKInstance;

        public SDKManager(EventBus eventBus, CurrencyManager currencyManager, GameplayManager gameplayManager) {
            this.eventBus = eventBus;
            this.currencyManager = currencyManager;
            this.gameplayManager = gameplayManager;
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
                    Debug.Log($"[SDKManager][OnEvent] Listened {@event}");
                    voodooSDKInstance.OfferSystem.GetChainedOffers(this, offer => {
                        if (offer != null) {
                            UIManager.Instance.LoadPopUpWindow(WindowType.ChainedOffer, offer);
                            Debug.Log($"[SDKManager] Showing chained offer: {offer.Id}");
                        } 
                        else {
                            Debug.LogWarning("[SDKManager] No eligible chained offer found.");
                        }
                    });
                    break;

                case OnShowEndlessOffer _: {
                    Debug.Log($"[SDKManager][OnEvent] Listened {@event}");
                    voodooSDKInstance.OfferSystem.GetEndlessOffer(null, this, offer => {
                        if (offer != null) {
                            UIManager.Instance.LoadPopUpWindow(WindowType.EndlessOffer, offer);
                            Debug.Log($"[SDKManager] Showing endless offer: {offer.Id}");
                        }
                        else {
                            Debug.LogWarning("[SDKManager] No eligible endless offer found.");
                        }
                    });
                    break;
                }
                case OnShowMultipleOffer _:
                    Debug.Log($"[SDKManager][OnEvent] Listened {@event}");
                    voodooSDKInstance.OfferSystem.GetMultipleOffersManual(
                        this,
                        selected => {
                            if (selected != null)
                                UIManager.Instance.LoadPopUpWindow(WindowType.MultipleOffer, selected);
                            else
                                Debug.LogWarning("[SDKManager] No eligible multiple offers to show");
                        }
                    );
                    break;
                // this is given to multiple offers
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
                // this is given to single offers
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
            try {
                var path = Path.Combine(Application.persistentDataPath, "boughtOffers.json");
                if (!File.Exists(path))
                    return false;

                var json = File.ReadAllText(path);
                var dto = JsonUtility.FromJson<BoughtOffersDTO>(json);

                if (dto == null || dto.offerIds == null)
                    return false;

                return dto.offerIds.Contains(offerId);
            }
            catch (Exception ex) {
                Debug.LogError($"[SDKManager][HasPurchased] Failed to check HasPurchased for {offerId}: {ex}");
                return false;
            }
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

        public void GetChainedOfferWrapper(Action<Offer> callback) {
            voodooSDKInstance.OfferSystem.GetChainedOffers(this, callback);
        }

        public void GetEndlessOfferWrapper(Offer current, Action<Offer> callback) {
            voodooSDKInstance.OfferSystem.GetEndlessOffer(current, this, callback);
        }

        public void HandleBuyOffer(string offerId, Action<Offer> callback) {
            voodooSDKInstance.OfferSystem.GetOfferById(offerId, offer => {
                if (offer == null) {
                    Debug.LogWarning($"[SDKManager][HandleBuyOffer] Offer {offerId} not found.");
                    callback?.Invoke(null);
                    return;
                }

                var type = Converters.ToCurrencyType(offer.Price.Currency);
                var amount = offer.Price.Amount;

                if (!GameManager.Instance.CurrencyManager.TrySpend(type, amount)) {
                    Debug.LogWarning($"[SDKManager][HandleBuyOffer] Not enough currency for offer {offer.Id}");
                    callback?.Invoke(null);
                    return;
                }

                voodooSDKInstance.OfferSystem.BuyOfferWithId(offerId, purchased => {
                    if (purchased != null) {
                        Debug.Log($"[SDKManager][HandleBuyOffer] Offer purchased successfully: {purchased.Id}");

                        RegisterRewards(purchased.Rewards);
                        callback?.Invoke(purchased);

                        if (purchased.Type == OfferType.Chained)
                            GetChainedOfferWrapper(
                                nextOffer => {
                                    if (nextOffer != null) {
                                        Debug.Log($"[SDKManager][HandleBuyOffer] Next chained offer: {nextOffer.Id}");
                                        UIManager.Instance.LoadPopUpWindow(WindowType.ChainedOffer, nextOffer);
                                    }
                                    else {
                                        Debug.Log("[SDKManager][HandleBuyOffer] No further chained offers available.");
                                    }
                                }
                            );

                        if (purchased.Type == OfferType.Endless)
                            voodooSDKInstance.OfferSystem.GetEndlessOffer(purchased, this, nextOffer => {
                                if (nextOffer != null) {
                                    Debug.Log($"[SDKManager][HandleBuyOffer] Next endless offer: {nextOffer.Id}");
                                    UIManager.Instance.LoadPopUpWindow(WindowType.EndlessOffer, nextOffer);
                                }
                                else {
                                    Debug.Log("[SDKManager][HandleBuyOffer] No eligible endless offer.");
                                }
                            });
                    }
                    else {
                        Debug.LogWarning($"[SDKManager][HandleBuyOffer] Failed to finalize purchase of offer {offerId}");
                        callback?.Invoke(null);
                    }
                });
            });
        }

        private void RegisterRewards(List<OfferReward> rewards) {
            if (rewards == null || rewards.Count == 0) {
                Debug.LogWarning("[SDKManager][RegisterRewards] No rewards to register.");
                return;
            }

            foreach (var reward in rewards) {
                var type = Converters.ToCurrencyType(reward.ItemId);
                if (type == CurrencyType.None) {
                    Debug.LogWarning($"[SDKManager][RegisterRewards] Unsupported reward type: {reward.ItemId}");
                    continue;
                }

                GameManager.Instance.CurrencyManager.Add(type, reward.Amount);
                Debug.Log($"[SDKManager][RegisterRewards] Granted {reward.Amount} {type}");
            }
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