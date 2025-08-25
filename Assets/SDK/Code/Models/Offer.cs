using System.Collections.Generic;
using SDK.Code.Interfaces;

namespace SDK.Code.Models {

    /// <summary>
    ///     Represents an in-game offer that can be displayed and purchased.
    /// </summary>
    public class Offer {
        public Offer(string id,
            OfferType type,
            string trigger,
            Dictionary<string, string> targetSegments,
            OfferPrice price,
            List<OfferReward> rewards,
            string nextOfferId,
            List<IOfferCondition> conditions) {
            Id = id;
            Type = type;
            Trigger = trigger;
            TargetSegments = targetSegments ?? new Dictionary<string, string>();
            Price = price;
            Rewards = rewards ?? new List<OfferReward>();
            NextOfferId = nextOfferId;
            Conditions = conditions ?? new List<IOfferCondition>();
        }

        public string Id { get; }
        public OfferType Type { get; }
        public string Trigger { get; }
        public Dictionary<string, string> TargetSegments { get; private set; }
        public OfferPrice Price { get; }
        public List<OfferReward> Rewards { get; }
        public string NextOfferId { get; }
        public List<IOfferCondition> Conditions { get; }

        /// <summary>
        ///     Validates whether this offer is eligible for the given game state.
        /// </summary>
        public bool Validate(IGameStateProvider state) {
            foreach (var condition in Conditions)
                if (!condition.Evaluate(state))
                    return false;
            return true;
        }

        public override string ToString() {
            var rewardsStr = Rewards is { Count: > 0 }
                ? string.Join(", ", Rewards.ConvertAll(r => $"{r.Amount} {r.ItemId}"))
                : "No Rewards";

            var conditionsStr = Conditions is { Count: > 0 }
                ? string.Join(", ", Conditions.ConvertAll(c => c.ToString()))
                : "No Conditions";

            return
                $"[Offer] Id={Id}, Type={Type}, Trigger={Trigger}, Price={Price.Amount} {Price.Currency}, Rewards=({rewardsStr}), NextOffer={NextOfferId}, Conditions=({conditionsStr})";
        }
    }

    public enum OfferType {
        Single,
        Chained,
        Endless,
        Multiple
    }

    public class OfferPrice {
        public OfferPrice(string currency, int amount) {
            Currency = currency;
            Amount = amount;
        }

        public string Currency { get; }
        public int Amount { get; }
    }

    public class OfferReward {
        public OfferReward(string itemId, int amount) {
            ItemId = itemId;
            Amount = amount;
        }

        public string ItemId { get; }
        public int Amount { get; }
    }

}