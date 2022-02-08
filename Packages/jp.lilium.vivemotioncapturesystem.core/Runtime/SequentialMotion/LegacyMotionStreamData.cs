using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using System;

namespace VMCCore
{
    [Serializable]
    public class LegacyMotionStreamData
    {
        [Serializable]
        public struct Keyframe
        {
            public float time;

            public float value;
        }

        [Serializable]
        public class Curve
        {
            public List<Keyframe> frames = new List<Keyframe> ();

            public void AddKey(float time, float value)
            {
                frames.Add (new Keyframe { time = time, value = value });
            }

            public AnimationCurve MakeAnimationCurve ()
            {
                var ac = new AnimationCurve ();
                foreach (var frame in frames) {
                    ac.AddKey (frame.time, frame.value);
                }
                return ac;
            }
        }


        [Serializable]
        public struct Binding
        {
            public string path;

            public string propertyName;

            public Curve curve;
        }

        public int samplingRate = 60;


        public List<Binding> bindings = new List<Binding> ();

    }


}