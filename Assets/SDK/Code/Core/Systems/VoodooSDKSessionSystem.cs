using System;
using System.Threading;
using System.Threading.Tasks;
using SDK.Code.Core.Handlers;

namespace SDK.Code.Core.Systems
{
    public class VoodooSDKSessionSystem : AbstractBaseSystem
    {
        internal Timer _sessionTimer;
        private DateTime _sessionStartTime;
        private DateTime _lastUpdateTime;
        private string _sessionId;

        private bool IsActive { get; set; }

        public VoodooSDKSessionSystem(VoodooSDKConfiguration configuration, VoodooSDKLogHandler logHandler) 
            : base(configuration, logHandler) { }

        /// <summary>
        /// Initializes session tracking.
        /// If automatic session tracking is not disabled, automatically begins session.
        /// </summary>
        internal void Init() {
            if (!Configuration.IsAutomaticSessionTrackingDisabled()) {
                Log.Info("[SessionSystem] Automatic session tracking enabled.");
                _ = BeginSessionAsync();
            } else {
                Log.Info("[SessionSystem] Automatic session tracking disabled.");
            }
        }

        /// <summary>
        /// Begin a new session and start the timer loop.
        /// </summary>
        internal async Task BeginSessionAsync() {
            if (IsActive) {
                Log.Warning("[SessionSystem] Session already active, ignoring begin.");
                return;
            }

            _sessionStartTime = DateTime.UtcNow;
            _lastUpdateTime = _sessionStartTime;
            _sessionId = Guid.NewGuid().ToString();
            IsActive = true;

            InitSessionTimer();
            NotifyListenersOnStart();

            await MockRequest("begin_session");
            Log.Info($"[SessionSystem] Session begun at {_sessionStartTime}, id={_sessionId}");
        }

        /// <summary>
        /// End the session manually (called on quit/pause).
        /// </summary>
        internal async Task EndSessionAsync() {
            if (!IsActive) {
                Log.Warning("[SessionSystem] No active session to end.");
                return;
            }

            Log.Info("[SessionSystem] Ending session.");
            _sessionTimer?.Dispose();
            _sessionTimer = null;
            IsActive = false;

            await MockRequest("end_session");
            NotifyListenersOnEnd();
        }
        
        /// <summary>
        /// Internal tick update (called automatically by timer).
        /// </summary>
        private async Task UpdateSessionAsync() {
            if (!IsActive) return;

            _lastUpdateTime = DateTime.UtcNow;
            await MockRequest("update_session");
            Log.Debug("[SessionSystem] Session updated.");
            NotifyListenersOnUpdate();
        }

        private void InitSessionTimer() {
            _sessionTimer?.Dispose();
            int delayMs = Configuration.GetUpdateSessionTimerDelay() * 1000;

            _sessionTimer = new Timer(_ => { _ = UpdateSessionAsync(); }, null, delayMs, delayMs);
        }

        private async Task MockRequest(string type) {
            var requestParams = new {
                method = type,
                session_id = _sessionId ?? "unknown",
                timestamp = DateTime.UtcNow
            };

            Log.Debug($"[SessionSystem][Request] {type} → {requestParams}");

            // simulate network latency
            await Task.Delay(50);
        }

        private void NotifyListenersOnStart() {
            foreach (AbstractBaseSystem listener in Listeners)
                listener.OnSessionStarted();
        }

        private void NotifyListenersOnUpdate() {
            foreach (AbstractBaseSystem listener in Listeners)
                listener.OnSessionUpdate();
        }

        private void NotifyListenersOnEnd() {
            foreach (AbstractBaseSystem listener in Listeners)
                listener.OnSessionEnded();
        }
    }
}
