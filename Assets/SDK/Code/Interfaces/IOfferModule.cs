using System;
using System.Collections.Generic;
using SDK.Code.Models;

namespace SDK.Code.Interfaces {

    public interface IOfferModule {
        public void GetSingleOfferManual(IGameStateProvider state, Action<Offer> callback,
            Dictionary<string, string> userSegments = null);

        public void GetSingleOffer(string trigger, IGameStateProvider state, Action<Offer> callback,
            Dictionary<string, string> userSegments = null);

        public List<Offer> GetMultipleOffers();
        public List<Offer> GetChainedOffers();
        public List<Offer> GetEndlessOffers();
        public void BuyOfferWithId(string offerId, Action<Offer> callback);

        public void GetOfferById(string offerId, Action<Offer> callback,
            Dictionary<string, string> userSegments = null);
    }

}