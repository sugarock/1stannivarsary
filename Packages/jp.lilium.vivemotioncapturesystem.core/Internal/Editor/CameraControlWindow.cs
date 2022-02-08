using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Cinemachine;

namespace Lilium.VMCStudio.Editor 
{

    public class CameraControlWindow : EditorWindow
    {
        public VisualTreeAsset rootVisualTreeAsset;
        public VisualTreeAsset cameraTemplateAsset;
        public VisualTreeAsset sceneTemplateAsset;
        public VisualTreeAsset cameraCharacterTemplateAsset;

        private SerializedObject _serializedObject;

        [SerializeField]
        private CameraEnvironment _model;

        private bool _validated = false;

        public static bool enableLiveUpdate = true;


        [MenuItem ("Window/VMC Studio/Camera Control Window %&I")]
        public static void ShowExample ()
        {
            CameraControlWindow wnd = GetWindow<CameraControlWindow> ();
            wnd.titleContent = new GUIContent ("Cameras");
        }

        public void OnEnable ()
        {
            Setup ();
        }

        void OnDisable()
        {
        }


        void Setup()
        {
            rootVisualElement.Clear ();
            var root = rootVisualTreeAsset.CloneTree ();

            _model = CameraEnvironment.instance;
            if (_model == null) {
                root.Add (new Label ("CameraEnvironment component not found in scene.\nPlease add it."));
                rootVisualElement.Add (root);
                _validated = false;
                return;
            }
            _serializedObject = new SerializedObject (_model);

            var so = new SerializedObject (this);
            root.Q<Toggle> ("live-update-toggle2").RegisterGetSetCallbacks (() => CameraPropertyDrawer.isEnableLiveUpdate, (value) => CameraPropertyDrawer.isEnableLiveUpdate = value);

            var camerasPropertyField = root.Q<PropertyField> ("cameras");
            camerasPropertyField.bindingPath = "cameras";
            camerasPropertyField.Bind (_serializedObject);
            camerasPropertyField.Q<Foldout>().value = true;

            var refreshButton = root.Q<ToolbarButton> ("reflesh-button");
            refreshButton.clicked += () => Setup ();            

            _validated = true;
            rootVisualElement.Add (root);
        }

        private void OnHierarchyChange ()
        {
            if (_validated != (CameraEnvironment.instance != null)) {
                Setup ();
            }

        }

        private void OnFocus ()
        {
            if (_validated != (CameraEnvironment.instance != null)) {
                Setup ();
            }
        }

    }

}