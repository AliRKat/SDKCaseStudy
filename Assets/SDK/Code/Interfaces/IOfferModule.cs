using System;
using System.Collections.Generic;
using SDK.Code.Models;

namespace SDK.Code.Interfaces {

    public interface IOfferModule {
        public void GetSingleOfferManual(IGameStateProvider state, Action<Offer> callback);
        public void GetSingleOffer(string trigger, IGameStateProvider state, Action<Offer> callback);
        public List<Offer> GetMultipleOffers();
        public List<Offer> GetChainedOffers();

        public List<Offer> GetEndlessOffers();
        // public bool ValidateOffer(string offerId);
        // public void PurchaseOffer(string offerId);
        // public bool IsOfferAvailable(string offerId);
        // public List<Offer> GetAllActiveOffers();
    }

}