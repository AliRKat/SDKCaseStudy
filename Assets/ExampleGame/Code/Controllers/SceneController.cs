using Code.UI.Buttons;
using ExampleGame.Code.Enums;
using UnityEngine;

namespace ExampleGame.Code.Controllers {

    public class SceneController : MonoBehaviour {
        [Header("Offer Event Buttons")]
        [SerializeField]
        private GameActionButton singleOfferButton;

        [SerializeField] private GameActionButton chainedOfferButton;
        [SerializeField] private GameActionButton endlessOfferButton;
        [SerializeField] private GameActionButton multipleOfferButton;

        [Header("Gameplay Event Buttons")]
        [SerializeField] private GameActionButton levelUpButton;
        [SerializeField] private GameActionButton completeStageButton;
        [SerializeField] private GameActionButton addCurrencyButton;
        [SerializeField] private GameActionButton swapPlayerTypeButton;
        [SerializeField] private GameActionButton swapRegionButton;

        private void Start() {
            singleOfferButton.RegisterClick(GameAction.ShowSingleOffer);
            chainedOfferButton.RegisterClick(GameAction.ShowChainedOffer);
            endlessOfferButton.RegisterClick(GameAction.ShowEndlessOffer);
            multipleOfferButton.RegisterClick(GameAction.ShowMultipleOffer);

            levelUpButton.RegisterClick(GameAction.LevelUp);
            completeStageButton.RegisterClick(GameAction.StageComplete);
            addCurrencyButton.RegisterClick(GameAction.AddCurrency);
            swapPlayerTypeButton.RegisterClick(GameAction.SwapPlayerType);
            swapRegionButton.RegisterClick(GameAction.SwapRegion);
        }
    }

}