using SDK.Code.Core.Handlers;

namespace SDK.Code.Core.Systems {

    public abstract class AbstractBaseSystem {
        protected readonly VoodooSDKConfiguration Configuration;

        protected AbstractBaseSystem(VoodooSDKConfiguration configuration, VoodooSDKLogHandler logHandler) {
            Configuration = configuration;
            Log = logHandler;
        }

        protected VoodooSDKLogHandler Log { get; private set; }
    }

}