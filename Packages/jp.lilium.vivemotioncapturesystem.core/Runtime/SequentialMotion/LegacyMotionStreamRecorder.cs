using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Lilium.FbxExporter;
using System.IO;


namespace VMCCore
{

    public class LegacyMotionStreamRecorder
    {
        public float currentTime { get; private set; } = 0f;

        public bool isRecroding { get; private set; } = false;

        public GameObject root { get; private set; } = null;

        private struct CurveBinding
        {
            public Type type;

            public GameObject target;
            public Func<float> data;

            public LegacyMotionStreamData.Binding bind;
        }

        private List<CurveBinding> _bindings = new List<CurveBinding> ();

        public LegacyStreamMotionClip clip;

        public LegacyMotionStreamRecorder (GameObject root)
        {
            this.root = root;
        }

        public void BindProperty (GameObject target, Type componentType, string propertyName, Func<float> data)
        {
            Debug.Assert(clip != null);
            Debug.Assert(clip.data != null);
            Debug.Assert(clip.data.bindings != null);

            var path = MakeRelativePath (root.transform, target.transform);

            var clipbind = new LegacyMotionStreamData.Binding { path = path, propertyName = propertyName, curve = new LegacyMotionStreamData.Curve () };

            var bind = new CurveBinding { type = componentType, target = target, data = data, bind = clipbind };

            clip.data.bindings.Add(clipbind);
            _bindings.Add (bind);
        }

        public void BindTransformPosition (Transform target, bool recursive = false)
        {
            // RecordingUnbindTagがあるGameObjectはレコードしないように
            if (target.GetComponent<RecordingUnbindTag>() != null) return;
            
            BindProperty (target.gameObject, typeof (Transform), "m_LocalPosition.x", () => target.localPosition.x);
            BindProperty (target.gameObject, typeof (Transform), "m_LocalPosition.y", () => target.localPosition.y);
            BindProperty (target.gameObject, typeof (Transform), "m_LocalPosition.z", () => target.localPosition.z);

            if (recursive) {
                foreach (Transform child in target) {

                    BindTransformPosition (child, recursive);
                }
            }
        }

        public void BindTransformEulerRotation (Transform target, bool recursive = false)
        {
            // RecordingUnbindTagがあるGameObjectはレコードしないように
            if (target.GetComponent<RecordingUnbindTag>() != null) return;

            BindProperty (target.gameObject, typeof (Transform), "localEulerAnglesRaw.x", () => RawAngleValue (target.localEulerAngles.x));
            BindProperty (target.gameObject, typeof (Transform), "localEulerAnglesRaw.y", () => RawAngleValue (target.localEulerAngles.y));
            BindProperty (target.gameObject, typeof (Transform), "localEulerAnglesRaw.z", () => RawAngleValue (target.localEulerAngles.z));

            if (recursive) {
                foreach (Transform child in target) {
                    BindTransformEulerRotation (child, recursive);
                }
            }
        }
        public void BindTransformRotation (Transform target, bool recursive = false)
        {
            // RecordingUnbindTagがあるGameObjectはレコードしないように
            if (target.GetComponent<RecordingUnbindTag>() != null) return;

            BindProperty (target.gameObject, typeof (Transform), "m_LocalRotation.x", () => target.localRotation.x);
            BindProperty (target.gameObject, typeof (Transform), "m_LocalRotation.y", () => target.localRotation.y);
            BindProperty (target.gameObject, typeof (Transform), "m_LocalRotation.z", () => target.localRotation.z);
            BindProperty (target.gameObject, typeof (Transform), "m_LocalRotation.w", () => target.localRotation.w);

            if (recursive) {
                foreach (Transform child in target) {
                    BindTransformRotation (child, recursive);
                }
            }
        }


        public void TakeSnapshot (float deltaTime)
        {
            if (isRecroding) {
                currentTime += deltaTime;
            }

            foreach (var bind in _bindings) {
                bind.bind.curve.AddKey (currentTime, bind.data ());
            }

            isRecroding = true;
        }


        public void SaveToAnimationClip (AnimationClip clip)
        {
            var bindings = new List<RuntimeCurveBinding> ();
            foreach (var bind in _bindings) {
                var curve = bind.bind.curve.MakeAnimationCurve ();
                clip.SetCurve (bind.bind.path, bind.type, bind.bind.propertyName, curve);
                var curveBinding = new RuntimeCurveBinding {
                    target = bind.target,
                    type = bind.type,
                    propertyName = bind.bind.propertyName,
                    curve = curve
                };
                bindings.Add (curveBinding);
            }
            clip.frameRate = 60;

            // Quaternionキーを再調整
            clip.EnsureQuaternionContinuity ();

            // FBX出力用にAnimationClipのCurve情報を記録する
            RuntimeAnimationUtility.AddCurveBindings (clip, bindings);
        }

        public void SaveToAnimatonFile (string filePath)
        {
            // TODO: 新しいアニメーションファイルフォーマットで保存する
            //MessagePackFileUtility.Save (filePath, clip.data);

            //Debug.Log ($"Save to primitive animation file. file:{filePath}");
        }

        public void Clear ()
        {
            isRecroding = false;
            currentTime = 0f;
            _bindings.Clear ();
        }


        private string MakeRelativePath (Transform root, Transform target)
        {
            List<string> pths = new List<string> ();
            Transform t = target;
            while (t != root && t != t.root) {
                pths.Add (t.name);
                t = t.parent;
            }

            //pths.Add (root.name);
            pths.Reverse ();
            return string.Join ("/", pths);
        }
        private static float RawAngleValue (float value)
        {
            if (value > 180) {
                return value - 360;
            }
            return value;
        }

    }



}
