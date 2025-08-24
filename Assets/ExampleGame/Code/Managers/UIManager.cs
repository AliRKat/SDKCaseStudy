using System;
using Code.Core;
using Code.Events;
using Core;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    private const string WindowResourcePath = "UI/Windows/";

    [SerializeField]
    private Transform windowParent;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void HandleGameAction(GameAction action, object data = null)
    {
        Debug.Log($"[UIManager][HandleGameAction] Handling GameAction: {action}");

        switch (action)
        {
            case GameAction.ShowSingleOffer:
                GameManager.Instance.EventBus.Raise(new OnShowSingleOffer());
                break;

            case GameAction.ShowChainedOffer:
                GameManager.Instance.EventBus.Raise(new OnShowChainedOffer());
                break;

            case GameAction.ShowEndlessOffer:
                GameManager.Instance.EventBus.Raise(new OnShowEndlessOffer());
                break;

            case GameAction.CloseWindow:
                if (data is BaseWindowController window)
                {
                    window.Close();
                }
                break;

            default:
                Debug.LogWarning($"[UIManager][HandleGameAction] Unhandled action: {action}");
                break;
        }
    }

    public void LoadPopUpWindow(WindowType windowType, object data = null)
    {
        string windowName = windowType.ToString();
        var prefab = Resources.Load<GameObject>($"{WindowResourcePath}{windowName}");

        if (prefab == null)
        {
            Debug.LogError(
                $"[UIManager][LoadPopUpWindow] Failed to load window prefab: {WindowResourcePath}{windowName}"
            );
            return;
        }

        var instance = UnityEngine.Object.Instantiate(prefab, windowParent);
        instance.name = windowName;

        var controller = instance.GetComponent<BaseWindowController>();
        if (controller != null)
        {
            controller.Init(data);
        }

        Debug.Log($"[UIManager][LoadPopUpWindow] Loaded popup window: {windowName}");
    }
}
