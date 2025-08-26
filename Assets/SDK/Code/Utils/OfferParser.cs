using System.Collections.Generic;
using System.Linq;
using SDK.Code.Interfaces;
using SDK.Code.Models;
using UnityEngine;

namespace SDK.Code.Utils {

    public static class OfferParser {
        /// <summary>
        ///     Loads offers from a JSON file in Resources folder and maps them to Offer objects.
        /// </summary>
        /// <param name="resourcePath">Path inside Resources folder (without .json extension)</param>
        public static List<Offer> LoadOffersFromJson(string resourcePath) {
            var jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset == null) {
                Debug.LogError($"[OfferParser] Failed to load JSON at {resourcePath}");
                return new List<Offer>();
            }

            var dtoWrapper = JsonUtility.FromJson<OfferListDTO>(jsonAsset.text);
            if (dtoWrapper == null || dtoWrapper.offers == null)
                return new List<Offer>();

            return dtoWrapper.offers.Select(MapOffer).ToList();
        }

        public static Offer MapOffer(OfferDTO dto) {
            var price = new OfferPrice(dto.price.currency, dto.price.amount);
            var rewards = dto.rewards?.Select(r => new OfferReward(r.itemId, r.amount)).ToList() ??
                          new List<OfferReward>();

            var conditions = new List<IOfferCondition>();
            if (dto.conditions != null)
                foreach (var c in dto.conditions)
                    conditions.Add(OfferConditionFactory.Create(c));

            Debug.Log($"DTO {dto.id} → Conditions count: {dto.conditions?.Count ?? 0}");

            return new Offer(
                dto.id,
                ParseOfferType(dto.type),
                dto.trigger,
                dto.targetSegments ?? new Dictionary<string, string>(),
                price,
                rewards,
                dto.nextOfferId,
                conditions
            );
        }

        private static OfferType ParseOfferType(string type) {
            return type switch {
                "Chained" => OfferType.Chained,
                "Endless" => OfferType.Endless,
                "Multiple" => OfferType.Multiple,
                _ => OfferType.Single
            };
        }
    }

}