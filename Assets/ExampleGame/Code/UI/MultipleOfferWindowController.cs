using System;
using Code.UI.Buttons;
using ExampleGame.Code.Enums;
using UnityEngine;

namespace ExampleGame.Code.UI {

    public class MultipleOfferWindowController : MonoBehaviour {
        [SerializeField] private GameActionButton closeButton;
        [SerializeField] private OfferRepresentationController offerRepresentationPrefab;
        [SerializeField] private Transform offerListContainer;

        public event Action OnClosed;

        public void Init(object data) {
            if (data is MultipleOfferWindowInitData multipleOfferWindowInitData)
                foreach (var offer in multipleOfferWindowInitData.offerList) {
                    var offerRepresentation = Instantiate(offerRepresentationPrefab, offerListContainer);
                    offerRepresentation.Init(offer);
                }

            closeButton.RegisterClick(GameAction.CloseWindow, this);
        }

        public void Close() {
            OnClosed?.Invoke();
            Destroy(gameObject);
        }
    }

}