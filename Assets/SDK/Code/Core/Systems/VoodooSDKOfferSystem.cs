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
            LoadOffers(OfferType.Single);
            var eligible = GetEligibleOffers("MANUAL_SHOW");
            var offer = eligible.FirstOrDefault(o => o.Type == OfferType.Single);
            Log.Info(offer != null
                ? $"[OfferSystem] Selected Manual Single Offer: {offer.Id}"
                : "[OfferSystem] No manual single offer found.");
            callback?.Invoke(offer);
        }

        public void GetSingleOffer(string trigger, Action<Offer> callback) {
            LoadOffers(OfferType.Single);
            var eligible = GetEligibleOffers(trigger);
            var offer = eligible.FirstOrDefault(o => o.Type == OfferType.Single);
            Log.Info(offer != null
                ? $"[OfferSystem] Selected Single Offer for {trigger}: {offer.Id}"
                : $"[OfferSystem] No eligible single offer for {trigger}");
            callback?.Invoke(offer);
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

                var mapped = new List<Offer>();
                foreach (var dto in dtoWrapper.offers) {
                    var price = new OfferPrice(dto.price.currency, dto.price.amount);
                    var rewards = dto.rewards != null
                        ? dto.rewards.ConvertAll(r => new OfferReward(r.itemId, r.amount))
                        : new List<OfferReward>();

                    mapped.Add(new Offer(
                        dto.id,
                        ParseOfferType(dto.type),
                        dto.trigger,
                        dto.targetSegments,
                        price,
                        rewards,
                        dto.nextOfferId,
                        null
                    ));
                }

                _offers = mapped.Where(o => o.Type == type).ToList();
                BuildIndexes();
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