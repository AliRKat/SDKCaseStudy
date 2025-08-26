using System;
using System.Collections.Generic;
using System.Linq;
using SDK.Code.Core.Handlers;
using SDK.Code.Core.Services;
using SDK.Code.Interfaces;
using SDK.Code.Models;
using UnityEngine;

namespace SDK.Code.Core.Systems {

    public class VoodooSDKOfferSystem : AbstractBaseSystem, IOfferModule {
        private Dictionary<string, Offer> _byId = new();
        private Dictionary<string, List<Offer>> _byTrigger = new();
        private int _endlessCursor;
        private List<Offer> _offers;
        private VoodooSDKRequestService voodooSDKRequestService;

        public VoodooSDKOfferSystem(VoodooSDKConfiguration configuration,
            VoodooSDKLogHandler logHandler,
            VoodooSDKRequestService requestService)
            : base(configuration, logHandler) {
            voodooSDKRequestService = requestService;
        }

        // --- Public API ---

        public void GetSingleOfferManual(Action<Offer> callback) {
            RequestOffers(OfferType.Single, offers => {
                var eligible = offers.Where(o => o.Trigger == "MANUAL_SHOW").ToList();
                var offer = eligible.FirstOrDefault(o => o.Type == OfferType.Single);

                Log.Info(offer != null
                    ? $"[OfferSystem][GetSingleOfferManual] Selected Manual Single Offer: {offer.Id}"
                    : "[OfferSystem][GetSingleOfferManual] No manual single offer found.");

                callback?.Invoke(offer);
            });
        }

        public void GetSingleOffer(string trigger, Action<Offer> callback) {
            RequestOffers(OfferType.Single, offers => {
                var eligible = offers.Where(o => o.Trigger == trigger).ToList();
                var offer = eligible.FirstOrDefault(o => o.Type == OfferType.Single);

                Log.Info(offer != null
                    ? $"[OfferSystem][GetSingleOffer] Selected Single Offer for {trigger}: {offer.Id}"
                    : $"[OfferSystem][GetSingleOffer] No eligible single offer for {trigger}");

                callback?.Invoke(offer);
            });
        }

        public List<Offer> GetChainedOffers() {
            return null;
        }

        public List<Offer> GetEndlessOffers() {
            return null;
        }

        public List<Offer> GetMultipleOffers() {
            return null;
        }

        // --- Internal ---

        private void RequestOffers(OfferType type, Action<List<Offer>> callback) {
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
                    callback?.Invoke(new List<Offer>());
                    return;
                }

                var dtoWrapper = JsonUtility.FromJson<OfferListDTO>(json);
                if (dtoWrapper?.offers == null) {
                    Log.Error("[OfferSystem] Failed to parse offers from JSON!");
                    callback?.Invoke(new List<Offer>());
                    return;
                }

                var mapped = dtoWrapper.offers.Select(dto =>
                    new Offer(
                        dto.id,
                        ParseOfferType(dto.type),
                        dto.trigger,
                        dto.targetSegments,
                        new OfferPrice(dto.price.currency, dto.price.amount),
                        dto.rewards?.ConvertAll(r => new OfferReward(r.itemId, r.amount)) ?? new List<OfferReward>(),
                        dto.nextOfferId,
                        null
                    )
                ).ToList();

                _offers = mapped;
                BuildIndexes();

                callback?.Invoke(mapped);
            });
        }

        private static OfferType ParseOfferType(string t) {
            return t == "Chained" ? OfferType.Chained :
                t == "Endless" ? OfferType.Endless :
                t == "Multiple" ? OfferType.Multiple : OfferType.Single;
        }

        private void BuildIndexes() {
            _byTrigger.Clear();
            foreach (var offer in _offers) {
                var key = string.IsNullOrEmpty(offer.Trigger) ? string.Empty : offer.Trigger;
                if (!_byTrigger.TryGetValue(key, out var list)) {
                    list = new List<Offer>();
                    _byTrigger[key] = list;
                }

                list.Add(offer);
            }

            _byId = new Dictionary<string, Offer>();
            foreach (var o in _offers) _byId[o.Id] = o;
        }

        private List<Offer> GetEligibleOffers(string trigger) {
            if (_byTrigger.TryGetValue(trigger, out var candidates))
                return candidates;
            return new List<Offer>();
        }
    }

}