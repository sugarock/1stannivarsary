using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class ChronoAnimation : EditorWindow
{
    private void Awake()
    {
        return;
    }
    private void Update()
    {
        return;
    }

    private float flamelate = 1;
    private Animation recordingAnim;
    bool groupEnabled;
    bool myBool = true;
    float myFloat = 1.23f;
    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Animation/Chrono Animation")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        ChronoAnimation window = (ChronoAnimation)EditorWindow.GetWindow(typeof(ChronoAnimation));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Record Settings", EditorStyles.boldLabel);
        flamelate = EditorGUILayout.FloatField("Recording flame", flamelate);
        if (GUILayout.Button("click"))
        {
            Debug.Log("click");
        }
        groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        myBool = EditorGUILayout.Toggle("Toggle", myBool);
        myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
        EditorGUILayout.EndToggleGroup();
    }
}
