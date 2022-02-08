using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Lilium.VMCStudio
{
    [System.Serializable]
    public sealed class SceneItem
    {
        public string name;
        public string path { get; private set; }
        public bool isPersistent { get; private set; }

        public SceneItem (string path, bool isPersistent)
        {
            this.name = Path.GetFileNameWithoutExtension (path);
            this.path = path;
            this.isPersistent = isPersistent;
        }

        public bool isActive { get => SceneManager.GetActiveScene ().path == path; }
    }

    public sealed class SceneEnvironment : MonoBehaviour
    {
        public SceneItem[] scenes = new SceneItem[0];

        /// <summary>
        /// 中心となる永続的なシーン
        /// </summary>
        public static Scene persistenceScene;


        public static SceneEnvironment instance => _GetOrFindInstance ();

        static SceneEnvironment _instance;

        static SceneEnvironment _GetOrFindInstance ()
        {
            if (_instance == null) {
                _instance = FindObjectOfType<SceneEnvironment>();
            }
            return _instance;
        }

        public SceneItem FindSceneData (string path)
        {
            return scenes.Where (t => t.path == path).FirstOrDefault ();
        }

        public IEnumerator ChangeSceneCoroutine (SceneItem nextSceneData)
        {
            var prevScene = SceneManager.GetActiveScene ();
            var prevSceneData = SceneEnvironment.instance.FindSceneData (prevScene.path);

            Scene nextScene = SceneManager.GetSceneByPath (nextSceneData.path);

            if (nextSceneData.path == prevScene.path) {
                yield break;
            }
            if (!nextScene.isLoaded) {
#if UNITY_EDITOR
                if (Application.isPlaying) {
                    var asyncLoad = EditorSceneManager.LoadSceneAsyncInPlayMode (nextSceneData.path, new LoadSceneParameters (LoadSceneMode.Additive));
                    while (!asyncLoad.isDone) {
                        yield return null;
                    }
                    nextScene = SceneManager.GetSceneByPath (nextSceneData.path);
                }
                else {
                    nextScene = EditorSceneManager.OpenScene (nextSceneData.path, OpenSceneMode.Additive);
                }
#else
                nextScene = SceneManager.GetSceneByPath (nextSceneData.path);
#endif
            }

            Debug.Assert (nextScene.isLoaded);

            SceneManager.SetActiveScene (nextScene);


            if (!prevSceneData.isPersistent) {
                if (Application.isPlaying) {
                    SceneManager.UnloadSceneAsync (prevScene);
                }
#if UNITY_EDITOR
                else {
                    EditorSceneManager.CloseScene (prevScene, false);
                }
#endif
            }
        }


        public static bool HasGameObjectInScene (Scene scene, GameObject go)
        {
            Debug.Assert (scene != null);
            Debug.Assert (go != null);

            var roots = scene.GetRootGameObjects ();
            return roots.Contains (go.transform.root.gameObject);
        }

        public void UpdateScenes (string[] scenePathes)
        {
            // 1番目のシーンは自動的にパーシステンス（永続的な）シーンとなる
            scenes = scenePathes.Select ( (t,i) => new SceneItem (t, i == 0)).ToArray ();

            var persistenceSceneItem = scenes.Take (0).FirstOrDefault ();
            if (persistenceSceneItem != null) {
                persistenceScene = SceneManager.GetSceneByPath (persistenceSceneItem.path);
            }
            else {
                persistenceScene = new Scene ();
            }
        }

    }
}

