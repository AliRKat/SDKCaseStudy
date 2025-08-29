using System;
using Code.UI.Buttons;
using ExampleGame.Code.Enums;
using TMPro;
using UnityEngine;

namespace ExampleGame.Code.UI {

    [Serializable]
    public class OfferWindowInitData {
        public string offerId;
        public string price;
        public string reward;
    }

    public class BaseWindowController : MonoBehaviour {
        [SerializeField] private GameActionButton closeButton;
        [SerializeField] private GameActionButton buyOfferButton;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private TMP_Text rewardText;
        private object _initData;
        public event Action OnClosed;

        public void Init(object data) {
            _initData = data;
            if (data is OfferWindowInitData offerWindowInitData) {
                priceText.text = $"Price: {offerWindowInitData.price}";
                rewardText.text = $"Reward: {offerWindowInitData.reward}";

                closeButton.RegisterClick(GameAction.CloseWindow, this);
                buyOfferButton.RegisterClick(GameAction.BuyOffer, this);
            }
        }

        public T GetInitData<T>() where T : class {
            return _initData as T;
        }

        public void Close() {
            OnClosed?.Invoke();
            Destroy(gameObject);
        }
    }

}