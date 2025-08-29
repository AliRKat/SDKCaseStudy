using System;
using System.Collections.Generic;

namespace SDK.Code.Models {

    [Serializable]
    public class MultipleOfferDTO {
        public string id;
        public string trigger;
        public List<OfferDTO> offers;
        public List<OfferConditionDTO> conditions;
    }

    [Serializable]
    public class MultipleOfferListDTO {
        public List<MultipleOfferDTO> multipleOffers;
    }

    [Serializable]
    public class BoughtOffersDTO {
        public List<string> offerIds = new();
    }

    [Serializable]
    public class OfferListDTO {
        public List<OfferDTO> offers;
    }

    [Serializable]
    public class SegmentEntry {
        public string key;
        public string value;
    }

    [Serializable]
    public class OfferVariantDTO {
        public OfferPriceDTO price;
        public List<OfferRewardDTO> rewards;
        public List<OfferConditionDTO> conditions;
        public List<SegmentEntry> segments;
    }

    [Serializable]
    public class OfferDTO {
        public string id;
        public string type;
        public string trigger;
        public string nextOfferId;

        public OfferPriceDTO price;
        public List<OfferRewardDTO> rewards;
        public List<OfferConditionDTO> conditions;

        public List<OfferVariantDTO> variants;
        public List<SegmentEntry> targetSegments;
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
        public string type;
        public string value;
    }

}