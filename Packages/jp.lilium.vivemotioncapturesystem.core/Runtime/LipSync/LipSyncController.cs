using UnityEngine;
using System.Collections;
using System.Linq;

namespace VMCCore.Facial
{
    [DisallowMultipleComponent]
    public class LipSyncController : MonoBehaviour
    {
        static readonly FaceKey[] kMouseBlendShapePresets = new FaceKey[] {
            FacePreset.A,
            FacePreset.I,
            FacePreset.U,
            FacePreset.E,
            FacePreset.O,
        };

        public Face face { get; private set; }

        public LipSyncProvider provider;


        [Tooltip ("最も高い口形素のみを使用する")]
        public bool maxVisemesEmphasis = true;

        [Tooltip ("口形素の倍率")]
        public float visemesMultiply = 1.0f;

        [Tooltip ("最小判定のしきい値")]
        public float visemesThreashold = 0.1f;

        [Tooltip ("「A」の口形のみにする")]
        public bool onlyVisemesA = false;

        [Tooltip ("口を閉じる時間")]
        public float closeTime = 0.5f;

        void Reset()
        {
            provider = GetComponent<LipSyncProvider> ();
        }


        /// <summary>
        /// Start this instance.
        /// </summary>
        void Awake ()
        {
            face = GetComponent<Face> ();
        }

        /// <summary>
        /// Update this instance.
        /// </summary>
        void Update ()
        {
            if (face == null) return;
            if (provider == null) return;

            int maxIndex = 0;
            float maxVisemes = 0;
            float[] visemes = new float[6];
            for (int i = 0; i < provider.visemes.Length; i++) {
                visemes[i] = provider.visemes[i];
                if (visemes[i] < visemesThreashold) visemes[i] = 0;

                if (maxVisemes < visemes[i]) {
                    maxIndex = i;
                    maxVisemes = visemes[i];
                }
            }

            if (maxVisemesEmphasis) {
                for (int i = 0; i < kMouseBlendShapePresets.Length; i++) {
                    if (i != maxIndex) visemes[i] = 0.0f;
                }
            }
            // すべてを母音を「A」にまとめる。
            if (onlyVisemesA) {
                face.SetValue (kMouseBlendShapePresets[0], maxVisemes * visemesMultiply);
            }
            /// A, I, U, E, O のそれぞれに反映する。
            else {
                for (int i = 0; i < kMouseBlendShapePresets.Length; i++) {
                    var prevValue = face.GetValue (kMouseBlendShapePresets[i]);
                    var value = visemes[i] * visemesMultiply;

                    value = (value > prevValue) ? value : Mathf.Clamp(prevValue - Time.deltaTime / closeTime, value, 2);

                    face.SetValue (kMouseBlendShapePresets[i], value);
                }
            }
        }


    }

}
