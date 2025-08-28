using System.Runtime.CompilerServices;
using SDK.Code.Core.Handlers;

namespace SDK.Code.Core.Systems {

    public abstract class AbstractBaseSystem {
        protected readonly VoodooSDKConfiguration Configuration;

        protected AbstractBaseSystem(VoodooSDKConfiguration configuration, VoodooSDKLogHandler logHandler) {
            Configuration = configuration;
            Log = logHandler;
        }

        protected VoodooSDKLogHandler Log { get; }

        internal bool EnsureSDKInitialized([CallerMemberName] string caller = "") {
            if (VoodooSDK.Instance.IsSDKInitialized) return true;
            Log.Warning($"[VoodooSDK][{caller}] SDK is not initialized. Returning.");
            return false;
        }
    }

}