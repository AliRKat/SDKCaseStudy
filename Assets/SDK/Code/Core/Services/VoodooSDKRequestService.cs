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
        private VoodooSDKLogHandler Log;

        public VoodooSDKRequestService(VoodooSDKLogHandler logHandler, string resourcePath = "Offers/") {
            _resourcePath = resourcePath;
            Log = logHandler;
        }

        /// <summary>
        ///     Retrieves offers for the given resource key.
        ///     In a production environment, this would normally send a request to the server
        ///     and process the response.
        ///     However, since the system is running with mock data, it instead loads a local
        ///     JSON file from <c>Resources</c> and parses it into offers.
        /// </summary>
        /// <param name="resourceKey">
        ///     The resource key identifying which JSON file to load (e.g., "singleOffers").
        /// </param>
        /// <param name="userSegments">
        ///     Optional user segment data used to personalize the mapped offers.
        /// </param>
        /// <param name="onResponse">
        ///     Callback invoked with the list of parsed offers.
        ///     An empty list is provided if the JSON file is missing or invalid.
        /// </param>
        public void GetOffers(string resourceKey, Dictionary<string, string> userSegments,
            Action<List<Offer>> onResponse) {
            var jsonAsset = Resources.Load<TextAsset>($"{_resourcePath}{resourceKey}");
            if (jsonAsset == null) {
                Log.Error($"[RequestService] Failed to load JSON at {_resourcePath}{resourceKey}");
                onResponse?.Invoke(new List<Offer>());
                return;
            }

            Log.Info($"[RequestService] Returning parsed offers from {resourceKey}.json");

            var dtoWrapper = JsonUtility.FromJson<OfferListDTO>(jsonAsset.text);
            if (dtoWrapper == null || dtoWrapper.offers == null) {
                onResponse?.Invoke(new List<Offer>());
                return;
            }

            var offers = dtoWrapper.offers
                .Select(dto => OfferParser.MapOffer(dto, userSegments))
                .Where(o => o != null)
                .ToList();

            onResponse?.Invoke(offers);
        }
    }

}