using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class AutoSaveTimer
{
    private static readonly double saveInterval = 300; 
    private static double nextSaveTime;

    static AutoSaveTimer()
    {
        nextSaveTime = EditorApplication.timeSinceStartup + saveInterval;
        EditorApplication.update += Update;
    }

    private static void Update()
    {
        if (Application.isPlaying) 
        {
            nextSaveTime = EditorApplication.timeSinceStartup + saveInterval;
            return;
        }

        if (EditorApplication.timeSinceStartup >= nextSaveTime)
        {
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            nextSaveTime = EditorApplication.timeSinceStartup + saveInterval;
        }
    }
}