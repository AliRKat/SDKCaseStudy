using System;
using System.Collections.Generic;

namespace SDK.Code.Models {

    [Serializable]
    public class OfferListDTO {
        public List<OfferDTO> offers;
    }

    [Serializable]
    public class OfferDTO {
        public string id;
        public string type; // "Single", "Chained", "Endless", "Multiple"
        public string trigger;

        public OfferPriceDTO price;
        public List<OfferRewardDTO> rewards;
        public string nextOfferId;
        public List<OfferConditionDTO> conditions;
        public Dictionary<string, string> targetSegments;
    }

    [Serializable]
    public class OfferPriceDTO {
        public string currency;
        public int amount;
    }

    [Serializable]
    public class OfferRewardDTO {
        public string itemId;
        public int amount;
    }

    [Serializable]
    public class OfferConditionDTO {
        public string type; // e.g. "LevelAtLeast"
        public string value; // e.g. "5", "Gems:50"
    }

}