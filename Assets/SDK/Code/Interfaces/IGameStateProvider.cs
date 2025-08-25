using System;

namespace SDK.Code.Interfaces {

    public interface IGameStateProvider {
        int GetPlayerLevel();
        int GetCurrency(string currency);
        bool HasPurchased(string offerId);
        DateTime GetLastShown(string offerId);
    }

}