using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using VMCCore.Facial;

namespace VMCCore.Editor
{
    public class FacialAvatarEditor : UnityEditor.Editor
    {
        ReorderableList _ro;

        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI ();

            var t = (FacialAvatar)target;
            if (_ro == null) {
                _ro = new ReorderableList (serializedObject, serializedObject.FindProperty ("faceDescriptions"));
                _ro.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                    var sp = _ro.serializedProperty.GetArrayElementAtIndex (index);

                    rect.height = 18;

                    EditorGUI.PropertyField (rect, sp.FindPropertyRelative ("stateName"));
                    rect.y += 18;

                    EditorGUI.PropertyField (rect, sp.FindPropertyRelative ("fallbackStateName"), new GUIContent ("Fallback"));
                    rect.y += 18;

                    EditorGUI.PropertyField (rect, sp.FindPropertyRelative ("layer"));
                    rect.y += 18;

                    EditorGUI.PropertyField (rect, sp.FindPropertyRelative ("exclusiveLayer"));
                    rect.y += 18;

                    EditorGUI.PropertyField (rect, sp.FindPropertyRelative ("easeInDuration"));
                    rect.y += 18;

                    EditorGUI.PropertyField (rect, sp.FindPropertyRelative ("easeOutDuration"));
                    rect.y += 18;

                };

                _ro.elementHeight = 162-18*3;
            }

            // DoLayoutListを呼ばないと最終的に表示しない
            _ro.DoLayoutList ();

            serializedObject.ApplyModifiedProperties ();
        }
    }

}
