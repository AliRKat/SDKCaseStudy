using System;

namespace SDK.Code.Interfaces {

    public interface IGameStateProvider {
        int GetPlayerLevel();
        int GetCurrency(string currency);
        int GetCompletedStages();
        bool HasPurchased(string offerId);
        DateTime GetLastShown(string offerId);
    }

}