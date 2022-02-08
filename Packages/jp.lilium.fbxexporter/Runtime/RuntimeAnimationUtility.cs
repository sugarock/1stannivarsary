using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Lilium.FbxExporter
{
    public struct RuntimeCurveBinding
    {
        public System.Type type;
        public string propertyName;

        public AnimationCurve curve;
        public Object target;
    }

    public static class RuntimeAnimationUtility
    {
        private static Dictionary<AnimationClip, RuntimeCurveBinding[]> bindingMap = new Dictionary<AnimationClip, RuntimeCurveBinding[]> ();

        public static RuntimeCurveBinding[] GetCurveBindings (AnimationClip source)
        {
            if (!bindingMap.ContainsKey (source)) {
                return null;
            }

            return bindingMap[source].ToArray ();
        }

        public static void AddCurveBindings (AnimationClip source, IEnumerable<RuntimeCurveBinding> bindings)
        {
            bindingMap[source] = bindings.ToArray ();
        }

        public static void ClearBindings(AnimationClip source)
        {
            bindingMap.Remove (source);
        }

    }
}
