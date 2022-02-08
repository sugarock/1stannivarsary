using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEditor;

public class DuplicateTimeline : EditorWindow
{
    [MenuItem("imeline/Duplicate Timeline", true)]
    private static bool DupTimelineValidate()
    {
        if (UnityEditor.Selection.activeObject as GameObject == null)
        {
            Debug.Log("Null active object");
            return false;
        }

        GameObject playableDirectorObj = UnityEditor.Selection.activeObject as GameObject;

        PlayableDirector playableDirector = playableDirectorObj.GetComponent<PlayableDirector>();
        if (playableDirector == null)
        {
            Debug.Log("Null playableDirector");
            return false;
        }

        TimelineAsset timelineAsset = playableDirector.playableAsset as TimelineAsset;
        if (timelineAsset == null)
        {
            Debug.Log("Null timelineAsset");
            return false;
        }

        string path = AssetDatabase.GetAssetPath(timelineAsset);
        if (string.IsNullOrEmpty(path))
        {
            Debug.Log("Null timeline asset path");
            return false;
        }

        return true;
    }

    [MenuItem("Timeline/Duplicate Timeline")]
    public static void DupTimeline()
    {
        // 再生可能なディレクターを取得し、タイムラインを取得します
        GameObject playableDirectorObj = UnityEditor.Selection.activeObject as GameObject;
        PlayableDirector playableDirector = playableDirectorObj.GetComponent<PlayableDirector>();
        TimelineAsset timelineAsset = playableDirector.playableAsset as TimelineAsset;

        // 複製
        string path = AssetDatabase.GetAssetPath(timelineAsset);
        string newPath = path.Replace(".", "(Clone).");
        if (!AssetDatabase.CopyAsset(path, newPath))
        {
            Debug.LogError("Couldn't Clone Asset");
            return;
        }

        // バインディングのコピー
        TimelineAsset newTimelineAsset = AssetDatabase.LoadMainAssetAtPath(newPath) as TimelineAsset;
        PlayableBinding[] oldBindings = timelineAsset.outputs.ToArray();
        PlayableBinding[] newBindings = newTimelineAsset.outputs.ToArray();
        for (int i = 0; i < oldBindings.Length; i++)
        {
            playableDirector.playableAsset = timelineAsset;
            Object boundTo = playableDirector.GetGenericBinding(oldBindings[i].sourceObject);

            playableDirector.playableAsset = newTimelineAsset;
            playableDirector.SetGenericBinding(newBindings[i].sourceObject, boundTo);
        }

        // 公開された参照をコピーする
        playableDirector.playableAsset = newTimelineAsset;
        foreach (TrackAsset newTrackAsset in newTimelineAsset.GetRootTracks())
        {
            foreach (TimelineClip newClip in newTrackAsset.GetClips())
            {
                foreach (FieldInfo fieldInfo in newClip.asset.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(ExposedReference<>))
                    {
                        // 古い公開名を取得する
                        object exposedReference = fieldInfo.GetValue(newClip.asset);
                        PropertyName oldExposedName = (PropertyName)fieldInfo.FieldType
                            .GetField("exposedName")
                            .GetValue(exposedReference);
                        bool isValid;

                        // 古い公開値を取得する
                        Object oldExposedValue = playableDirector.GetReferenceValue(oldExposedName, out isValid);
                        if (!isValid)
                        {
                            Debug.LogError("Failed to copy exposed references to duplicate timeline. Could not find: " + oldExposedName);
                            return;
                        }

                        // 構造体のexposedNameを置き換えます
                        PropertyName newExposedName = new PropertyName(UnityEditor.GUID.Generate().ToString());
                        fieldInfo.FieldType
                            .GetField("exposedName")
                            .SetValue(exposedReference, newExposedName);

                        // ExposedReferenceを設定します
                        fieldInfo.SetValue(newClip.asset, exposedReference);

                        // PlayableDirectorのリファレンスを設定する
                        playableDirector.SetReferenceValue(newExposedName, oldExposedValue);
                    }
                }
            }
        }
    }
}