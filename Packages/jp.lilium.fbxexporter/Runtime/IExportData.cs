using UnityEngine;
using UnityEngine.Timeline;
using System.Collections.Generic;

#if UNITY_EDITOR2
using UnityEditor;
#endif

namespace Lilium.FbxExporter
{
    /// <summary>
    /// Export data containing extra information required to export
    /// </summary>
    internal interface IExportData
    {
        HashSet<GameObject> Objects { get; }
    }

    /// <summary>
    /// Export data containing what to export when
    /// exporting animation only.
    /// </summary>
    internal class AnimationOnlyExportData : IExportData
    {
        // map from animation clip to GameObject that has Animation/Animator
        // component containing clip
        public Dictionary<AnimationClip, GameObject> animationClips;

        // set of all GameObjects to export
        public HashSet<GameObject> goExportSet;
        public HashSet<GameObject> Objects { get { return goExportSet; } }

        // map from GameObject to component type to export
        public Dictionary<GameObject, System.Type> exportComponent;

        // first clip to export
        public AnimationClip defaultClip;

        public AnimationOnlyExportData(
            Dictionary<AnimationClip, GameObject> animClips,
            HashSet<GameObject> exportSet,
            Dictionary<GameObject, System.Type> exportComponent
        )
        {
            this.animationClips = animClips;
            this.goExportSet = exportSet;
            this.exportComponent = exportComponent;
            this.defaultClip = null;
        }

        public AnimationOnlyExportData()
        {
            this.animationClips = new Dictionary<AnimationClip, GameObject>();
            this.goExportSet = new HashSet<GameObject>();
            this.exportComponent = new Dictionary<GameObject, System.Type>();
            this.defaultClip = null;
        }

        /// <summary>
        /// collect all object dependencies for given animation clip
        /// </summary>
        public void CollectDependencies(
            AnimationClip animClip,
            GameObject rootObject,
            IExportOptions exportOptions
        )
        {
            Debug.Assert(rootObject != null);
            Debug.Assert(exportOptions != null);

            if (this.animationClips.ContainsKey(animClip))
            {
                // we have already exported gameobjects for this clip
                return;
            }

#if UNITY_EDITOR2
            // NOTE: the object (animationRootObject) containing the animation is not necessarily animated
            // when driven by an animator or animation component.
            this.animationClips.Add(animClip, rootObject);

            foreach (EditorCurveBinding uniCurveBinding in AnimationUtility.GetCurveBindings(animClip))
            {
                Object uniObj = AnimationUtility.GetAnimatedObject(rootObject, uniCurveBinding);
                if (!uniObj)
                {
                    continue;
                }

                GameObject unityGo = ModelExporter.GetGameObject(uniObj);
                if (!unityGo)
                {
                    continue;
                }

                if (!exportOptions.AnimateSkinnedMesh && unityGo.GetComponent<SkinnedMeshRenderer>())
                {
                    continue;
                }

                // If we have a clip driving a camera or light then force the export of FbxNodeAttribute
                // so that they point the right way when imported into Maya.
                if (unityGo.GetComponent<Light>())
                    this.exportComponent[unityGo] = typeof(Light);
                else if (unityGo.GetComponent<Camera>())
                    this.exportComponent[unityGo] = typeof(Camera);

                this.goExportSet.Add(unityGo);
            }
#endif
        }

        /// <summary>
        /// collect all objects dependencies for animation clips.
        /// </summary>
        public void CollectDependencies(
            AnimationClip[] animClips,
            GameObject rootObject,
            IExportOptions exportOptions
        )
        {
            Debug.Assert(rootObject != null);
            Debug.Assert(exportOptions != null);
            
            foreach (var animClip in animClips)
            {
                CollectDependencies(animClip, rootObject, exportOptions);
            }
        }

        /// <summary>
        /// Get the property propertyName from object obj using reflection.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns>property propertyName as an object</returns>
        private static object GetPropertyReflection(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName).GetValue(obj, null);
        }




        /// <summary>
        /// Get the filename of the format {model}@{anim}.fbx from the given object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>filename for use for exporting animation clip</returns>
        public static string GetFileName(Object obj)
        {
            return obj.name;
        }
    }
}