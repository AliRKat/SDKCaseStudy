using System.Collections;
using System.Collections.Generic;
using Code.UI.Buttons;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    [Header("Game Action Buttons")]
    [SerializeField]
    private GameActionButton singleOfferButton;

    [SerializeField]
    private GameActionButton chainedOfferButton;

    [SerializeField]
    private GameActionButton endlessOfferButton;

    void Start()
    {
        singleOfferButton.RegisterClick(GameAction.ShowSingleOffer);
        chainedOfferButton.RegisterClick(GameAction.ShowChainedOffer);
        endlessOfferButton.RegisterClick(GameAction.ShowEndlessOffer);
    }
}
