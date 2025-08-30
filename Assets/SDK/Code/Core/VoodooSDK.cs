using System;
using System.Collections.Generic;
using System.IO;
using SDK.Code.Core.Handlers;
using SDK.Code.Core.Services;
using SDK.Code.Core.Systems;
using SDK.Code.Interfaces;
using UnityEngine;

namespace SDK.Code.Core {

    public class VoodooSDK : MonoBehaviour {
        private static VoodooSDK _instance;
        private List<AbstractBaseSystem> _listeners = new();

        private VoodooSDKConfiguration Configuration;

        /// <summary>
        ///     Exposes functionality to get Offers
        /// </summary>
        public IOfferModule OfferSystem;

        private VoodooSDKRequestService RequestService;
        private VoodooSDKLogHandler SDKLogHandler;
        private VoodooSDKSessionSystem SessionSystem;

        public static VoodooSDK Instance {
            get {
                if (_instance == null) {
                    var gameObject = new GameObject("VoodooSDK");
                    _instance = gameObject.AddComponent<VoodooSDK>();
                    gameObject.AddComponent<VoodooSDKMainThreadDispatcher>();
                }

                return _instance;
            }
            private set => _instance = value;
        }

        /// <summary>
        ///     Check if SDK has been initialized.
        /// </summary>
        /// <returns>bool</returns>
        public bool IsSDKInitialized { get; private set; }

        private void Awake() {
            DontDestroyOnLoad(gameObject);
            Instance = this;
            if (GetComponent<VoodooSDKMainThreadDispatcher>() == null)
                gameObject.AddComponent<VoodooSDKMainThreadDispatcher>();
        }

        private void OnApplicationPause(bool pauseStatus) {
            if (pauseStatus) {
                if (!Configuration.IsAutomaticSessionTrackingDisabled()) _ = SessionSystem?.EndSessionAsync();
            }
            else {
                if (!Configuration.IsAutomaticSessionTrackingDisabled()) _ = SessionSystem?.BeginSessionAsync();
            }
        }

        private void OnApplicationQuit() {
            ClearStorage();
            SessionSystem?._sessionTimer?.Dispose();
        }

        /// <summary>
        ///     Initializes the Voodoo SDK with the given configuration.
        ///     Sets up logging, and initializes all subsystems.
        ///     This method should be called once at application startup.
        /// </summary>
        /// <param name="voodooSDKConfiguration">
        ///     The configuration object containing app key, server URL, and optional parameters.
        /// </param>
        /// <remarks>
        ///     If the SDK is already initialized, the method logs an error and returns without reinitializing.
        /// </remarks>
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
            SessionSystem = new VoodooSDKSessionSystem(Configuration, SDKLogHandler);

            _listeners.Clear();
            _listeners.Add((VoodooSDKOfferSystem)OfferSystem);
            _listeners.Add(SessionSystem);

            SessionSystem.Listeners = _listeners;
            OnInitializationCompleted();
        }

        private void OnInitializationCompleted() {
            SDKLogHandler.Debug("[VoodooSDK][OnInitializationCompleted] VoodooSDK Initialized");
            IsSDKInitialized = true;

            // start the session after initializing the sdk
            SessionSystem.Init();
        }

        private static void ClearStorage() {
            var path = Application.persistentDataPath;

            try {
                if (Directory.Exists(path)) {
                    // Delete all files
                    foreach (var file in Directory.GetFiles(path)) File.Delete(file);

                    // Delete all directories
                    foreach (var dir in Directory.GetDirectories(path)) Directory.Delete(dir, true);

                    Debug.Log($"[StorageUtils] Cleared persistent storage at {path}");
                }
                else {
                    Debug.LogWarning($"[StorageUtils] Persistent data path not found: {path}");
                }
            }
            catch (Exception ex) {
                Debug.LogError($"[StorageUtils] Failed to clear storage: {ex}");
            }
        }
    }

}