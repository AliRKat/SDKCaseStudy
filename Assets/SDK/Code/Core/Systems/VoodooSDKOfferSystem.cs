using System.Collections.Generic;
using SDK.Code.Core.Handlers;
using SDK.Code.Core.Services;
using SDK.Code.Interfaces;
using SDK.Code.Models;
using SDK.Code.Utils;
using UnityEngine;

namespace SDK.Code.Core.Systems {

    public class VoodooSDKOfferSystem : AbstractBaseSystem, IOfferModule {
        private List<Offer> _offers;
        private VoodooSDKRequestService voodooSDKRequestService;

        public VoodooSDKOfferSystem(VoodooSDKConfiguration configuration, VoodooSDKLogHandler logHandler,
            VoodooSDKRequestService requestService) : base(
            configuration, logHandler) {
            voodooSDKRequestService = requestService;
        }

        public Offer GetSingleOffer() {
            Log.Info("[VoodooSDKOfferSystem][GetSingleOffer] Getting single offer.");
            LoadOffers(OfferType.Single);
            return null;
        }

        public List<Offer> GetChainedOffers() {
            Log.Info("[VoodooSDKOfferSystem][GetChainedOffers] Getting chained offers.");
            LoadOffers(OfferType.Chained);
            return null;
        }

        public List<Offer> GetEndlessOffers() {
            Log.Info("[VoodooSDKOfferSystem][GetEndlessOffers] Getting endless offers.");
            LoadOffers(OfferType.Endless);
            return null;
        }

        public List<Offer> GetMultipleOffers() {
            Log.Info("[VoodooSDKOfferSystem][GetMultipleOffers] Getting multiple offers.");
            LoadOffers(OfferType.Multiple);
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

        private void LoadOffers(OfferType type) {
            var resourceKey = type switch {
                OfferType.Single => "singleOffers",
                OfferType.Multiple => "multipleOffers",
                OfferType.Chained => "chainedOffers",
                OfferType.Endless => "endlessOffers",
                _ => "singleOffers"
            };

            voodooSDKRequestService.GetOffers(resourceKey, json => {
                if (string.IsNullOrEmpty(json)) {
                    Log.Error("[OfferSystem] No offers returned!");
                    return;
                }

                var dtoWrapper = JsonUtility.FromJson<OfferListDTO>(json);
                if (dtoWrapper == null || dtoWrapper.offers == null) {
                    Log.Error("[OfferSystem] Failed to parse offers from JSON!");
                    return;
                }

                _offers = OfferParser.LoadOffersFromJson($"Offers/{resourceKey}");
                Log.Info($"[OfferSystem] Loaded {_offers.Count} offers for {type}");

                foreach (var offer in _offers)
                    Log.Debug(offer.ToString());
            });
        }
    }

}