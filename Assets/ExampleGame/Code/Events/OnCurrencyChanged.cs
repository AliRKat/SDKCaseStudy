using Core;
using ExampleGame.Code.Enums;

namespace ExampleGame.Code.Events {

    /// <summary>
    ///     Raised when a currency balance changes.
    ///     Contains info about which currency, previous amount, new amount, and delta.
    /// </summary>
    public struct OnCurrencyChanged : IEvent {
        public CurrencyType Type { get; }
        public int PreviousAmount { get; }
        public int NewAmount { get; }

        public OnCurrencyChanged(CurrencyType type, int previousAmount, int newAmount) {
            Type = type;
            PreviousAmount = previousAmount;
            NewAmount = newAmount;
        }
    }

}