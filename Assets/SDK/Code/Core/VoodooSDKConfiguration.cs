using System;
using UnityEngine;

namespace SDK.Code.Core {

    [Serializable]
    public class VoodooSDKConfiguration {
        public GameObject Parent;
        internal string AppKey;
        internal bool autoShowOffers = true;
        internal bool canPrintLogs;
        internal string ServerURL;
        internal int sessionTimeout;
        internal bool isAutomaticSessionDisabled;

        public VoodooSDKConfiguration(string appKey, string serverUrl) {
            AppKey = appKey;
            ServerURL = serverUrl;
        }

        #region Setters

        public VoodooSDKConfiguration EnableLogging() {
            canPrintLogs = true;
            return this;
        }

        public VoodooSDKConfiguration DisableAutoShowOffers() {
            autoShowOffers = false;
            return this;
        }

        public VoodooSDKConfiguration DisableAutomaticSessions() {
            isAutomaticSessionDisabled = true;
            return this;
        }

        public VoodooSDKConfiguration SetSessionTimeout(int timeout) {
            sessionTimeout = timeout;
            return this;
        }

        #endregion

        #region Getters

        public string GetAppKey() {
            return AppKey;
        }

        public string GetServerURL() {
            return ServerURL;
        }

        public bool IsLoggingEnabled() {
            return canPrintLogs;
        }

        public bool IsAutoShowOffersEnabled() {
            return autoShowOffers;
        }

        public bool IsAutomaticSessionTrackingDisabled() {
            return isAutomaticSessionDisabled;
        }

        public int GetUpdateSessionTimerDelay() {
            return sessionTimeout;
        }
        

        #endregion
    }

}