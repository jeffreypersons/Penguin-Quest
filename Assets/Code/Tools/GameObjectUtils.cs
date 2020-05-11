using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


// note that these methods use old style for loops so that the found value CAN be modified after return
public static class GameObjectUtils
{


    // note: only fetches active gameObjects
    public static List<GameObject> FindAllObjectsWithTags(params string[] tags)
    {
        var objects = new List<GameObject>();
        foreach (string tag in tags)
        {
            objects.AddRange(GameObject.FindGameObjectsWithTag(tag));
        }
        return objects;
    }
    // note: only fetches active gameObjects
    public static List<T> FindAllObjectsWithTags<T>(params string[] tags) where T : Object
    {
        var objects = new List<T>();
        foreach (string tag in tags)
        {
            GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(tag);
            for (int i = 0; i < objectsWithTag.Length; i++)
            {
                T component;
                if (objectsWithTag[i].TryGetComponent(out component))
                {
                    objects.Add(component);
                }
            }
        }
        return objects;
    }

    // note: only fetches active game objects, null if not found
    public static GameObject FindFirstChildWithTag(GameObject gameObject, string tag)
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Transform child = gameObject.transform.GetChild(i);
            if (child.CompareTag(tag))
            {
                return child.gameObject;
            }
        }
        return default;
    }
    // note: only fetches active game objects, null if not found
    public static T FindFirstChildWithTag<T>(GameObject gameObject, string tag)
    {
        T component;
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Transform child = gameObject.transform.GetChild(i);
            if (child.CompareTag(tag) && child.TryGetComponent(out component))
            {
                return component;
            }
        }
        return default;
    }

    // note: only fetches active game object, null if not found
    public static GameObject FindChildWithName(GameObject gameObject, string name)
    {
        Transform child = gameObject.transform.Find(name);
        if (child != null && child.gameObject.activeSelf)
        {
            return child.gameObject;
        }
        return default;
    }
    // note: only fetches active game object, null if not found
    public static T FindChildWithName<T>(GameObject gameObject, string name)
    {
        T componentOfChild;
        Transform child = gameObject.transform.Find(name);
        if (child != null && child.gameObject.activeSelf && child.TryGetComponent(out componentOfChild))
        {
            return componentOfChild;
        }
        return default;
    }
}
