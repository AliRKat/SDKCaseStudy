using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDK.Code.Core {

    public class VoodooSDKMainThreadDispatcher : MonoBehaviour {
        private static readonly Queue<Action> _executionQueue = new();

        private void Update() {
            lock (_executionQueue) {
                while (_executionQueue.Count > 0) _executionQueue.Dequeue()?.Invoke();
            }
        }

        internal static void Enqueue(Action action) {
            if (action == null) return;
            lock (_executionQueue) {
                _executionQueue.Enqueue(action);
            }
        }
    }

}