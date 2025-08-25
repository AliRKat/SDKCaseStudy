namespace SDK.Code.Core.Handlers {

    public class VoodooSDKLogHandler {
        private const string TAG = "[VoodooSDK]";
        private readonly VoodooSDKConfiguration _configuration;

        internal VoodooSDKLogHandler(VoodooSDKConfiguration configuration) {
            _configuration = configuration;
        }

        internal void Info(string message) {
            if (_configuration.IsLoggingEnabled()) UnityEngine.Debug.Log("[Info]" + TAG + message);
        }

        internal void Debug(string message) {
            if (_configuration.IsLoggingEnabled()) UnityEngine.Debug.Log("[Debug]" + TAG + message);
        }

        internal void Error(string message) {
            if (_configuration.IsLoggingEnabled()) UnityEngine.Debug.LogError("[Error]" + TAG + message);
        }

        internal void Warning(string message) {
            if (_configuration.IsLoggingEnabled()) UnityEngine.Debug.LogWarning("[Warning]" + TAG + message);
        }
    }

}