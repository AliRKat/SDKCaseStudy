using System.Diagnostics;
using Code.Events;
using Core;
using UnityEngine;

namespace Code.Core
{
    public class SDKManager : IBaseEventReceiver
    {
        private EventBus eventBus;

        public SDKManager(EventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        public void OnEvent(IEvent @event)
        {
            switch (@event)
            {
                case OnShowSingleOffer onShowSingleOffer:
                case OnShowChainedOffer onShowChainedOffer:
                case OnShowEndlessOffer onShowEndlessOffer:
                    UnityEngine.Debug.Log($"[SDKManager][OnEvent] Listened {@event}");
                    break;
            }
        }

        public void Init() { }

        public void SubscribeToEvents()
        {
            eventBus.Register<OnShowSingleOffer>(this);
            eventBus.Register<OnShowChainedOffer>(this);
            eventBus.Register<OnShowEndlessOffer>(this);
        }

        public void UnsubscribeFromEvents()
        {
            eventBus.Unregister<OnShowSingleOffer>(this);
            eventBus.Unregister<OnShowChainedOffer>(this);
            eventBus.Unregister<OnShowEndlessOffer>(this);
        }
    }
}
