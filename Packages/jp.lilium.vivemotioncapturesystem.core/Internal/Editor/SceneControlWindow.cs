using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Unity.EditorCoroutines.Editor;


namespace Lilium.VMCStudio.Editor 
{

    public class SceneControlWindow : EditorWindow
    {
        public VisualTreeAsset rootVisualTreeAsset;
        public VisualTreeAsset sceneTemplateAsset;

        private SerializedObject _serializedObject;

        [SerializeField]
        private SceneEnvironment _model;

        private bool _validated = false;

        private ListView _scenesListView;

        [MenuItem ("Window/VMC Studio/Scene Control Window")]
        public static void Show ()
        {
            SceneControlWindow wnd = GetWindow<SceneControlWindow> ();
            wnd.titleContent = new GUIContent ("Scenes");

        }

        public void OnEnable ()
        {

            _Bind ();

        }

        void OnDisable()
        {
            _Unbind ();
        }

        void _Bind ()
        {
            rootVisualElement.Clear ();
            var root = rootVisualTreeAsset.CloneTree ();
            root.style.flexGrow = 1;

            _model = SceneEnvironment.instance;
            if (_model == null) {
                root.Add (new Label ("SceneEnvironment component not found in scene.\nPlease add it."));
                rootVisualElement.Add (root);
                _validated = false;
                return;
            }
            _serializedObject = new SerializedObject (_model);   


            _scenesListView = root.Q<ListView> ("scenes-list");
            _scenesListView.selectionType = SelectionType.None;
            _scenesListView.itemHeight = 18;
            _scenesListView.makeItem = () => sceneTemplateAsset.CloneTree ();
            _scenesListView.bindItem = (e, i) => {

                Debug.Assert (_model != null);
                Debug.Assert (_model.scenes != null);

                // インデックスを見て最後の要素を廃棄
                if (i >= _model.scenes.Count ()) {
                    e.style.display = DisplayStyle.None;
                    return;
                }

                var sceneData = SceneEnvironment.instance.scenes[i];
                e.Q<Button> ().text = sceneData.name;
                e.Q<Button> ().clickable.clicked += () => {
                    EditorCoroutineUtility.StartCoroutine (_model.ChangeSceneCoroutine (sceneData), this);
                    _scenesListView.selectedIndex = i;
                };

                if (sceneData.isActive) {
                    _scenesListView.selectedIndex = i;
                }

            };
            _scenesListView.onSelectionChanged += (selects) => {
            };
            _scenesListView.bindingPath = "scenes";
            //_scenesListView.Bind (_serializedObject);

            _validated = true;
            root.Bind (_serializedObject);
            rootVisualElement.Add (root);            
        }

        void _Unbind()
        {
        }


    }

}