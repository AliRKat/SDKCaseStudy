using System;

namespace SDK.Code.Interfaces {

    public interface IRequestService {
        void GetOffers(string resourceKey, Action<string> onResponse);
    }

}