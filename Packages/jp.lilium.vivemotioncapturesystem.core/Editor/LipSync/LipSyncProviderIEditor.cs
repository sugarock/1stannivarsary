using UnityEngine;
using UnityEditor;
using VMCCore.Facial;

namespace VMCCore.Editor
{

    /// <summary>
    /// VTAvatarInputController インスペクター
    /// </summary>
    [CustomEditor (typeof (LipSyncProvider))]
    class LipSyncProviderIEditor : UnityEditor.Editor
    {
        SerializedProperty deviceNameProperty;

        public void OnEnable ()
        {
            deviceNameProperty = serializedObject.FindProperty ("_deviceName");
        }

        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI ();
            var t = (LipSyncProvider)this.target;

            if (t.useMicrophone) {
                int index = EditorGUILayout.Popup ("Device", MicrophoneUtility.GetIndex(deviceNameProperty.stringValue), Microphone.devices);
                if (index >= 0 && Microphone.devices.Length > 0) {
                     deviceNameProperty.stringValue = Microphone.devices[index];
                }
            }
            serializedObject.ApplyModifiedProperties ();

            GUI.enabled = false;
            EditorGUILayout.Slider ("A", t.visemes[0], 0, 1);
            EditorGUILayout.Slider ("I", t.visemes[1], 0, 1);
            EditorGUILayout.Slider ("U", t.visemes[2], 0, 1);
            EditorGUILayout.Slider ("E", t.visemes[3], 0, 1);
            EditorGUILayout.Slider ("O", t.visemes[4], 0, 1);
            GUI.enabled = true;
        }
    }

}
