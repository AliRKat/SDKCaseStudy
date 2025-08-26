using System;
using Code.Core;
using Code.Events;
using Core;
using ExampleGame.Code.Enums;
using ExampleGame.Code.Events;
using SDK.Code.Core;
using SDK.Code.Interfaces;
using UnityEngine;

namespace ExampleGame.Code.Managers {

    public class SDKManager : IBaseEventReceiver, IGameStateProvider {
        private readonly string AppKey = "VoodooSDKAppKey";
        private readonly string ServerURL = "http://localhost:5000/";
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
                case OnShowSingleOffer onShowSingleOffer:
                    voodooSDKInstance.OfferSystem.GetSingleOfferManual(this, offer => {
                        if (offer != null) {
                            Debug.Log($"[SDKManager] Showing manual offer: {offer.Id}");
                            UIManager.Instance.LoadPopUpWindow(WindowType.SingleOffer);
                        }
                        else {
                            Debug.Log("[SDKManager] No manual offer found");
                        }
                    });
                    break;
                case OnShowChainedOffer onShowChainedOffer:
                    UIManager.Instance.LoadPopUpWindow(WindowType.ChainedOffer);
                    Debug.Log($"[SDKManager][OnEvent] Listened {@event}");
                    voodooSDKInstance.OfferSystem.GetChainedOffers();
                    break;
                case OnShowEndlessOffer onShowEndlessOffer:
                    UIManager.Instance.LoadPopUpWindow(WindowType.EndlessOffer);
                    Debug.Log($"[SDKManager][OnEvent] Listened {@event}");
                    voodooSDKInstance.OfferSystem.GetEndlessOffers();
                    break;
                case OnShowMultipleOffer onShowMultipleOffer:
                    UIManager.Instance.LoadPopUpWindow(WindowType.MultipleOffer);
                    Debug.Log($"[SDKManager][OnEvent] Listened {@event}");
                    voodooSDKInstance.OfferSystem.GetMultipleOffers();
                    break;
                case OnLevelComplete onLevelComplete:
                    voodooSDKInstance.OfferSystem.GetSingleOffer("LEVEL_COMPLETE", this, offer => {
                        if (offer != null) {
                            Debug.Log($"[SDKManager] Showing level complete offer: {offer.Id}");
                            UIManager.Instance.LoadPopUpWindow(WindowType.SingleOffer);
                        }
                        else {
                            Debug.Log("[SDKManager] No eligible offer found for LEVEL_COMPLETE");
                        }
                    });
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

        public void Init() {
            voodooSDKInstance = VoodooSDK.Instance;
            var sdkConfiguration = new VoodooSDKConfiguration(AppKey, ServerURL)
                .EnableLogging();
            voodooSDKInstance.Init(sdkConfiguration);
        }

        public void SubscribeToEvents() {
            eventBus.Register<OnShowSingleOffer>(this);
            eventBus.Register<OnShowChainedOffer>(this);
            eventBus.Register<OnShowEndlessOffer>(this);
            eventBus.Register<OnShowMultipleOffer>(this);
            eventBus.Register<OnLevelComplete>(this);
        }

        public void UnsubscribeFromEvents() {
            eventBus.Unregister<OnShowSingleOffer>(this);
            eventBus.Unregister<OnShowChainedOffer>(this);
            eventBus.Unregister<OnShowEndlessOffer>(this);
            eventBus.Unregister<OnShowMultipleOffer>(this);
            eventBus.Unregister<OnLevelComplete>(this);
        }
    }

}