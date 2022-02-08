using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;

using Unity.EditorCoroutines.Editor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;


namespace Lilium.VMCStudio.Editor
{


    [System.Serializable]

    public sealed class EnvironmentManager
    {
        public static EnvironmentManager instance { get; private set; }

        public static event System.Action playModeStateChanged;

        [InitializeOnLoadMethod]
        static void _Initialize ()
        {
            //Debug.Log ("EnvironmentManager._Initialize");

            var instance = new EnvironmentManager ();

            EditorApplication.playModeStateChanged += instance.OnPlayModeStateChanged;
            Undo.postprocessModifications += instance.OnPostprocessModifications;
            Undo.undoRedoPerformed += instance.OnUndoRedoPerformed;
            Undo.willFlushUndoRecord += instance.OnWillFlushUndoRecord;
            CustomPostprocessor.onAssetChanged += instance.OnAssetChanged;
            EditorSceneManager.sceneSaved += instance.OnSceneSaved;

            // 起動直後はアセットの読み込みができないため、コルーチンを使って初期化処理を遅延
            EditorCoroutineUtility.StartCoroutine (instance._InitializeDelay (), instance);
        }

        IEnumerator _InitializeDelay ()
        {
            //Debug.Log ("EnvironmentManager._InitializeDelay");
            _UpdateFromBuildSettings ();

            UpdateSceneModifications ();
            UpdateAssetModifications ();

            yield return null;
        }

        static void _Uninitialize ()
        {
            EditorApplication.playModeStateChanged -= instance.OnPlayModeStateChanged;
            CustomPostprocessor.onAssetChanged -= instance.OnAssetChanged;
            Undo.postprocessModifications -= instance.OnPostprocessModifications;
            Undo.undoRedoPerformed -= instance.OnUndoRedoPerformed;
            Undo.willFlushUndoRecord -= instance.OnWillFlushUndoRecord;
            instance = null;
        }


        private void OnSceneSaved (Scene scene)
        {
            UpdateSceneModifications ();
        }

        private void OnWillFlushUndoRecord ()
        {
            //OnSceneModifications ();
        }

        private void OnUndoRedoPerformed ()
        {
        }

        private UndoPropertyModification[] OnPostprocessModifications (UndoPropertyModification[] modifications)
        {
            return modifications;
        }

        private void OnAssetChanged (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            UpdateAssetModifications ();
        }


        private void UpdateSceneModifications ()
        {
#if VIRTUALPRODUCTION_DEBUG
            Debug.Log ("[VMCStudio] Update modification scenes.");
#endif
            EditorCoroutineUtility.StartCoroutine (_UpdateSceneModificationssDelay (), this);
        }

        IEnumerator _UpdateSceneModificationssDelay ()
        {
            //CameraEnvironment.instance.UpdateCameras ();
            yield return null;
        }


        private void UpdateAssetModifications ()
        {
#if VIRTUALPRODUCTION_DEBUG
            Debug.Log ("[VMCStudio] Update modification assets.");
#endif
        }

        private void OnPlayModeStateChanged (PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.EnteredPlayMode || obj == PlayModeStateChange.EnteredEditMode) {
                _UpdateSceneModificationssDelay ();
                playModeStateChanged?.Invoke ();
            }
        }


        public void _UpdateFromBuildSettings ()
        {
            if (SceneEnvironment.instance != null) {
                var scenePathes = EditorBuildSettings.scenes.Select( t => t.path).ToArray();
                SceneEnvironment.instance.UpdateScenes (scenePathes);
            }
        }        

    }


}
