using Code.Core;
using Code.Events;
using Core;
using ExampleGame.Code.Enums;
using ExampleGame.Code.Events;
using SDK.Code.Core;
using UnityEngine;

namespace ExampleGame.Code.Managers {

    public class SDKManager : IBaseEventReceiver {
        private readonly string AppKey = "VoodooSDKAppKey";
        private readonly string ServerURL = "http://localhost:5000/";
        private EventBus eventBus;
        private VoodooSDK voodooSDKInstance;

        public SDKManager(EventBus eventBus) {
            this.eventBus = eventBus;
        }

        public void OnEvent(IEvent @event) {
            switch (@event) {
                case OnShowSingleOffer onShowSingleOffer:
                    UIManager.Instance.LoadPopUpWindow(WindowType.SingleOffer);
                    Debug.Log($"[SDKManager][OnEvent] Listened {@event}");
                    voodooSDKInstance.OfferSystem.GetSingleOffer();
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
            }
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
        }

        public void UnsubscribeFromEvents() {
            eventBus.Unregister<OnShowSingleOffer>(this);
            eventBus.Unregister<OnShowChainedOffer>(this);
            eventBus.Unregister<OnShowEndlessOffer>(this);
            eventBus.Unregister<OnShowMultipleOffer>(this);
        }
    }

}