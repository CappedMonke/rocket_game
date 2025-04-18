using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPersistentObject
{
    void Initialize();
}


public static class PersistentObjects
{
    public static void Initialize(GameObject prefab)
    {
        var root = new GameObject();
        root.SetActive(false);
        var instance = Object.Instantiate(prefab, root.transform);

        foreach (var component in instance.GetComponentsInChildren<IPersistentObject>(includeInactive: true))
        {
            component.Initialize();
        }

        Object.DontDestroyOnLoad(root);
        instance.transform.DetachChildren();
        Object.Destroy(root);
    }
}


static class PersistentObjectInitializer
{
    private static GameObject GetPrefab()
    {
        return Resources.Load<GameObject>("PersistentObjects");
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnSubsystemRegistration()
    {
        PersistentObjects.Initialize(GetPrefab());
    }
}