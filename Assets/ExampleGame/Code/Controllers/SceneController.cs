using Code.UI.Buttons;
using ExampleGame.Code.Enums;
using UnityEngine;

public class SceneController : MonoBehaviour {
    [Header("Game Action Buttons")]
    [SerializeField]
    private GameActionButton singleOfferButton;

    [SerializeField] private GameActionButton chainedOfferButton;

    [SerializeField] private GameActionButton endlessOfferButton;
    [SerializeField] private GameActionButton multipleOfferButton;

    private void Start() {
        singleOfferButton.RegisterClick(GameAction.ShowSingleOffer);
        chainedOfferButton.RegisterClick(GameAction.ShowChainedOffer);
        endlessOfferButton.RegisterClick(GameAction.ShowEndlessOffer);
        multipleOfferButton.RegisterClick(GameAction.ShowMultipleOffer);
    }
}