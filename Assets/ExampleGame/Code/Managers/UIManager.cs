using System;
using Code.Core;
using Code.Events;
using Core;
using UnityEngine;

public class UIManager : IBaseEventReceiver
{
    private EventBus eventBus;

    public UIManager(EventBus eventBus)
    {
        this.eventBus = eventBus;
    }

    public void Init()
    {
        Debug.Log("[UIManager] Initialized");
    }

    public void OnEvent(IEvent @event)
    {
        Debug.Log($"[UIManager] Event received: {@event.GetType().Name}");
    }

    public void HandleGameAction(GameAction action, object data = null)
    {
        Debug.Log($"[UIManager] Handling GameAction: {action}");

        switch (action)
        {
            case GameAction.ShowSingleOffer:
                eventBus.Raise(new OnShowSingleOffer());
                break;

            case GameAction.ShowChainedOffer:
                eventBus.Raise(new OnShowChainedOffer());
                break;

            case GameAction.ShowEndlessOffer:
                eventBus.Raise(new OnShowEndlessOffer());
                break;

            default:
                Debug.LogWarning($"[UIManager][HandleGameAction] Unhandled action: {action}");
                break;
        }
    }
}
