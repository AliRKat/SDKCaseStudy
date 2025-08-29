using ExampleGame.Code.Enums;

namespace SDK.Code.Utils {

    public static class Converters {
        public static CurrencyType ToCurrencyType(string currency) {
            if (string.IsNullOrEmpty(currency))
                return CurrencyType.None;

            switch (currency.ToLowerInvariant()) {
                case "free": return CurrencyType.Free;
                case "coins": return CurrencyType.Coins;
                case "gems": return CurrencyType.Gems;
                case "usd": return CurrencyType.USD;
                case "eur": return CurrencyType.EUR;
                case "tokens": return CurrencyType.Tokens;
                default: return CurrencyType.None;
            }
        }
    }

}