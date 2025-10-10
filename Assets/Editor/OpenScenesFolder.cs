using UnityEditor;
using UnityEngine;
using System.IO;

// opens Assets/Scenes by default in the Unity Editor
// code provided by ChatGPT
public static class OpenScenesFolder
{
    [MenuItem("File/Open Scene (Scenes Folder) %#o")] // Ctrl/Cmd + Shift + O
    public static void OpenSceneFromScenesFolder()
    {
        string path = "Assets/Scenes";
        if (!Directory.Exists(path))
            path = "Assets"; // fallback if folder is missing

        string scenePath = EditorUtility.OpenFilePanel("Open Scene", path, "unity");
        if (!string.IsNullOrEmpty(scenePath))
        {
            // Convert absolute path back to relative for Unity
            string relPath = "Assets" + scenePath.Substring(Application.dataPath.Length);
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(relPath);
        }
    }
}