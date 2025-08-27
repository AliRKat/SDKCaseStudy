using System;
using System.Collections.Generic;
using System.Linq;
using SDK.Code.Core.Handlers;
using SDK.Code.Core.Services;
using SDK.Code.Interfaces;
using SDK.Code.Models;

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

        public List<Offer> GetChainedOffers() {
            return null;
        }

        public List<Offer> GetEndlessOffers() {
            return null;
        }

        public List<Offer> GetMultipleOffers() {
            return null;
        }

        // --- Public API ---

        public void GetSingleOfferManual(IGameStateProvider state, Action<Offer> callback,
            Dictionary<string, string> userSegments = null) {
            RequestOffers(OfferType.Single, userSegments, offers => {
                var eligible = GetEligibleOffers("MANUAL_SHOW", state);
                var offer = eligible.FirstOrDefault(o => o.Type == OfferType.Single);

                Log.Info(offer != null
                    ? $"[OfferSystem] Selected Manual Single Offer: {offer.Id}"
                    : "[OfferSystem] No manual single offer found.");

                callback?.Invoke(offer);
            });
        }

        public void GetSingleOffer(string trigger, IGameStateProvider state, Action<Offer> callback,
            Dictionary<string, string> userSegments = null) {
            RequestOffers(OfferType.Single, userSegments, offers => {
                var eligible = GetEligibleOffers(trigger, state);
                var offer = eligible.FirstOrDefault(o => o.Type == OfferType.Single);

                Log.Info(offer != null
                    ? $"[OfferSystem] Selected Single Offer for {trigger}: {offer.Id}"
                    : $"[OfferSystem] No eligible single offer for {trigger}");

                callback?.Invoke(offer);
            });
        }

        // --- Internal ---

        private void RequestOffers(OfferType type, Dictionary<string, string> userSegments,
            Action<List<Offer>> callback) {
            var resourceKey = type switch {
                OfferType.Single => "singleOffers",
                OfferType.Multiple => "multipleOffers",
                OfferType.Chained => "chainedOffers",
                OfferType.Endless => "endlessOffers",
                _ => "singleOffers"
            };

            voodooSDKRequestService.GetOffers(resourceKey, userSegments, offers => {
                _offers = offers.Where(o => o.Type == type).ToList();
                BuildIndexes();
                callback?.Invoke(_offers);
            });
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

        private List<Offer> GetEligibleOffers(string trigger, IGameStateProvider state) {
            var result = new List<Offer>();

            if (_byTrigger.TryGetValue(trigger, out var candidates))
                foreach (var o in candidates)
                    if (o.Validate(state))
                        result.Add(o);

            if (!_byTrigger.TryGetValue(string.Empty, out var alwaysOn)) return result;
            {
                foreach (var o in alwaysOn)
                    if (o.Validate(state))
                        result.Add(o);
            }

            return result;
        }
    }

}