using System;
using SDK.Code.Core.Strategy;
using SDK.Code.Interfaces;
using SDK.Code.Models;
using UnityEngine;

namespace SDK.Code.Core {

    /// <summary>
    ///     Represents the configuration settings for initializing and controlling the Voodoo SDK.
    ///     Provides options for logging, offer display behavior, session tracking, and server connection.
    ///     This configuration must be passed to <c>VoodooSDK.Init</c> during SDK initialization.
    /// </summary>
    [Serializable]
    public class VoodooSDKConfiguration {
        /// <summary>
        ///     Optional parent <see cref="GameObject" /> to which the SDK’s main transform will be attached.
        ///     Useful for organizing SDK components in the scene hierarchy.
        /// </summary>
        public GameObject Parent;

        internal string AppKey;
        internal bool autoShowOffers = true;
        internal bool canPrintLogs;
        internal IGameStateProvider gameStateProvider;
        internal bool isAutomaticSessionDisabled;
        internal Action<Offer> OnOfferReady;
        internal string ServerURL;
        internal int sessionTimeout;

        /// <summary>
        ///     Creates a new configuration instance with the specified app key and server URL.
        /// </summary>
        /// <param name="appKey">The unique application key used to authenticate with the server.</param>
        /// <param name="serverUrl">The base server URL used by the SDK for remote requests.</param>
        public VoodooSDKConfiguration(string appKey, string serverUrl) {
            AppKey = appKey;
            ServerURL = serverUrl;
        }

        internal IOfferSelectionStrategy OfferSelectionStrategy { get; private set; }

        #region Setters

        /// <summary>
        ///     Enables logging for the SDK, allowing internal debug and info logs to be printed.
        ///     Returns the same configuration instance for method chaining.
        /// </summary>
        public VoodooSDKConfiguration EnableLogging() {
            canPrintLogs = true;
            return this;
        }

        /// <summary>
        ///     Disables automatic showing of offers.
        ///     By default, offers are automatically shown when triggered.
        /// </summary>
        public VoodooSDKConfiguration DisableAutoShowOffers() {
            autoShowOffers = false;
            return this;
        }

        /// <summary>
        ///     Disables automatic session tracking by the SDK.
        ///     This allows manual session management via events.
        /// </summary>
        public VoodooSDKConfiguration DisableAutomaticSessions() {
            isAutomaticSessionDisabled = true;
            return this;
        }

        /// <summary>
        ///     Sets the session timeout in seconds.
        ///     Determines the delay before a new session is considered started after inactivity.
        /// </summary>
        /// <param name="timeout">Timeout value in seconds.</param>
        public VoodooSDKConfiguration SetSessionTimeout(int timeout) {
            sessionTimeout = timeout;
            return this;
        }

        /// <summary>
        ///     Sets an optional GameStateProvider that the SDK can query for player state.
        ///     If not set, SDK will only use manually provided state dictionaries.
        /// </summary>
        /// <param name="provider">Implementation of IGameStateProvider from game side.</param>
        public VoodooSDKConfiguration SetGameStateProvider(IGameStateProvider provider) {
            gameStateProvider = provider;
            return this;
        }

        /// <summary>
        ///     Registers a callback that will be invoked whenever the SDK finds an eligible offer.
        /// </summary>
        /// <param name="onOfferReady">
        ///     The action to execute when an eligible <see cref="Offer" /> is discovered by the SDK.
        ///     The offer object provided contains all metadata (id, type, trigger, price, rewards, conditions)
        ///     needed for the game to decide how to present or consume it.
        /// </param>
        /// <returns>
        ///     Returns the current <see cref="VoodooSDKConfiguration" /> instance to allow fluent chaining.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         This method does not show any UI. It is the responsibility of the game to subscribe via this callback
        ///         and decide how to present the offer (e.g., opening a custom popup or integrating with existing UI).
        ///     </para>
        ///     <para>
        ///         In a production environment, the server normally filters out ineligible or purchased offers.
        ///         In a mock/local setup, this callback ensures the game is always informed when the SDK
        ///         detects an offer that is ready to be shown.
        ///     </para>
        /// </remarks>
        public VoodooSDKConfiguration SetOfferReadyAction(Action<Offer> onOfferReady) {
            OnOfferReady += onOfferReady;
            return this;
        }

        public VoodooSDKConfiguration SetOfferSelectionStrategy(IOfferSelectionStrategy strategy) {
            OfferSelectionStrategy = strategy;
            return this;
        }

        #endregion

        #region Getters

        /// <summary>
        ///     Gets the application key used for server authentication.
        /// </summary>
        public string GetAppKey() {
            return AppKey;
        }

        /// <summary>
        ///     Gets the server URL used for remote requests.
        /// </summary>
        public string GetServerURL() {
            return ServerURL;
        }

        /// <summary>
        ///     Returns <c>true</c> if logging is enabled.
        /// </summary>
        public bool IsLoggingEnabled() {
            return canPrintLogs;
        }

        /// <summary>
        ///     Returns <c>true</c> if automatic showing of offers is enabled.
        ///     Defaults to <c>true</c>.
        /// </summary>
        public bool IsAutoShowOffersEnabled() {
            return autoShowOffers;
        }

        /// <summary>
        ///     Returns <c>true</c> if automatic session tracking is disabled.
        ///     Defaults to <c>false</c>.
        /// </summary>
        public bool IsAutomaticSessionTrackingDisabled() {
            return isAutomaticSessionDisabled;
        }

        /// <summary>
        ///     Gets the configured session timeout (in seconds) used for detecting new sessions.
        /// </summary>
        public int GetUpdateSessionTimerDelay() {
            return sessionTimeout;
        }

        /// <summary>
        ///     Gets the configured GameStateProvider, if any.
        ///     Returns null if no provider was set.
        /// </summary>
        public IGameStateProvider GetGameStateProvider() {
            return gameStateProvider;
        }

        /// <summary>
        ///     Gets the registered callback that is invoked when an eligible offer is found.
        /// </summary>
        /// <returns>
        ///     The <see cref="Action{Offer}" /> delegate provided via <c>SetOfferReadyAction</c>,
        ///     or <c>null</c> if none has been registered.
        /// </returns>
        public Action<Offer> GetOfferReadyAction() {
            return OnOfferReady;
        }

        public IOfferSelectionStrategy GetOfferSelectionStrategy() {
            return OfferSelectionStrategy;
        }

        #endregion
    }

}