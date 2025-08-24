using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.UI
{
    public abstract class ActionButton<T> : ActionButton
        where T : Enum
    {
        private T _clickAction;
        private object _data;
        private bool _isInitialized;
        protected abstract void InvokeAction(T action, object data = null);

        public void RegisterClick(T action, object data = null)
        {
            _clickAction = action;
            _data = data;
            _isInitialized = true;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            if (_isInitialized == false || interactable == false)
                return;
            InvokeAction(_clickAction, _data);
        }
    }

    public class ActionButton : Button
    {
        public event Action OnClickToActionButton;

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable)
                return;
            OnClickToActionButton?.Invoke();
            base.OnPointerClick(eventData);
        }
    }
}
