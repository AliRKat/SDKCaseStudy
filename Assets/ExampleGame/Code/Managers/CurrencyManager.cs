using System;
using System.Collections.Generic;
using Code.Core;
using Core;
using ExampleGame.Code.Enums;
using ExampleGame.Code.Events;
using UnityEngine;

namespace ExampleGame.Code.Managers {

    public class CurrencyManager : IBaseEventReceiver {
        private readonly Dictionary<CurrencyType, int> _balances = new();

        public void OnEvent(IEvent @event) {
        }

        public event Action<CurrencyType, int, int> CurrencyChanged;

        public void Init() {
        }

        public int Get(CurrencyType type) {
            return _balances.TryGetValue(type, out var value) ? value : 0;
        }

        public void Add(CurrencyType type, int amount) {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            ChangeBalance(type, Get(type) + amount);
        }

        public bool HasEnough(CurrencyType type, int amount) {
            return Get(type) >= amount;
        }

        public bool Spend(CurrencyType type, int amount) {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            if (!HasEnough(type, amount))
                return false;
            ChangeBalance(type, Get(type) - amount);
            return true;
        }

        private void ChangeBalance(CurrencyType type, int newValue) {
            var prev = Get(type);
            _balances[type] = Mathf.Max(0, newValue);
            CurrencyChanged?.Invoke(type, prev, _balances[type]);
            GameManager.Instance.EventBus.Raise(new OnCurrencyChanged(type, prev, _balances[type]));
        }
    }

}