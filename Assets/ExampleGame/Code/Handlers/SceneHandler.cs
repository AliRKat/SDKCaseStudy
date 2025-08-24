using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Core
{
    public class SceneHandler
    {
        private static readonly Dictionary<GameScene, string> SceneNames = new()
        {
            { GameScene.Load, "Load" },
            { GameScene.Home, "Home" },
        };

        public static IEnumerator LoadSceneAsync(GameScene scene, System.Action onLoaded = null)
        {
            string sceneName = SceneNames[scene];
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            op.allowSceneActivation = false;

            while (!op.isDone)
            {
                if (op.progress >= 0.9f)
                    op.allowSceneActivation = true;
                yield return null;
            }

            onLoaded?.Invoke();
        }
    }
}
