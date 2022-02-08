using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Linq;
using Cinemachine;

namespace Lilium.VMCStudio.Editor
{

    [CustomPropertyDrawer(typeof(CameraData))]
    public class CameraPropertyDrawer : PropertyDrawer
    {
        public static bool isEnableLiveUpdate = true;


        private static Camera _captureCamera;

        private static uint _createCount = 0;

        static float VerticalFOVToFocalLength (float fov, float sensorSize)
        {
            return sensorSize * 0.5f / Mathf.Tan (Mathf.Deg2Rad * fov * 0.5f);
        }

        static float FocalLengthToVerticalFOV (float focalLength, float sensorSize)
        {
            if (focalLength < Mathf.Epsilon)
                return 180f;
            return Mathf.Rad2Deg * 2.0f * Mathf.Atan (sensorSize * 0.5f / focalLength);
        }

        // UIToolkit 
        public override VisualElement CreatePropertyGUI (SerializedProperty property)
        {
            var target = SerializedPropertyUtility.GetTargetObjectOfProperty (property) as CameraData;

            //Debug.Assert (target.virtualCameraGo != null);

            var container = new VisualElement ();

            var vsTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset> ("Packages/jp.lilium.vivemotioncapturesystem.core/Editor/UI/CameraItem.uxml");
            Debug.Assert (vsTree != null);
            var root = vsTree.CloneTree ();

            root.Q<ObjectField> ("name").objectType = typeof (GameObject);
            root.Q<ObjectField> ("name").BindProperty (property.FindPropertyRelative ("virtualCamera"));

            root.Q<Button> ("image").clickable.clicked += () => {
                var t = SerializedPropertyUtility.GetTargetObjectOfProperty (property) as CameraData;

                if (t.virtualCamera == null) return;
                t.virtualCamera.MoveToTopOfPrioritySubqueue ();
                if (!Application.isPlaying) {
                    EditorApplication.QueuePlayerLoopUpdate ();
                }                
            };

            var indexNumber = CameraEnvironment.instance.cameras.IndexOf(target);
            root.Q<Label>("no").text = (indexNumber+1).ToString();

            // FOV
            if (target.virtualCamera != null) {
                var serializedVirtualCamera = SerializedObjectPool.GetOrCreate (target.virtualCamera);
                root.Q<Slider> ("fov").BindProperty (serializedVirtualCamera.FindProperty ("m_Lens.FieldOfView"));
                root.Q<TextField> ("fov-text").BindProperty (serializedVirtualCamera.FindProperty ("m_Lens.FieldOfView"));
            }

            uint drawSyncCount = _createCount % 8;
            container.schedule.Execute (() => {
                var t = SerializedPropertyUtility.GetTargetObjectOfProperty (property) as CameraData;
                if (t?.virtualCamera == null) return;

                if (++t.drawCount != 0 && drawSyncCount != (t.drawCount % 8)) return; 

                if (CameraPropertyDrawer.isEnableLiveUpdate) {
                    _UpdateCameraImage (t);
                }

                var image = root.Q<Button> ("image");

                image.style.backgroundImage = new StyleBackground (t.captureTexture);
                image.MarkDirtyRepaint ();
                image.EnableInClassList("camera-live", t.isLive);
            }).Every (20);

            root.BindProperty (property);
            container.Add (root);

            target.drawCount = 0;

            _createCount++;
            return container;
        }

        // IMGUI
        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty (position, label, property);
            EditorGUI.PropertyField (position, property.FindPropertyRelative ("virtualCamera"));
            EditorGUI.EndProperty ();
        }
        
        void _UpdateCameraImage (CameraData target)
        {
            if (_captureCamera == null) {
                _CraeteCaptureCamera ();
            }

            Debug.Assert (target.virtualCamera != null);
            //if (cameraData.virtualCamera == null) return;

            var renderTexture = RenderTexture.GetTemporary (320, 180, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            _captureCamera.transform.position = target.virtualCamera.State.FinalPosition;
            _captureCamera.transform.rotation = target.virtualCamera.State.FinalOrientation;
            _captureCamera.fieldOfView = target.virtualCamera.State.Lens.FieldOfView;
            _captureCamera.aspect = target.virtualCamera.State.Lens.Aspect;
            _captureCamera.nearClipPlane = target.virtualCamera.State.Lens.NearClipPlane;
            _captureCamera.farClipPlane = target.virtualCamera.State.Lens.FarClipPlane;
            _captureCamera.lensShift = target.virtualCamera.State.Lens.LensShift;
            _captureCamera.orthographicSize = target.virtualCamera.State.Lens.OrthographicSize;
            _captureCamera.targetTexture = renderTexture;
            _captureCamera.rect = new Rect (0, 0, 1, 1);
            _captureCamera.Render ();
            Graphics.CopyTexture (renderTexture, target.captureTexture);
            RenderTexture.ReleaseTemporary (renderTexture);
        }

        private void _CraeteCaptureCamera ()
        {
            Debug.Assert (_captureCamera == null);

            _captureCamera = new GameObject ("Capture Camera", typeof (Camera)).GetComponent<Camera> ();
            _captureCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
            _captureCamera.enabled = false;
        }

    }

}
