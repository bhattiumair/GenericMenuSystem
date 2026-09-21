using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MenuManager))]
public class MenuManagerEditor : Editor
{
    private string _newMenuType = "";
    private string _newMenuName = "";
    private const string EnumFilePath = "Assets/MenuSystem/Scripts/ScreenType.cs"; // Path to your ScreenType.cs file
    private const string tempScreen = "Assets/MenuSystem/Scripts/Menus/TempScreen.cs";
    private const string prefabPath = "Assets/MenuSystem/Prefabs/Menus/";

    public override void OnInspectorGUI()
    {
        // Draw the default MenuManager inspector fields
        DrawDefaultInspector();

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("Add New Menu Type to Enum", EditorStyles.boldLabel);

        // Input field for new enum name
        _newMenuType = EditorGUILayout.TextField("New Menu Type", _newMenuType);
        _newMenuName = EditorGUILayout.TextField("New Menu Name", _newMenuName);

        if (GUILayout.Button("Add Menu to Enum", GUILayout.Height(30)))
        {
            if (string.IsNullOrWhiteSpace(_newMenuType))
            {
                EditorUtility.DisplayDialog("Error", "Menu Name cannot be empty.", "OK");
                return;
            }

            // Sanitize string (remove spaces & special chars to form valid C# identifier)
            string sanitizedName = Regex.Replace(_newMenuType, @"\s+", "");
            sanitizedName = Regex.Replace(sanitizedName, @"[^a-zA-Z0-9_]", "");

            if (string.IsNullOrEmpty(sanitizedName) || char.IsDigit(sanitizedName[0]))
            {
                EditorUtility.DisplayDialog("Error", "Invalid C# Enum identifier name.", "OK");
                return;
            }
            if (isScreenTypeOrMenuNameAlreadyExists(sanitizedName, _newMenuName))
            {
                // show alert already shown in the method, just return
                // EditorUtility.DisplayDialog("Duplicate", "Menu Type or Name already exists.", "OK");
                return;
            }

            AddValueToEnum(sanitizedName);
            AddScreenScript(_newMenuName);
            CreatePrefabForScreen(_newMenuName);
            _newMenuType = "";
            _newMenuName = "";
        }

        if (GUILayout.Button("Remove Menu to Enum", GUILayout.Height(30)))
        {
            EditorUtility.DisplayDialog("Not Implemented", "Remove functionality is not implemented yet.", "OK");
        }

    }

    public bool isScreenTypeOrMenuNameAlreadyExists(string screenType, string menuName)
    {
        // Check if the enum value already exists
        if (Enum.IsDefined(typeof(ScreenType), screenType))
        {
            EditorUtility.DisplayDialog("Duplicate Enum", $"ScreenType.{screenType} already exists!", "OK");
            return true;
        }

        // Check if the script already exists
        string newScriptPath = $"Assets/MenuSystem/Scripts/Menus/{menuName}.cs";
        if (File.Exists(newScriptPath))
        {
            EditorUtility.DisplayDialog("Duplicate Script", $"Script {menuName}.cs already exists.", "OK");
            return true;
        }

        return false;
    }

    private void AddValueToEnum(string enumName)
    {
        if (!File.Exists(EnumFilePath))
        {
            Debug.LogError($"[MenuManagerEditor] Could not find enum file at path: {EnumFilePath}");
            return;
        }

        // Check if value already exists in the enum
        if (Enum.IsDefined(typeof(ScreenType), enumName))
        {
            EditorUtility.DisplayDialog("Duplicate Enum", $"ScreenType.{enumName} already exists!", "OK");
            return;
        }

        string fileText = File.ReadAllText(EnumFilePath);

        // Find closing brace of enum definition
        int lastBraceIndex = fileText.LastIndexOf('}');
        if (lastBraceIndex == -1)
        {
            Debug.LogError("[MenuManagerEditor] Invalid enum file formatting.");
            return;
        }

        // Calculate next auto-increment integer ID
        int nextId = Enum.GetValues(typeof(ScreenType)).Cast<int>().Max() + 1;

        // Insert new enum definition line right before the closing brace '}'
        string enumEntry = $"    {enumName} = {nextId},\n";
        string updatedFileText = fileText.Insert(lastBraceIndex, enumEntry);

        // Write to disk and trigger Unity script compilation
        File.WriteAllText(EnumFilePath, updatedFileText);
        AssetDatabase.Refresh();

        Debug.Log($"<color=green>Successfully added ScreenType.{enumName} to {EnumFilePath}. Recompiling...</color>");
    }

    private void AddScreenScript(string screenName)
    {
        if (string.IsNullOrWhiteSpace(screenName))
        {
            Debug.LogError("[MenuManagerEditor] Screen name cannot be empty.");
            return;
        }

        // Sanitize string (remove spaces & special chars to form valid C# identifier)
        string sanitizedName = Regex.Replace(screenName, @"\s+", "");
        sanitizedName = Regex.Replace(sanitizedName, @"[^a-zA-Z0-9_]", "");

        if (string.IsNullOrEmpty(sanitizedName) || char.IsDigit(sanitizedName[0]))
        {
            Debug.LogError("[MenuManagerEditor] Invalid C# class name.");
            return;
        }

        // Check if the script already exists
        string newScriptPath = $"Assets/MenuSystem/Scripts/Menus/{sanitizedName}.cs";
        if (File.Exists(newScriptPath))
        {
            Debug.LogError($"[MenuManagerEditor] Script {sanitizedName}.cs already exists.");
            return;
        }

        // Read the template script
        if (!File.Exists(tempScreen))
        {
            Debug.LogError($"[MenuManagerEditor] Could not find template script at path: {tempScreen}");
            return;
        }

        string templateText = File.ReadAllText(tempScreen);
        string newScriptText = templateText.Replace("TempScreen", sanitizedName);

        // Write the new script to disk
        File.WriteAllText(newScriptPath, newScriptText);
        AssetDatabase.Refresh();

        Debug.Log($"<color=green>Successfully created {sanitizedName}.cs from template.</color>");
    }

    public void CreatePrefabForScreen(string screenName)
    {
        string sanitizedName = Regex.Replace(screenName, @"\s+", "");
        sanitizedName = Regex.Replace(sanitizedName, @"[^a-zA-Z0-9_]", "");

        string scriptPath = $"Assets/MenuSystem/Scripts/Menus/{sanitizedName}.cs";
        if (!File.Exists(scriptPath))
        {
            Debug.LogError($"[MenuManagerEditor] Script {sanitizedName}.cs does not exist. Cannot create prefab.");
            return;
        }

        // Create a new GameObject and add the screen script component
        GameObject newScreenGO = new GameObject(sanitizedName);
        Type screenType = Type.GetType(sanitizedName);
        if (screenType == null)
        {
            Debug.LogError($"[MenuManagerEditor] Could not find type {sanitizedName}. Ensure the script compiles correctly.");
            DestroyImmediate(newScreenGO);
            return;
        }
        newScreenGO.AddComponent(screenType);

        // Create prefab
        string prefabFilePath = $"{prefabPath}{sanitizedName}.prefab";
        PrefabUtility.SaveAsPrefabAsset(newScreenGO, prefabFilePath);
        DestroyImmediate(newScreenGO);

        Debug.Log($"<color=green>Successfully created prefab {sanitizedName}.prefab at {prefabFilePath}.</color>");
    }
}