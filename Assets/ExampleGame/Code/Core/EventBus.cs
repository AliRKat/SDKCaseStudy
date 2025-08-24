using System;
using System.Collections.Generic;
using Core;

namespace Code.Core {
    public class EventBus {
        private Dictionary<Type, SortedDictionary<int, List<WeakReference<IBaseEventReceiver>>>> _receivers;
        private Dictionary<int, WeakReference<IBaseEventReceiver>> _receiverHashToReference;

        public EventBus() {
            _receivers = new Dictionary<Type, SortedDictionary<int, List<WeakReference<IBaseEventReceiver>>>>();
            _receiverHashToReference = new Dictionary<int, WeakReference<IBaseEventReceiver>>();
        }

        public void Register<T>(IBaseEventReceiver receiver, int priority = 0) where T : struct, IEvent {
            var eventType = typeof(T);
            if (!_receivers.ContainsKey(eventType)) {
                _receivers[eventType] = new SortedDictionary<int, List<WeakReference<IBaseEventReceiver>>>();
            }

            if (!_receivers[eventType].ContainsKey(priority)) {
                _receivers[eventType][priority] = new List<WeakReference<IBaseEventReceiver>>();
            }

            foreach (var priorityGroup in _receivers[eventType]) {
                foreach (var existingReference in priorityGroup.Value) {
                    if (existingReference.TryGetTarget(out var existingReceiver) && existingReceiver == receiver) {
                        return;
                    }
                }
            }

            var reference = new WeakReference<IBaseEventReceiver>(receiver);
            _receivers[eventType][priority].Add(reference);
            _receiverHashToReference[receiver.GetHashCode()] = reference;
        }

        public void Unregister<T>(IBaseEventReceiver receiver) where T : struct, IEvent {
            var eventType = typeof(T);
    
            if (!_receivers.ContainsKey(eventType)) {
                return;
            }

            var priorityGroupsToRemove = new List<int>();

            foreach (var priorityGroup in _receivers[eventType]) {
                for (int i = priorityGroup.Value.Count - 1; i >= 0; i--) {
                    if (priorityGroup.Value[i].TryGetTarget(out var existingReceiver) && existingReceiver == receiver) {
                        priorityGroup.Value.RemoveAt(i);
                    }
                }

                if (priorityGroup.Value.Count == 0) {
                    priorityGroupsToRemove.Add(priorityGroup.Key);
                }
            }

            foreach (var priority in priorityGroupsToRemove) {
                _receivers[eventType].Remove(priority);
            }

            if (_receivers[eventType].Count == 0) {
                _receivers.Remove(eventType);
            }
        }

        public void Raise<T>(T @event) where T : struct, IEvent {
            var eventType = typeof(T);
            if (!_receivers.ContainsKey(eventType)) return;

            foreach (var priorityGroup in _receivers[eventType]) {
                foreach (var reference in priorityGroup.Value) {
                    if (reference.TryGetTarget(out var receiver)) {
                        receiver.OnEvent(@event);
                    }
                }
            }
        }
    }
}