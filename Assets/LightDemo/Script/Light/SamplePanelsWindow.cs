using System.IO;
using UnityEditor;
using UnityEngine;

public class SamplePanelsWindow : Editor
{
    [MenuItem("Edit/CaptureScreenshot #�2")]
    private static void CaptureScreenshot()
    {
        string path = EditorUtility.SaveFilePanel("Save Screenshot", Application.dataPath, System.DateTime.Now.ToString("yyyyMMdd-HHmmss"), "png");
        ScreenCapture.CaptureScreenshot(path);
        var assembly = typeof(UnityEditor.EditorWindow).Assembly;
        var type = assembly.GetType("UnityEditor.GameView");
        var gameview = EditorWindow.GetWindow(type);
        gameview.Repaint();
        Debug.Log("ScreenShot: " + path);
    }
}
