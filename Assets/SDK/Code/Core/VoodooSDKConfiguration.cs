using System;
using UnityEngine;

namespace SDK.Code.Core {

    [Serializable]
    public class VoodooSDKConfiguration {
        public GameObject Parent;
        internal string AppKey;
        internal bool canPrintLogs;
        internal string ServerURL;

        public VoodooSDKConfiguration(string appKey, string serverUrl) {
            AppKey = appKey;
            ServerURL = serverUrl;
        }

        #region Setters

        public VoodooSDKConfiguration EnableLogging() {
            canPrintLogs = true;
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

        #endregion
    }

}