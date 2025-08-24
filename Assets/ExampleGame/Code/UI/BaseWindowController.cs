using System.Collections;
using System.Collections.Generic;
using Code.UI.Buttons;
using UnityEngine;

public class BaseWindowController : MonoBehaviour
{
    [SerializeField]
    private GameActionButton closeButton;

    public void Init(object data)
    {
        closeButton.RegisterClick(GameAction.CloseWindow, this);
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}
