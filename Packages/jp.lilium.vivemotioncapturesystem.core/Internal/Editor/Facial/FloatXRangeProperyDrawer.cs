using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Globalization;

namespace VMCCore.Facial
{
    [CustomPropertyDrawer (typeof (FloatXRangeAttribute))]
    internal sealed class FloatXDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
        {
            return 38/2;
        }

        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
        {
            var rangeAttribute = (FloatXRangeAttribute)base.attribute;

            position.height = 18;

            var proeprtyX = property.FindPropertyRelative ("x");
            if (proeprtyX != null) {
                EditorGUI.Slider (position, proeprtyX, rangeAttribute.min, rangeAttribute.max, CultureInfo.CurrentCulture.TextInfo.ToTitleCase (property.name));
            }

        }
    }
}

