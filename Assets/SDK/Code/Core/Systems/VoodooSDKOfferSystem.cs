using System;
using System.Collections.Generic;
using System.Linq;
using SDK.Code.Core.Enums;
using SDK.Code.Core.Handlers;
using SDK.Code.Core.Services;
using SDK.Code.Core.Strategy;
using SDK.Code.Interfaces;
using SDK.Code.Models;
using UnityEngine;

namespace SDK.Code.Core.Systems {

    public class VoodooSDKOfferSystem : AbstractBaseSystem, IOfferModule {
        private readonly IMultipleOfferSelectionStrategy _multipleOfferSelectionStrategy;
        private readonly Dictionary<string, Offer> _offersById = new();
        private Dictionary<string, List<Offer>> _byTrigger = new();
        private int _endlessCursor;
        private List<MultipleOffer> _multipleOffers;
        private List<Offer> _offers;

        private VoodooSDKRequestService voodooSDKRequestService;

        public VoodooSDKOfferSystem(VoodooSDKConfiguration configuration,
            VoodooSDKLogHandler logHandler,
            VoodooSDKRequestService requestService)
            : base(configuration, logHandler) {
            voodooSDKRequestService = requestService;
            _multipleOfferSelectionStrategy = new RotationMultipleOfferSelectionStrategy();
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
        public void GetSingleOfferManual(
            IGameStateProvider state,
            Action<Offer> callback,
            Dictionary<string, string> userSegments = null) {
            if (!EnsureSDKInitialized()) return;

            userSegments ??= state.GetUserSegmentation();

            RequestOffers(OfferType.Single, userSegments, offers => {
                var eligible = GetEligibleOffers("MANUAL_SHOW", state)
                    .Where(o => o.Type == OfferType.Single)
                    .ToList();

                var strategy = Configuration.OfferSelectionStrategy ?? new RotationOfferSelectionStrategy();
                var offer = strategy.Select(eligible, "MANUAL_SHOW", state);

                Log.Info(offer != null
                    ? $"[OfferSystem] Selected Manual Single Offer: {offer}"
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
                var eligible = GetEligibleOffers(trigger, state)
                    .Where(o => o.Type == OfferType.Single)
                    .ToList();

                // RotationOfferSelectionStrategy is the default
                var strategy = Configuration.OfferSelectionStrategy ?? new RotationOfferSelectionStrategy();
                var offer = strategy.Select(eligible, trigger, state);

                Log.Info(offer != null
                    ? $"[OfferSystem] Selected Single Offer for {trigger}: {offer}"
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

        public void GetMultipleOffers(
            string trigger,
            IGameStateProvider state,
            Action<MultipleOffer> callback,
            Dictionary<string, string> userSegments = null) {
            if (!EnsureSDKInitialized()) return;

            userSegments ??= state.GetUserSegmentation();

            voodooSDKRequestService.GetMultipleOffers(userSegments, offers => {
                var eligible = offers
                    .Where(o => o.Trigger == trigger && o.IsEligible(state))
                    .ToList();

                var selected = _multipleOfferSelectionStrategy.Select(eligible);

                Log.Info(selected != null
                    ? $"[OfferSystem] Selected MultipleOffer: {selected.Id}"
                    : $"[OfferSystem] No eligible multiple offers for trigger {trigger}");

                _multipleOffers = offers;
                BuildIndexes(_offers, _multipleOffers);
                callback?.Invoke(selected);
            });
        }

        /// <summary>
        ///     Marks an offer as purchased by its unique identifier.
        /// </summary>
        /// <param name="offerId">
        ///     The unique string identifier of the offer to purchase (as defined in the offer JSON).
        /// </param>
        /// <param name="callback">
        ///     Callback invoked once the purchase process has completed:
        ///     <list type="bullet">
        ///         <item>
        ///             <description>If the purchase succeeds, the purchased <see cref="Offer" /> is passed.</description>
        ///         </item>
        ///         <item>
        ///             <description>If the offer could not be found or the purchase fails, <c>null</c> is passed.</description>
        ///         </item>
        ///     </list>
        /// </param>
        public void BuyOfferWithId(string offerId, Action<Offer> callback) {
            if (!EnsureSDKInitialized()) {
                callback?.Invoke(null);
                return;
            }

            if (string.IsNullOrEmpty(offerId)) {
                Log.Warning("[OfferSystem] BuyOfferWithId called with null/empty id.");
                callback?.Invoke(null);
                return;
            }

            if (!_offersById.TryGetValue(offerId, out var offer)) {
                Log.Warning($"[OfferSystem] Offer with id {offerId} not found in indexed offers.");
                callback?.Invoke(null);
                return;
            }

            voodooSDKRequestService.MarkOfferAsPurchased(offer, success => {
                if (success) {
                    Log.Info($"[OfferSystem] Offer purchased: {offer.Id}");
                    callback?.Invoke(offer);
                }
                else {
                    Log.Error($"[OfferSystem] Failed to persist purchased offer: {offer.Id}");
                    callback?.Invoke(null);
                }
            });
        }

        /// <summary>
        ///     Retrieves a single offer by its unique identifier.
        /// </summary>
        /// <param name="offerId">
        ///     The unique string identifier of the offer (as defined in the offer JSON).
        /// </param>
        /// <param name="callback">
        ///     Callback invoked once the lookup is complete:
        ///     <list type="bullet">
        ///         <item>
        ///             <description>If an offer with the given ID exists, the <see cref="Offer" /> instance is provided.</description>
        ///         </item>
        ///         <item>
        ///             <description>If no offer is found, <c>null</c> is passed.</description>
        ///         </item>
        ///     </list>
        /// </param>
        /// <param name="userSegments">
        ///     Optional user segmentation data used when mapping offers.
        ///     If <c>null</c>, defaults will be applied by the offer system.
        /// </param>
        public void GetOfferById(string offerId, Action<Offer> callback,
            Dictionary<string, string> userSegments = null) {
            if (!EnsureSDKInitialized()) {
                callback?.Invoke(null);
                return;
            }

            if (string.IsNullOrEmpty(offerId)) {
                Log.Warning("[OfferSystem] GetOfferById called with null/empty id.");
                callback?.Invoke(null);
                return;
            }

            if (_offersById.TryGetValue(offerId, out var offer)) {
                Log.Info($"[OfferSystem] Found offer by id: {offer.Id}");
                callback?.Invoke(offer);
            }
            else {
                Log.Warning($"[OfferSystem] Offer with id {offerId} not found in indexed offers.");
                callback?.Invoke(null);
            }
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
                BuildIndexes(_offers);
                callback?.Invoke(_offers);
            });
        }

        private void BuildIndexes(List<Offer> offers, List<MultipleOffer> multipleOffers = null) {
            _offersById.Clear();
            _byTrigger = new Dictionary<string, List<Offer>>(StringComparer.OrdinalIgnoreCase);

            if (offers != null)
                foreach (var o in offers) {
                    if (string.IsNullOrEmpty(o.Id)) {
                        Log.Warning("[OfferSystem] Skipping offer with null/empty id during indexing.");
                        continue;
                    }

                    if (_offersById.ContainsKey(o.Id))
                        Log.Warning($"[OfferSystem] Duplicate offer id detected: {o.Id}. Overwriting previous entry.");
                    _offersById[o.Id] = o;

                    var key = string.IsNullOrEmpty(o.Trigger) ? string.Empty : o.Trigger;
                    if (!_byTrigger.TryGetValue(key, out var list)) {
                        list = new List<Offer>();
                        _byTrigger[key] = list;
                    }

                    list.Add(o);
                }

            if (multipleOffers != null)
                foreach (var m in multipleOffers) {
                    if (m?.Offers == null) continue;

                    foreach (var sub in m.Offers) {
                        if (string.IsNullOrEmpty(sub.Id)) {
                            Log.Warning("[OfferSystem] Skipping sub-offer with null/empty id during indexing.");
                            continue;
                        }

                        if (_offersById.ContainsKey(sub.Id))
                            Log.Warning(
                                $"[OfferSystem] Duplicate sub-offer id detected: {sub.Id}. Overwriting previous entry.");
                        _offersById[sub.Id] = sub;
                    }
                }

            Log.Info($"[OfferSystem] Indexed total {_offersById.Count} offers (including sub-offers). " +
                     $"Triggers: {_byTrigger?.Count ?? 0} keys -> [{string.Join(", ", _byTrigger.Keys)}]");
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
                    Log.Debug($"[OfferSystem][OnSessionStarted] Eligible offer found: {offer}");

                    var callback = Configuration.GetOfferReadyAction();
                    callback?.Invoke(offer);
                }
                else {
                    Log.Warning("[OfferSystem][OnSessionStarted] No eligible offer found for SESSION_START");
                }
            });
        }

        internal override void OnSessionUpdate() {
            Log.Debug("[OfferSystem] Session updated.");
            GetSingleOffer(SDKEventKeys.SessionUpdate, Configuration.GetGameStateProvider(), offer => {
                if (offer != null) {
                    Log.Debug($"[OfferSystem][OnSessionUpdate] Eligible offer found: {offer}");

                    var callback = Configuration.GetOfferReadyAction();
                    callback?.Invoke(offer);
                }
                else {
                    Log.Warning("[OfferSystem][OnSessionUpdate] No eligible offer found for SESSION_UPDATE");
                }
            });
        }

        internal override void OnSessionEnded() {
            Log.Debug("[OfferSystem] Session ended.");
        }

        #endregion
    }

}