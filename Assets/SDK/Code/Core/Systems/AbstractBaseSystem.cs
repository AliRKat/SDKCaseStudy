using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SDK.Code.Core.Handlers;

namespace SDK.Code.Core.Systems {

    public abstract class AbstractBaseSystem {
        protected readonly VoodooSDKConfiguration Configuration;
        internal List<AbstractBaseSystem> Listeners { get; set; }
        protected VoodooSDKLogHandler Log { get; }

        protected AbstractBaseSystem(VoodooSDKConfiguration configuration, VoodooSDKLogHandler logHandler) {
            Configuration = configuration;
            Log = logHandler;
        }

        internal bool EnsureSDKInitialized([CallerMemberName] string caller = "") {
            if (VoodooSDK.Instance.IsSDKInitialized) return true;
            Log.Warning($"[VoodooSDK][{caller}] SDK is not initialized. Returning.");
            return false;
        }

        internal virtual void OnSessionStarted() {
        }

        internal virtual void OnSessionUpdate() {
        }
        
        internal virtual void OnSessionEnded() {
        }

        
    }

}