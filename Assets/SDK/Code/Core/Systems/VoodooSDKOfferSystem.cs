using System;
using System.Collections.Generic;
using System.Linq;
using ExampleGame.Code.Enums;
using ExampleGame.Code.Managers;
using SDK.Code.Core.Enums;
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

        #region Public API

        /// <summary>
        ///     Retrieves a manually triggered single offer.
        ///     Requests all available single offers, then filters for eligibility
        ///     using the "MANUAL_SHOW" trigger and the provided game state.
        ///     If an eligible offer is found, it is passed to the callback.
        /// </summary>
        /// <param name="state">Current game state provider used to evaluate offer eligibility.</param>
        /// <param name="callback">Callback invoked with the selected offer or null if none found.</param>
        /// <param name="userSegments">
        ///     Optional user segment data to customize the offer request.
        ///     Defaults to null.
        /// </param>
        public void GetSingleOfferManual(IGameStateProvider state, Action<Offer> callback,
            Dictionary<string, string> userSegments = null) {
            if (!EnsureSDKInitialized()) return;

            RequestOffers(OfferType.Single, userSegments, offers => {
                var eligible = GetEligibleOffers("MANUAL_SHOW", state);
                var offer = eligible.FirstOrDefault(o => o.Type == OfferType.Single);

                Log.Info(offer != null
                    ? $"[OfferSystem] Selected Manual Single Offer: {offer.Id}"
                    : "[OfferSystem] No manual single offer found.");

                callback?.Invoke(offer);
            });
        }

        /// <summary>
        ///     Retrieves a single offer based on a trigger event.
        ///     Requests all available single offers, then filters for eligibility
        ///     using the specified trigger and the provided game state.
        ///     If an eligible offer is found, it is passed to the callback.
        /// </summary>
        /// <param name="trigger">
        ///     Event trigger identifier (e.g., "LEVEL_COMPLETE") that determines
        ///     which offers are eligible.
        /// </param>
        /// <param name="state">Current game state provider used to evaluate offer eligibility.</param>
        /// <param name="callback">Callback invoked with the selected offer or null if none found.</param>
        /// <param name="userSegments">
        ///     Optional user segment data to customize the offer request.
        ///     Defaults to null.
        /// </param>
        public void GetSingleOffer(string trigger, IGameStateProvider state, Action<Offer> callback,
            Dictionary<string, string> userSegments = null) {
            if (!EnsureSDKInitialized()) return;

            if (!Configuration.IsAutoShowOffersEnabled()) {
                Debug.LogWarning(
                    "[VoodooSDKOfferSystem][GetSingleOffer] Automatic offer showing is disabled. Returning.");
                return;
            }

            userSegments ??= state.GetUserSegmentation();

            RequestOffers(OfferType.Single, userSegments, offers => {
                var eligible = GetEligibleOffers(trigger, state);
                var offer = eligible.FirstOrDefault(o => o.Type == OfferType.Single);

                Log.Info(offer != null
                    ? $"[OfferSystem] Selected Single Offer for {trigger}: {offer.Id}"
                    : $"[OfferSystem] No eligible single offer for {trigger}");

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

        #endregion

        #region Internal

        /// <summary>
        ///     Requests offers of a given type from the remote service.
        ///     Filters the response by <see cref="OfferType" /> and rebuilds internal indexes.
        ///     The filtered offers are then returned through the callback.
        /// </summary>
        /// <param name="type">The type of offers to request (Single, Multiple, Chained, Endless).</param>
        /// <param name="userSegments">
        ///     Optional user segment data used to personalize the request.
        ///     Defaults to null.
        /// </param>
        /// <param name="callback">
        ///     Callback invoked with the list of offers matching the requested type.
        /// </param>
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

        /// <summary>
        ///     Rebuilds internal lookup indexes for quick access to offers by trigger and by ID.
        ///     Populates <c>_byTrigger</c> with offers grouped by their trigger,
        ///     and <c>_byId</c> with offers keyed by their unique identifier.
        /// </summary>
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

        /// <summary>
        ///     Retrieves all offers that are eligible for the given trigger and game state.
        ///     First, offers matching the trigger are validated against the game state.
        ///     Additionally, any "always-on" offers (those without a trigger) are also validated.
        /// </summary>
        /// <param name="trigger">
        ///     The trigger key used to filter candidate offers (e.g., "LEVEL_COMPLETE").
        ///     Pass an empty string to include only "always-on" offers.
        /// </param>
        /// <param name="state">The current game state used to validate offer eligibility.</param>
        /// <returns>
        ///     A list of offers that are valid for the given trigger and game state.
        /// </returns>
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

        #endregion

        #region Override Methods

        internal override void OnSessionStarted() {
            Log.Debug("[OfferSystem][OnSessionStarted] Session started.");
            GetSingleOffer(SDKEventKeys.SessionStart, Configuration.GetGameStateProvider(), offer => {
                if (offer != null) {
                    Log.Debug($"[OfferSystem][OnSessionStarted] Showing level complete offer: {offer}");
                    UIManager.Instance.LoadPopUpWindow(WindowType.SingleOffer);
                }
                else {
                    Log.Warning("[OfferSystem][OnSessionStarted] No eligible offer found for LEVEL_COMPLETE");
                }
            });
        }
        
        internal override void OnSessionUpdate() {
            Log.Debug("[OfferSystem] Session updated.");
        }
        
        internal override void OnSessionEnded() {
            Log.Debug("[OfferSystem] Session ended.");
        }

        #endregion
    }

}