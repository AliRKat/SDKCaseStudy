using System.Collections.Generic;
using System.Linq;
using SDK.Code.Interfaces;
using SDK.Code.Models;

namespace SDK.Code.Utils {

    public static class OfferParser {
        public static Offer MapOffer(OfferDTO dto, Dictionary<string, string> userSegments) {
            if (!MatchesSegments(ToDict(dto.targetSegments), userSegments))
                return null;

            OfferVariantDTO selectedVariant = null;
            if (dto.variants != null && dto.variants.Count > 0)
                foreach (var v in dto.variants)
                    if (MatchesSegments(ToDict(v.segments), userSegments)) {
                        selectedVariant = v;
                        break;
                    }

            var price = selectedVariant != null
                ? new OfferPrice(selectedVariant.price.currency, selectedVariant.price.amount)
                : new OfferPrice(dto.price.currency, dto.price.amount);

            var rewards = selectedVariant?.rewards?.Select(r => new OfferReward(r.itemId, r.amount)).ToList()
                          ?? dto.rewards?.Select(r => new OfferReward(r.itemId, r.amount)).ToList()
                          ?? new List<OfferReward>();

            var conditions = selectedVariant?.conditions?.Select(OfferConditionFactory.Create).ToList()
                             ?? dto.conditions?.Select(OfferConditionFactory.Create).ToList()
                             ?? new List<IOfferCondition>();

            var offer = new Offer(
                dto.id,
                ParseOfferType(dto.type),
                dto.trigger,
                ToDict(dto.targetSegments),
                price,
                rewards,
                dto.nextOfferId,
                conditions
            );
            return offer;
        }

        private static bool MatchesSegments(Dictionary<string, string> required, Dictionary<string, string> user) {
            if (required == null || required.Count == 0) return true;

            if (user == null || user.Count == 0) return false;

            foreach (var kv in required) {
                if (!user.TryGetValue(kv.Key, out var val)) return false;

                if (val != kv.Value) return false;
            }

            return true;
        }

        private static OfferType ParseOfferType(string type) {
            return type switch {
                "Chained" => OfferType.Chained,
                "Endless" => OfferType.Endless,
                "Multiple" => OfferType.Multiple,
                _ => OfferType.Single
            };
        }

        private static Dictionary<string, string> ToDict(List<SegmentEntry> entries) {
            return entries?.ToDictionary(e => e.key, e => e.value)
                   ?? new Dictionary<string, string>();
        }
    }

}