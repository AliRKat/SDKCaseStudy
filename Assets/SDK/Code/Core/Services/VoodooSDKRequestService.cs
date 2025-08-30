using System;
using System.Collections.Generic;
using System.IO;
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
            var assets = Resources.LoadAll<TextAsset>(_resourcePath);
            Log.Info($"[RequestService] Loaded {assets?.Length ?? 0} assets from {_resourcePath}");

            if (assets == null || assets.Length == 0) {
                _offersCache = new Dictionary<string, TextAsset>();
                return;
            }

            foreach (var asset in assets) Log.Info($"[RequestService] Cached asset: {asset.name}");

            _offersCache = assets.ToDictionary(asset => asset.name, asset => asset);
        }

        /// <summary>
        ///     Marks an offer as purchased and persists the purchase state locally.
        /// </summary>
        /// <param name="offer">
        ///     The <see cref="Offer" /> object representing the purchased offer.
        ///     Only the <c>Id</c> is stored, as all other details can be retrieved
        ///     from the main offer configuration files (<c>singleOffers.json</c>, etc.).
        /// </param>
        /// <param name="onComplete">
        ///     Callback invoked once the purchase state has been processed:
        ///     <list type="bullet">
        ///         <item>
        ///             <description><c>true</c> if the offer was successfully marked as purchased or already present.</description>
        ///         </item>
        ///         <item>
        ///             <description><c>false</c> if an exception occurred during persistence.</description>
        ///         </item>
        ///     </list>
        /// </param>
        public void MarkOfferAsPurchased(Offer offer, Action<bool> onComplete) {
            VoodooSDKMainThreadDispatcher.Enqueue(() => {
                try {
                    var path = Path.Combine(Application.persistentDataPath, "boughtOffers.json");

                    BoughtOffersDTO dto;
                    if (File.Exists(path)) {
                        var json = File.ReadAllText(path);
                        dto = JsonUtility.FromJson<BoughtOffersDTO>(json) ?? new BoughtOffersDTO();
                    }
                    else {
                        dto = new BoughtOffersDTO();
                    }

                    if (dto.offerIds.Contains(offer.Id)) {
                        Log.Info($"[RequestService] Offer {offer.Id} already purchased.");
                        onComplete?.Invoke(true);
                        return;
                    }

                    dto.offerIds.Add(offer.Id);
                    File.WriteAllText(path, JsonUtility.ToJson(dto, true));

                    Log.Info($"[RequestService] Saved boughtOffers.json at {path}");
                    onComplete?.Invoke(true);
                }
                catch (Exception ex) {
                    Log.Error($"[RequestService] Failed to mark offer as purchased: {ex}");
                    onComplete?.Invoke(false);
                }
            });
        }

        public void GetMultipleOffers(Dictionary<string, string> userSegments, Action<List<MultipleOffer>> onResponse) {
            VoodooSDKMainThreadDispatcher.Enqueue(() => {
                try {
                    if (!_offersCache.TryGetValue("multipleOffers", out var jsonAsset) || jsonAsset == null) {
                        Log.Error("[RequestService] No cached asset for multipleOffers");
                        onResponse?.Invoke(new List<MultipleOffer>());
                        return;
                    }

                    var dtoWrapper = JsonUtility.FromJson<MultipleOfferListDTO>(jsonAsset.text);
                    if (dtoWrapper?.multipleOffers == null) {
                        onResponse?.Invoke(new List<MultipleOffer>());
                        return;
                    }

                    var mapped = dtoWrapper.multipleOffers
                        .Select(dto => MultipleOfferParser.Map(dto, userSegments))
                        .Where(o => o != null)
                        .ToList();

                    onResponse?.Invoke(mapped);
                }
                catch (Exception ex) {
                    Log.Error($"[RequestService] Exception in GetMultipleOffers: {ex}");
                    onResponse?.Invoke(new List<MultipleOffer>());
                }
            });
        }
    }

}