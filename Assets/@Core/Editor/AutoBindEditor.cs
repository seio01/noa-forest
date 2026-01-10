using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;

[CustomEditor(typeof(UI_Base), true)]
public class AutoBindEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(5);

        if(GUILayout.Button("Bind"))
        {
            AutoBind((MonoBehaviour)target);
        }
    }

    private void AutoBind(MonoBehaviour targetBehaviour)
    {
        var serializedObject = new SerializedObject(targetBehaviour);
        var fields = targetBehaviour.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        // 이름 인덱스 생성
        var nameIndex = BuildNameIndex(targetBehaviour.transform);

        foreach (var field in fields)
        {
            if (!(field.IsPublic || field.GetCustomAttribute<SerializeField>() != null))
                continue;

            if (!(typeof(Component).IsAssignableFrom(field.FieldType) || field.FieldType == typeof(GameObject)))
                continue;

            string objectName = field.Name;

            // 이름 후보 목록 가져오기
            if (!nameIndex.TryGetValue(objectName, out var nameMatches) || nameMatches.Count == 0)
            {
                Debug.LogWarning($"[AutoBindEditor] {field.Name} not found in {targetBehaviour.name}");
                continue;
            }

            if (field.FieldType == typeof(GameObject))
            {
                if (nameMatches.Count > 1)
                {
                    LogDuplicateNameWarning(targetBehaviour.transform, objectName, nameMatches);
                    continue;
                }

                var property = serializedObject.FindProperty(field.Name);
                property.objectReferenceValue = nameMatches[0].gameObject;
            }
            else
            {
                var componentMatches = new List<Component>();
                for (int i = 0; i < nameMatches.Count; i++)
                {
                    var c = nameMatches[i].GetComponent(field.FieldType);
                    if (c != null) componentMatches.Add(c);
                }

                if (componentMatches.Count == 0)
                {
                    Debug.LogWarning($"[AutoBindEditor] {field.Name} (Component of type {field.FieldType.Name}) not found in {targetBehaviour.name}");
                    continue;
                }

                if (componentMatches.Count > 1)
                {
                    var transforms = componentMatches.Select(c => c.transform).ToList();
                    LogDuplicateNameWarning(targetBehaviour.transform, objectName, transforms, field.FieldType);
                    continue;
                }

                var property = serializedObject.FindProperty(field.Name);
                property.objectReferenceValue = componentMatches[0];
            }
        }

        serializedObject.ApplyModifiedProperties();
        Debug.Log($"[AutoBindEditor] Auto-bound components for {targetBehaviour.name}");
    }

    // 이름 인덱스 생성: 한 번만 전체 순회
    private Dictionary<string, List<Transform>> BuildNameIndex(Transform root)
    {
        var dict = new Dictionary<string, List<Transform>>(StringComparer.OrdinalIgnoreCase);
        CollectAllTransforms(root, dict);
        return dict;
    }

    private void CollectAllTransforms(Transform parent, Dictionary<string, List<Transform>> dict)
    {
        foreach (Transform child in parent)
        {
            if (!dict.TryGetValue(child.name, out var list))
            {
                list = new List<Transform>();
                dict[child.name] = list;
            }
            list.Add(child);

            CollectAllTransforms(child, dict);
        }
    }

    private void LogDuplicateNameWarning(Transform root, string name, List<Transform> matches, System.Type componentType = null)
    {
        string typeLabel = componentType == null ? "GameObject" : $"Component({componentType.Name})";
        string paths = string.Join(", ", matches.Select(t => GetRelativePath(root, t)));
        Debug.LogWarning($"[AutoBindEditor] Duplicate name '{name}' detected for {typeLabel} under '{root.name}'. Skipping bind for this field. Matches: {paths}");
    }

    private string GetRelativePath(Transform root, Transform target)
    {
        if (root == null || target == null)
        {
            return target != null ? target.name : "<null>";
        }

        var parts = new List<string>();
        var current = target;
        while (current != null)
        {
            parts.Add(current.name);
            if (current == root)
                break;
            current = current.parent;
        }
        parts.Reverse();
        return string.Join("/", parts);
    }

}
