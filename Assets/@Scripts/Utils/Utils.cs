using UnityEngine;
using System;
using Unity.VisualScripting;

public class Utils
{
    public static T GetorAddComponent<T>(GameObject go) where T : Component
    {
        var component = go.GetComponent<T>();
        if (component == null)
        {
            component = go.AddComponent<T>();
        }

        return component;
    }   

    public static T FindChild<T>(GameObject parent, string name, bool includeInactive) where T : Component
    {
        if(parent == null) return null;

        foreach(Transform child in parent.transform)
        {
            if(child.name == name)
            {
                T component = child.GetComponent<T>();
                if(component != null)
                    return component;
            }

            if(includeInactive || child.gameObject.activeSelf)
            {
                T component1 = FindChild<T>(child.gameObject, name, includeInactive);
                if(component1 != null)
                    return component1;
            }
        }
        return null;
    }
}