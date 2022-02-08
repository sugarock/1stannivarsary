using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace VMCCore.Facial
{
    [System.Serializable]
    public class FaceDescription
    {
        public string stateName;

        [SerializeField]
        public float easeInDuration = 0.05f;

        [SerializeField]
        public float easeOutDuration = 0.1f;

        public FaceLayerFlag exclusiveLayer;

        public FaceLayer layer = FaceLayer.Unknown;

        public string fallbackStateName;

        [System.NonSerialized]
        public FaceKey faceKey;

        [System.NonSerialized]
        public FaceKey fallbackFaceKey;
    }


    public class FacialAvatar : ScriptableObject
    {
        [SerializeField]
        public FaceDescription[] faceDescriptions = new FaceDescription[0];

        /// <summary>
        /// FacialKey > FacialDescription 変換マップ
        /// </summary>
        private Dictionary<FaceKey, int> _descriptionMap;

        /// <summary>
        /// Faceネーム　＞ FaceKey 変換マップ
        /// </summary>
        private Dictionary<NativeString32, int> _descpritionKeyMap;

        private void OnEnable ()
        {
            Build ();
        }

        private void OnValidate ()
        {
            Build ();
        }

        /// <summary>
        /// 実行時変換マップの作成
        /// </summary>
        public void Build ()
        {
            int extraIndex = 0;
            for (int i = 0; i < faceDescriptions.Length; i++) {
                if (string.IsNullOrWhiteSpace (faceDescriptions[i].stateName)) continue;

                faceDescriptions[i].faceKey = new FaceKey (faceDescriptions[i].stateName);
                faceDescriptions[i].fallbackFaceKey = new FaceKey (faceDescriptions[i].fallbackStateName);

                // Unknownの場合はExtend1~ に順次割り当てる
                if (faceDescriptions[i].faceKey.preset == FacePreset.Unknown) {
                    faceDescriptions[i].faceKey = new FaceKey (faceDescriptions[i].faceKey.name, FacePreset.Extend1 + extraIndex * 3);
                    extraIndex ++;
                }
            }


            // 変換マップ作成
            _descriptionMap = new Dictionary<FaceKey, int> ();
            _descpritionKeyMap = new Dictionary<NativeString32, int> ();
            for (int i = 0; i < faceDescriptions.Length; i++) {
                var f = faceDescriptions[i];
                if (string.IsNullOrWhiteSpace (f.stateName)) {
                    Debug.LogWarning ($"[VMCCore] Undefine facial key. key:{f.stateName}", this);
                    continue;
                };

                if (_descriptionMap.ContainsKey (f.faceKey)) {
                    Debug.LogWarning ($"[VMCCore] Duplicate facial key. key:{f.stateName}", this);
                    continue;
                };
                _descriptionMap.Add (f.faceKey, i);
                _descpritionKeyMap.Add (f.stateName, i);
            }

            //PrintDescriptions();
        }

        public int GetIndex (FaceKey key)
        {
            Debug.Assert (_descriptionMap != null);

            int index;
            if (!_descriptionMap.TryGetValue (key, out index)) {
                return default (int);
            }
            return index;
        }

        public FaceDescription GetItem (FaceKey key)
        {
            Debug.Assert (_descriptionMap != null);

            int index;
            if (!_descriptionMap.TryGetValue (key, out index)) {
                return default (FaceDescription);
            }
            return faceDescriptions[index];
        }

        public FaceDescription GetItem (NativeString32 name)
        {
            Debug.Assert(_descpritionKeyMap != null);

            int index;
            if (!_descpritionKeyMap.TryGetValue (name, out index)) {
                return default (FaceDescription);
            }
            return faceDescriptions[index];
        }

        public FaceLayer GetLayer (FaceKey key)
        {
            Debug.Assert (_descriptionMap != null);

            int index;
            if (!_descriptionMap.TryGetValue (key, out index)) {
                return 0;
            }
            return faceDescriptions[index].layer;
        }


        /// <summary>
        /// キー名からFaceAvatarに登録している該当のフェイスキーを得る
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public FaceKey Resolve (in NativeString32 name)
        {
            var description = GetItem (name);
            if (description == default(FaceDescription)) {
                return new FaceKey(name);
            }
            return description.faceKey;
        }


        public void PrintDescriptions ()
        {
            for (int i = 0; i < faceDescriptions.Length; i++) {
                Debug.Log ($"face description preset:{faceDescriptions[i].faceKey.preset} name:{faceDescriptions[i].faceKey.name}");
            }            
        }
    }


}