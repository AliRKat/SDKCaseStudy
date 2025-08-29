using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ExampleGame.Code.UI {

    public class ShowAnimation : MonoBehaviour {
        public Transform showRoot;
        public float showStartSize = 0.8f;
        public float showDuration = 0.25f;
        public float animationInterval = 0.05f;
        public float fadeInDuration = 0.1f;
        public float darkBgTargetAlpha = 0.9f;
        public Image darkBg;
        public Button skipButton;

        public List<CanvasGroup> canvasGroups = new();
        public Coroutine animationCoroutine;
        public Action OnShowComplete;

        private void OnEnable() {
            Play();
        }

        public void Play() {
            if (animationCoroutine != null) StopCoroutine(animationCoroutine);

            darkBg.DOFade(darkBgTargetAlpha, showDuration);
            animationCoroutine = StartCoroutine(AnimateChildGroups());
            showRoot.localScale = Vector3.one * showStartSize;
            showRoot
                .DOScale(Vector3.one, showDuration)
                .OnComplete(() => { })
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        public IEnumerator AnimateChildGroups() {
            SetupAnimation();
            foreach (var canvasGroup in canvasGroups) {
                var obj = canvasGroup.transform;
                obj.localScale = Vector3.zero;

                obj
                    .DOScale(Vector3.one, fadeInDuration)
                    .SetEase(Ease.OutBack);

                canvasGroup
                    .DOFade(1, fadeInDuration)
                    .SetEase(Ease.OutBack);

                yield return new WaitForSeconds(animationInterval);
            }

            CompleteAnimation();
        }

        private void CompleteAnimation() {
            OnShowComplete?.Invoke();
            OnShowComplete = null;

            skipButton?.gameObject.SetActive(false);
        }

        public void Skip() {
            if (animationCoroutine != null) StopCoroutine(animationCoroutine);

            foreach (var canvasGroup in canvasGroups) {
                canvasGroup.alpha = 1;
                canvasGroup.transform.localScale = Vector3.one;
            }

            CompleteAnimation();
        }

        private void SetupAnimation() {
            foreach (var canvasGroup in canvasGroups) canvasGroup.alpha = 0;
        }
    }

}