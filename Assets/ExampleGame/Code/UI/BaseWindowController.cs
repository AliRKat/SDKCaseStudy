using System;
using Code.UI.Buttons;
using ExampleGame.Code.Enums;
using UnityEngine;

namespace ExampleGame.Code.UI {

    public class BaseWindowController : MonoBehaviour {
        [SerializeField] private GameActionButton closeButton;
        public event Action OnClosed;

        public void Init(object data) {
            closeButton.RegisterClick(GameAction.CloseWindow, this);
        }

        public void Close() {
            OnClosed?.Invoke();
            Destroy(gameObject);
        }
    }

}