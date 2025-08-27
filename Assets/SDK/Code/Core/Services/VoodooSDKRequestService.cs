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