using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


public static class SceneUtils
{
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
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
    public static void LoadScene(string sceneName, Action onSceneLoaded)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

        // note null initialization is required to force nonlocal scope of the handler, see https://stackoverflow.com/a/1362244
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
