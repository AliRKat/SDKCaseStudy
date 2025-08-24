using System;
using System.Collections.Generic;
using Core;
using UnityEngine;

public class CurrencyManager : IBaseEventReceiver
{
    private readonly Dictionary<CurrencyType, int> _balances = new();

    public event Action<CurrencyType, int, int> CurrencyChanged;

    public CurrencyManager() { }

    public void Init()
    {
        Debug.Log("[CurrencyManager] Initialized");
    }

    public void OnEvent(IEvent @event) { }

    public int Get(CurrencyType type)
    {
        return _balances.TryGetValue(type, out var value) ? value : 0;
    }

    public void Add(CurrencyType type, int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));
        ChangeBalance(type, Get(type) + amount);
    }

    public bool HasEnough(CurrencyType type, int amount)
    {
        return Get(type) >= amount;
    }

    public bool Spend(CurrencyType type, int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));
        if (!HasEnough(type, amount))
            return false;
        ChangeBalance(type, Get(type) - amount);
        return true;
    }

    private void ChangeBalance(CurrencyType type, int newValue)
    {
        int prev = Get(type);
        _balances[type] = Mathf.Max(0, newValue);
        CurrencyChanged?.Invoke(type, prev, _balances[type]);
    }
}
