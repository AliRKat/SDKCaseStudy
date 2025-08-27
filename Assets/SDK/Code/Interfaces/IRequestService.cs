using System;
using System.Collections.Generic;
using SDK.Code.Models;

namespace SDK.Code.Interfaces {

    public interface IRequestService {
        public void GetOffers(string resourceKey, Dictionary<string, string> userSegments,
            Action<List<Offer>> onResponse);
    }

}