using System.Collections.Generic;

namespace SDK.Code.Models {

    /// <summary>
    ///     Represents an in-game offer that can be displayed and purchased.
    /// </summary>
    public class Offer {
        /// <summary>
        ///     Unique identifier of the offer.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     The type of offer (Single, Chained, Endless).
        /// </summary>
        public OfferType Type { get; set; }

        /// <summary>
        ///     The price definition for this offer (currency type + amount).
        /// </summary>
        public OfferPrice Price { get; set; }

        /// <summary>
        ///     List of rewards granted when this offer is purchased.
        /// </summary>
        public List<OfferReward> Rewards { get; set; } = new();

        /// <summary>
        ///     (Optional) The next offer ID in the chain (if Type == Chained).
        /// </summary>
        public string NextOfferId { get; set; }

        /// <summary>
        ///     Conditions required for this offer to be valid (e.g., Level > 10).
        /// </summary>
        public List<OfferCondition> Conditions { get; set; } = new();
    }

    public enum OfferType {
        Single,
        Multiple,
        Chained,
        Endless
    }

    public class OfferPrice {
    }

    public class OfferReward {
    }

    public class OfferCondition {
    }

}