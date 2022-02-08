using UnityEngine;
using UnityEditor;

[CustomEditor(typeof (PlanarReflection))]
public class PlanarReflectionInspector : Editor
{
	SerializedProperty m_SP_Camera;
	SerializedProperty m_SP_TextureSize;
	SerializedProperty m_SP_ClipPlaneOffset;
	SerializedProperty m_SP_ReflectionStrength;
	SerializedProperty m_SP_ReflectionTint;
	SerializedProperty m_SP_ReflectLayers;

	SerializedProperty m_SP_EnableHeightFading;
	SerializedProperty m_SP_HeightFadingPos;
	SerializedProperty m_SP_HeightFadingNorm;
	SerializedProperty m_SP_SdrDepth;

	SerializedProperty m_SP_BumpTex;
	SerializedProperty m_SP_BumpStrength;
	SerializedProperty m_SP_BumpTexScale;

	SerializedProperty m_SP_MaskTex;
	SerializedProperty m_SP_MaskTexScale;
	SerializedProperty m_SP_Iterations;
	SerializedProperty m_SP_Interpolation;
	SerializedProperty m_SP_SdrBlur;

	void OnEnable ()
	{
		m_SP_Camera = serializedObject.FindProperty ("m_Camera");
		m_SP_TextureSize = serializedObject.FindProperty ("m_TextureSize");
		m_SP_ClipPlaneOffset = serializedObject.FindProperty ("m_ClipPlaneOffset");
		m_SP_ReflectionStrength = serializedObject.FindProperty ("m_ReflectionStrength");
		m_SP_ReflectionTint = serializedObject.FindProperty ("m_ReflectionTint");
		m_SP_ReflectLayers = serializedObject.FindProperty ("m_ReflectLayers");
		
		m_SP_EnableHeightFading = serializedObject.FindProperty ("m_EnableHeightFading");
		m_SP_HeightFadingPos = serializedObject.FindProperty ("m_HeightFadingPos");
		m_SP_HeightFadingNorm = serializedObject.FindProperty ("m_HeightFadingNorm");
		m_SP_SdrDepth = serializedObject.FindProperty ("m_SdrDepth");
		
		m_SP_BumpTex = serializedObject.FindProperty ("m_BumpTex");
		m_SP_BumpStrength = serializedObject.FindProperty ("m_BumpStrength");
		m_SP_BumpTexScale = serializedObject.FindProperty ("m_BumpTexScale");
		
		m_SP_MaskTex = serializedObject.FindProperty ("m_MaskTex");
		m_SP_MaskTexScale = serializedObject.FindProperty ("m_MaskTexScale");
		m_SP_Iterations = serializedObject.FindProperty ("m_Iterations");
		m_SP_Interpolation = serializedObject.FindProperty ("m_Interpolation");
		m_SP_SdrBlur = serializedObject.FindProperty ("m_SdrBlur");
	}
	public override void OnInspectorGUI ()
	{
		serializedObject.Update ();
		EditorGUILayout.BeginVertical ("GroupBox");
		{
			EditorGUILayout.LabelField ("Basic");
			EditorGUILayout.PropertyField (m_SP_Camera, true);
			EditorGUILayout.PropertyField (m_SP_TextureSize, true);
			EditorGUILayout.PropertyField (m_SP_ClipPlaneOffset, true);
			EditorGUILayout.PropertyField (m_SP_ReflectionStrength, true);
			EditorGUILayout.PropertyField (m_SP_ReflectionTint, true);
			EditorGUILayout.PropertyField (m_SP_ReflectLayers, true);
		}
		EditorGUILayout.EndVertical ();
		EditorGUILayout.BeginVertical ("GroupBox");
		{
			EditorGUILayout.LabelField ("Bump");
			EditorGUILayout.PropertyField (m_SP_BumpTex, true);
			EditorGUILayout.PropertyField (m_SP_BumpStrength, true);
			EditorGUILayout.PropertyField (m_SP_BumpTexScale, true);
		}
		EditorGUILayout.EndVertical ();
		EditorGUILayout.BeginVertical ("GroupBox");
		{
			EditorGUILayout.LabelField ("Height Fading");
			EditorGUILayout.PropertyField (m_SP_EnableHeightFading, true);
			EditorGUILayout.PropertyField (m_SP_HeightFadingPos, true);
			EditorGUILayout.PropertyField (m_SP_HeightFadingNorm, true);
			EditorGUILayout.PropertyField (m_SP_SdrDepth, true);
		}
		EditorGUILayout.EndVertical ();
		if (!m_SP_EnableHeightFading.boolValue)
		{
			EditorGUILayout.BeginVertical ("GroupBox");
			{
				EditorGUILayout.LabelField ("Mask");
				EditorGUILayout.PropertyField (m_SP_MaskTex, true);
				EditorGUILayout.PropertyField (m_SP_MaskTexScale, true);
			}
			EditorGUILayout.EndVertical ();
			EditorGUILayout.BeginVertical ("GroupBox");
			{
				EditorGUILayout.LabelField ("Blur");
				EditorGUILayout.PropertyField (m_SP_SdrBlur, true);
				EditorGUILayout.PropertyField (m_SP_Iterations, true);
				EditorGUILayout.PropertyField (m_SP_Interpolation, true);
			}
			EditorGUILayout.EndVertical ();
		}
		serializedObject.ApplyModifiedProperties ();
	}
}