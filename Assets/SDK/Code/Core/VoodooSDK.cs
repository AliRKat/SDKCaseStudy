using SDK.Code.Core.Handlers;
using SDK.Code.Core.Services;
using SDK.Code.Core.Systems;
using SDK.Code.Interfaces;
using UnityEngine;

namespace SDK.Code.Core {

    public class VoodooSDK : MonoBehaviour {
        private static VoodooSDK _instance;
        private VoodooSDKConfiguration Configuration;

        /// <summary>
        ///     Exposes functionality to record events
        /// </summary>
        public VoodooSDKEventSystem EventSystem;

        /// <summary>
        ///     Exposes functionality to get Offers
        /// </summary>
        public IOfferModule OfferSystem;

        private VoodooSDKRequestService RequestService;
        private VoodooSDKLogHandler SDKLogHandler;

        public static VoodooSDK Instance {
            get {
                if (_instance == null) {
                    var gameObject = new GameObject("VoodooSDK");
                    _instance = gameObject.AddComponent<VoodooSDK>();
                }

                return _instance;
            }
            internal set => _instance = value;
        }

        /// <summary>
        ///     Check if SDK has been initialized.
        /// </summary>
        /// <returns>bool</returns>
        public bool IsSDKInitialized { get; private set; }

        public void Init(VoodooSDKConfiguration voodooSDKConfiguration) {
            if (IsSDKInitialized) {
                Debug.LogError("[VoodooSDK][Init] VoodooSDK is already initialized");
                return;
            }

            Configuration = voodooSDKConfiguration;
            SDKLogHandler = new VoodooSDKLogHandler(Configuration);

            if (Configuration.Parent != null) transform.parent = Configuration.Parent.transform;

            SDKLogHandler.Info(
                $"[Init] Initialized VoodooSDK with App Key: {Configuration.GetAppKey()} and Server URL: {Configuration.GetServerURL()}");
            InitSubSystems();
        }

        private void InitSubSystems() {
            RequestService = new VoodooSDKRequestService(SDKLogHandler);
            OfferSystem = new VoodooSDKOfferSystem(Configuration, SDKLogHandler, RequestService);
            EventSystem = new VoodooSDKEventSystem(Configuration, SDKLogHandler);
        }
    }

}