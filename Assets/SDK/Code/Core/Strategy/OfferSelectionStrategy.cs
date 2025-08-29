using System;
using System.Collections.Generic;
using SDK.Code.Interfaces;
using SDK.Code.Models;

namespace SDK.Code.Core.Strategy {

    /// <summary>
    ///     Defines the contract for selecting a single offer from a set of eligible offers.
    /// </summary>
    public interface IOfferSelectionStrategy {
        /// <summary>
        ///     Selects one offer from the given list of eligible offers.
        /// </summary>
        /// <param name="eligibleOffers">List of eligible offers to choose from.</param>
        /// <param name="trigger">The trigger event that caused this selection.</param>
        /// <param name="state">Current game state provider, for additional evaluation if needed.</param>
        /// <returns>The selected offer, or null if none are eligible.</returns>
        Offer Select(List<Offer> eligibleOffers, string trigger, IGameStateProvider state);
    }

    /// <summary>
    ///     Selects offers in a round-robin rotation order.
    /// </summary>
    public class RotationOfferSelectionStrategy : IOfferSelectionStrategy {
        private int _index;

        public Offer Select(List<Offer> eligibleOffers, string trigger, IGameStateProvider state) {
            if (eligibleOffers == null || eligibleOffers.Count == 0)
                return null;

            var offer = eligibleOffers[_index % eligibleOffers.Count];
            _index++;
            return offer;
        }
    }

    /// <summary>
    ///     Selects an offer at random.
    /// </summary>
    public class RandomOfferSelectionStrategy : IOfferSelectionStrategy {
        private readonly Random _random = new();

        public Offer Select(List<Offer> eligibleOffers, string trigger, IGameStateProvider state) {
            if (eligibleOffers == null || eligibleOffers.Count == 0)
                return null;

            var idx = _random.Next(eligibleOffers.Count);
            return eligibleOffers[idx];
        }
    }

    /// <summary>
    ///     Selects the first offer that has not been shown before.
    ///     Uses a provided game state to check shown state.
    /// </summary>
    public class HasNotShownOfferSelectionStrategy : IOfferSelectionStrategy {
        public Offer Select(List<Offer> eligibleOffers, string trigger, IGameStateProvider state) {
            if (eligibleOffers == null || eligibleOffers.Count == 0)
                return null;

            foreach (var offer in eligibleOffers)
                if (!state.HasPurchased(offer.Id))
                    return offer;

            return null;
        }
    }

    /// <summary>
    ///     Endless rotation: always cycles through all offers endlessly as long as player consumes them.
    /// </summary>
    public class EndlessRotationOfferSelectionStrategy : IOfferSelectionStrategy {
        private Queue<Offer> _queue = new();

        public Offer Select(List<Offer> eligibleOffers, string trigger, IGameStateProvider state) {
            if (eligibleOffers == null || eligibleOffers.Count == 0)
                return null;

            // Initialize queue if empty or if eligible set changed
            if (_queue.Count == 0 || _queue.Count != eligibleOffers.Count) _queue = new Queue<Offer>(eligibleOffers);

            var next = _queue.Dequeue();
            _queue.Enqueue(next); // rotate endlessly
            return next;
        }
    }

}