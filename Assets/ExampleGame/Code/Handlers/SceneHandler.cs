using System;
using System.Collections;
using System.Collections.Generic;
using ExampleGame.Code.Enums;
using UnityEngine.SceneManagement;

namespace Code.Core {

    public class SceneHandler {
        private static readonly Dictionary<GameScene, string> SceneNames = new() {
            { GameScene.Load, "Load" },
            { GameScene.Home, "Home" }
        };

        public static IEnumerator LoadSceneAsync(GameScene scene, Action onLoaded = null) {
            var sceneName = SceneNames[scene];
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            op.allowSceneActivation = false;

            while (!op.isDone) {
                if (op.progress >= 0.9f)
                    op.allowSceneActivation = true;
                yield return null;
            }

            onLoaded?.Invoke();
        }
    }

}