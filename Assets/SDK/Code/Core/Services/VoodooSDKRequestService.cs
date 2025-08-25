using System;
using SDK.Code.Core.Handlers;
using SDK.Code.Interfaces;
using UnityEngine;

namespace SDK.Code.Core.Services {

    public class VoodooSDKRequestService : IRequestService {
        private readonly string _resourcePath;
        private VoodooSDKLogHandler Log;

        public VoodooSDKRequestService(VoodooSDKLogHandler logHandler, string resourcePath = "Offers/") {
            _resourcePath = resourcePath;
            Log = logHandler;
        }

        public void GetOffers(string resourceKey, Action<string> onResponse) {
            var jsonAsset = Resources.Load<TextAsset>($"{_resourcePath}{resourceKey}");
            if (jsonAsset == null) {
                Log.Error($"[VoodooSDKRequestService] Failed to load JSON at {_resourcePath}{resourceKey}");
                onResponse?.Invoke(null);
                return;
            }

            Log.Info($"[VoodooSDKRequestService][GetOffers] Returning mock offer data from {resourceKey}.json");
            onResponse?.Invoke(jsonAsset.text);
        }
    }

}