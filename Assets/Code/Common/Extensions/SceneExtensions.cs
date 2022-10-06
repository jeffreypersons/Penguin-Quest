using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


namespace PQ.Common.Extensions
{
    public static class SceneExtensions
    {
        public static bool IsSceneAbleToLoad(string sceneName)
        {
            return Application.CanStreamedLevelBeLoaded(sceneName);
        }
        public static string GetNameOfActiveScene()
        {
            return SceneManager.GetActiveScene().name;
        }
        public static GameObject[] GetRootObjectsOfActiveScene()
        {
            return SceneManager.GetActiveScene().GetRootGameObjects();
        }

        public static void LoadScene(string sceneName)
        {
            if (!IsSceneAbleToLoad(sceneName))
            {
                Debug.LogError($"Scene cannot be loaded, perhaps `{sceneName}` is misspelled?");
                return;
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        public static void LoadScene(string sceneName, Action onSceneLoaded)
        {
            if (!IsSceneAbleToLoad(sceneName))
            {
                Debug.LogError($"Scene cannot be loaded, perhaps `{sceneName}` is misspelled?");
                return;
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

            // note null initialization is required to force nonlocal scope of the handler,
            // see https://stackoverflow.com/a/1362244
            UnityAction<Scene, LoadSceneMode> handler = null;
            handler = (sender, args) =>
            {
                SceneManager.sceneLoaded -= handler;
                onSceneLoaded.Invoke();
            };
            SceneManager.sceneLoaded += handler;
        }

        public static void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}
