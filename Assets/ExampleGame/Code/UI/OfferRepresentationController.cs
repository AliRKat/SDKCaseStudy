using Code.UI.Buttons;
using ExampleGame.Code.Enums;
using TMPro;
using UnityEngine;

namespace ExampleGame.Code.UI {

    public class OfferRepresentationController : MonoBehaviour {
        [SerializeField] private GameActionButton buyOfferButton;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private TMP_Text rewardText;
        private object _initData;

        public void Init(object data) {
            if (data is SingleOfferWindowInitData offerWindowInitData) {
                _initData = offerWindowInitData;
                priceText.text = $"Price: {offerWindowInitData.price}";
                rewardText.text = $"Reward: {offerWindowInitData.reward}";
                buyOfferButton.RegisterClick(GameAction.BuyOffer, _initData);
            }
        }
    }

}