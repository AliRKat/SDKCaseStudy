using System.Collections.Generic;
using SDK.Code.Core.Handlers;
using SDK.Code.Interfaces;
using SDK.Code.Models;

namespace SDK.Code.Core.Systems {

    public class VoodooSDKOfferSystem : AbstractBaseSystem, IOfferModule {
        public VoodooSDKOfferSystem(VoodooSDKConfiguration configuration, VoodooSDKLogHandler logHandler) : base(
            configuration, logHandler) {
        }

        public Offer GetSingleOffer() {
            Log.Info("[VoodooSDKOfferSystem][GetSingleOffer] Getting single offer.");
            return null;
        }

        public List<Offer> GetChainedOffers() {
            Log.Info("[VoodooSDKOfferSystem][GetChainedOffers] Getting chained offers.");
            return null;
        }

        public List<Offer> GetEndlessOffers() {
            Log.Info("[VoodooSDKOfferSystem][GetEndlessOffers] Getting endless offers.");
            return null;
        }

        public List<Offer> GetMultipleOffers() {
            Log.Info("[VoodooSDKOfferSystem][GetMultipleOffers] Getting multiple offers.");
            return null;
        }

        public bool ValidateOffer(string offerId) {
            return false;
        }

        public void PurchaseOffer(string offerId) {
        }

        public bool IsOfferAvailable(string offerId) {
            return false;
        }

        public List<Offer> GetAllActiveOffers() {
            return null;
        }
    }

}