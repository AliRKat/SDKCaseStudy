using System.Collections.Generic;
using System.Linq;
using SDK.Code.Interfaces;
using SDK.Code.Models;

namespace SDK.Code.Utils {

    public static class OfferParser {
        /// <summary>
        ///     Maps an <see cref="OfferDTO" /> (data transfer object) into a runtime <see cref="Offer" />.
        ///     Selects the most appropriate variant based on user segments, falling back to the base offer
        ///     if no matching variant is found.
        ///     Populates price, rewards, and conditions accordingly.
        /// </summary>
        /// <param name="dto">The source DTO containing offer configuration data.</param>
        /// <param name="userSegments">
        ///     The current user segment data used to determine eligibility
        ///     and select the correct variant.
        /// </param>
        /// <returns>
        ///     A mapped <see cref="Offer" /> if the user segments are compatible;
        ///     otherwise <c>null</c> if no segment match is found.
        /// </returns>
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

            var conditions = (selectedVariant?.conditions != null && selectedVariant.conditions.Count > 0
                                 ? selectedVariant.conditions.Select(OfferConditionFactory.Create).ToList()
                                 : dto.conditions?.Select(OfferConditionFactory.Create).ToList())
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

        /// <summary>
        ///     Validates whether the user’s segment data satisfies the required segment constraints.
        /// </summary>
        /// <param name="required">The required segment key/value pairs defined by the offer.</param>
        /// <param name="user">The user’s segment key/value pairs.</param>
        /// <returns>
        ///     <c>true</c> if the user’s segments match all required segments; otherwise <c>false</c>.
        /// </returns>
        private static bool MatchesSegments(Dictionary<string, string> required, Dictionary<string, string> user) {
            if (required == null || required.Count == 0) return true;

            if (user == null || user.Count == 0) return false;

            foreach (var kv in required) {
                if (!user.TryGetValue(kv.Key, out var val)) return false;

                if (val != kv.Value) return false;
            }

            return true;
        }

        /// <summary>
        ///     Parses the string representation of an offer type into the corresponding <see cref="OfferType" /> enum.
        ///     Defaults to <see cref="OfferType.Single" /> if no explicit match is found.
        /// </summary>
        /// <param name="type">The string value representing the offer type (e.g., "Chained").</param>
        /// <returns>The parsed <see cref="OfferType" /> enum value.</returns>
        private static OfferType ParseOfferType(string type) {
            return type switch {
                "Chained" => OfferType.Chained,
                "Endless" => OfferType.Endless,
                "Multiple" => OfferType.Multiple,
                _ => OfferType.Single
            };
        }

        /// <summary>
        ///     Converts a list of <see cref="SegmentEntry" /> objects into a dictionary.
        ///     If the list is null, returns an empty dictionary.
        /// </summary>
        /// <param name="entries">The segment entries to convert.</param>
        /// <returns>
        ///     A dictionary mapping segment keys to their corresponding values.
        ///     Returns an empty dictionary if <paramref name="entries" /> is null.
        /// </returns>
        private static Dictionary<string, string> ToDict(List<SegmentEntry> entries) {
            return entries?.ToDictionary(e => e.key, e => e.value)
                   ?? new Dictionary<string, string>();
        }
    }

    public static class MultipleOfferParser {
        public static MultipleOffer Map(MultipleOfferDTO dto, Dictionary<string, string> userSegments) {
            if (dto == null) return null;

            var mappedOffers = dto.offers?
                                   .Select(o => OfferParser.MapOffer(o, userSegments))
                                   .Where(o => o != null)
                                   .ToList()
                               ?? new List<Offer>();

            var conditions = dto.conditions?
                                 .Select(OfferConditionFactory.Create)
                                 .Where(c => c != null)
                                 .ToList()
                             ?? new List<IOfferCondition>();

            return new MultipleOffer(dto.id, dto.trigger, mappedOffers, conditions);
        }
    }

}