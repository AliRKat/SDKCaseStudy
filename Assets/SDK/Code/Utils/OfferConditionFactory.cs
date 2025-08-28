using System;
using SDK.Code.Interfaces;
using SDK.Code.Models;
using UnityEngine;

namespace SDK.Code.Utils {

    public static class OfferConditionFactory {
        public static IOfferCondition Create(OfferConditionDTO dto) {
            switch (dto.type) {
                case "LevelAtLeast":
                    return new LevelAtLeastCondition(dto.value);
                case "StageCompleted":
                    return new StageCompletedCondition(dto.value);
                case "HasCurrency":
                    var parts = dto.value.Split(':');
                    return new HasCurrencyCondition(parts[0], int.Parse(parts[1]));
                case "CooldownSeconds":
                    return new CooldownCondition("unknown_offer", int.Parse(dto.value));
                case "HasNotPurchased":
                    return new HasNotPurchasedCondition(dto.value);
                default:
                    return new UnknownCondition(dto.type);
            }
        }
    }

    public class LevelAtLeastCondition : IOfferCondition {
        private readonly int _requiredLevel;

        public LevelAtLeastCondition(string value) {
            if (!int.TryParse(value, out _requiredLevel))
                _requiredLevel = int.MaxValue;
        }

        public bool Evaluate(IGameStateProvider state) {
            var current = state.GetPlayerLevel();

            return current >= _requiredLevel;
        }
    }
    
    public class StageCompletedCondition : IOfferCondition {
        private readonly int _requiredStage;

        public StageCompletedCondition(string value) {
            if (!int.TryParse(value, out _requiredStage))
                _requiredStage = int.MaxValue;
        }

        public bool Evaluate(IGameStateProvider state) {
            var current = state.GetCompletedStages();

            return current >= _requiredStage;
        }
    }

    public class HasCurrencyCondition : IOfferCondition {
        private readonly int _amount;
        private readonly string _currency;

        public HasCurrencyCondition(string currency, int amount) {
            _currency = currency;
            _amount = amount;
        }

        public bool Evaluate(IGameStateProvider state) {
            return state.GetCurrency(_currency) >= _amount;
        }
    }

    public class CooldownCondition : IOfferCondition {
        private readonly int _cooldownSeconds;
        private readonly string _offerId;

        public CooldownCondition(string offerId, int cooldownSeconds) {
            _offerId = offerId;
            _cooldownSeconds = cooldownSeconds;
        }

        public bool Evaluate(IGameStateProvider state) {
            var lastShown = state.GetLastShown(_offerId);
            return (DateTime.UtcNow - lastShown).TotalSeconds >= _cooldownSeconds;
        }
    }

    public class HasNotPurchasedCondition : IOfferCondition {
        private readonly string _offerId;

        public HasNotPurchasedCondition(string offerId) {
            _offerId = offerId;
        }

        public bool Evaluate(IGameStateProvider state) {
            return !state.HasPurchased(_offerId);
        }
    }

    public class UnknownCondition : IOfferCondition {
        private readonly string _type;

        public UnknownCondition(string type) {
            _type = type;
        }

        public bool Evaluate(IGameStateProvider state) {
            Debug.LogWarning($"[OfferCondition] Unknown condition type: {_type}");
            return false;
        }
    }

}