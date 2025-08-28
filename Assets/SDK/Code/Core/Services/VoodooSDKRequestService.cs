using System;
using System.Collections.Generic;
using System.Linq;
using SDK.Code.Core.Handlers;
using SDK.Code.Interfaces;
using SDK.Code.Models;
using SDK.Code.Utils;
using UnityEngine;

namespace SDK.Code.Core.Services {

    public class VoodooSDKRequestService : IRequestService {
        private readonly string _resourcePath;
        private Dictionary<string, TextAsset> _offersCache;
        private VoodooSDKLogHandler Log;

        public VoodooSDKRequestService(VoodooSDKLogHandler logHandler, string resourcePath = "Offers/") {
            _resourcePath = resourcePath;
            Log = logHandler;
        }

        /// <summary>
        ///     Retrieves offers for the given resource key on the Unity main thread.
        /// </summary>
        /// <param name="resourceKey">
        ///     The resource key identifying which JSON file to load (e.g., "singleOffers").
        ///     Must match the <c>TextAsset</c> names preloaded into the cache.
        /// </param>
        /// <param name="userSegments">
        ///     Optional user segmentation data used to tailor offer eligibility
        ///     during <see cref="OfferParser.MapOffer" /> evaluation.
        /// </param>
        /// <param name="onResponse">
        ///     Callback invoked on the main thread with the list of parsed offers.
        ///     - If the cache is uninitialized, corrupted, or the JSON is invalid, an empty list is provided.
        ///     - If mapping a specific offer fails, it is skipped and logged, but other offers are still returned.
        /// </param>
        /// <remarks>
        ///     <para>
        ///         In a production environment, this method would normally send a request to the server
        ///         and process the response.
        ///         However, since the system is running with mock data, it instead loads a local JSON file
        ///         from <c>Resources</c> and parses it into offers.
        ///     </para>
        ///     <para>
        ///         This method ensures all Unity API calls (<c>Resources.Load</c>, <c>JsonUtility.FromJson</c>)
        ///         are executed safely on the Unity main thread via <see cref="VoodooSDKMainThreadDispatcher" />.
        ///     </para>
        ///     <para>
        ///         The method is fail-safe: exceptions at every stage (cache, JSON parse, offer mapping)
        ///         are caught, logged, and result in <paramref name="onResponse" /> being invoked with a valid list
        ///         (possibly empty), so that UI or business logic never blocks.
        ///     </para>
        /// </remarks>
        public void GetOffers(string resourceKey, Dictionary<string, string> userSegments,
            Action<List<Offer>> onResponse) {
            VoodooSDKMainThreadDispatcher.Enqueue(() => {
                try {
                    if (_offersCache == null) {
                        Log.Warning("[RequestService] Cache not initialized. Calling EnsureCache.");
                        _offersCache = new Dictionary<string, TextAsset>();
                        EnsureCache();
                    }

                    if (!_offersCache.TryGetValue(resourceKey, out var jsonAsset) || jsonAsset == null) {
                        Log.Error($"[RequestService] No cached asset for {resourceKey}");
                        onResponse?.Invoke(new List<Offer>());
                        return;
                    }

                    OfferListDTO dtoWrapper = null;
                    try {
                        dtoWrapper = JsonUtility.FromJson<OfferListDTO>(jsonAsset.text);
                    }
                    catch (Exception ex) {
                        Log.Error($"[RequestService] JsonUtility.FromJson failed for {resourceKey}: {ex}");
                        onResponse?.Invoke(new List<Offer>());
                        return;
                    }

                    if (dtoWrapper == null) {
                        Log.Error($"[RequestService] dtoWrapper is null for {resourceKey}. JSON text={jsonAsset.text}");
                        onResponse?.Invoke(new List<Offer>());
                        return;
                    }

                    if (dtoWrapper.offers == null) {
                        Log.Warning($"[RequestService] No offers array in {resourceKey}.json");
                        onResponse?.Invoke(new List<Offer>());
                        return;
                    }

                    var offers = dtoWrapper.offers
                        .Select(dto => {
                            try {
                                return OfferParser.MapOffer(dto, userSegments);
                            }
                            catch (Exception ex) {
                                Log.Error($"[RequestService] Error mapping offer {dto?.id}: {ex}");
                                return null;
                            }
                        })
                        .Where(o => o != null)
                        .ToList();

                    Log.Info($"[RequestService] Returning {offers.Count} parsed offers from {resourceKey}.json");
                    onResponse?.Invoke(offers);
                }
                catch (Exception ex) {
                    Log.Error($"[RequestService] Exception in GetOffers: {ex}");
                    onResponse?.Invoke(new List<Offer>());
                }
            });
        }

        /// <summary>
        ///     Ensures that the offer cache is initialized before attempting to access it.
        ///     If the cache has not been built yet, it loads all offer JSON <see cref="TextAsset" /> files
        ///     from the <c>Resources</c> path.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This is a safeguard for mock data environments where offers are embedded as local JSON.
        ///         In production, a similar mechanism would validate server-side offer data or refresh stale caches.
        ///     </para>
        /// </remarks>
        private void EnsureCache() {
            if (_offersCache != null) return;
            try {
                var assets = Resources.LoadAll<TextAsset>(_resourcePath);
                if (assets == null || assets.Length == 0) {
                    Log.Warning($"[RequestService] No TextAssets found at {_resourcePath}");
                    _offersCache = new Dictionary<string, TextAsset>();
                    return;
                }

                _offersCache = assets.ToDictionary(asset => asset.name, asset => asset);
                Log.Info($"[RequestService] Cached {_offersCache.Count} offer files from {_resourcePath}");
            }
            catch (Exception ex) {
                Log.Error($"[RequestService] Exception while caching offers: {ex}");
            }
        }
    }

}