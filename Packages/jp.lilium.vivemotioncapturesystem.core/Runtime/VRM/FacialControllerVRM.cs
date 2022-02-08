#if VRM_0_INCLUDED

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRM;

namespace VMCCore.Facial.Public
{
    /// <summary>
    /// 表情情報の抽象レイヤー
    /// VRM対応版
    /// </summary>
    [RequireComponent (typeof (Facial))]
    [ExecuteAlways]
    public class FacialControllerVRM : MonoBehaviour
    {
        public BlendShapeAvatar blendShapeAvatar;

        public Face face { get; private set; }

        private BlendShapeMerger _merger;

        /// <summary>
        /// FaceKey と VRM BlendShapeKey の対応キャッシュ
        /// </summary>
        private Dictionary<FaceKey, BlendShapeKey> _blendShapeMap = new Dictionary<FaceKey, BlendShapeKey> ();

        /// <summary>
        /// BlendShapeAvatar.GetClip () GC対策マップ
        /// </summary>
        /// <typeparam name="FaceKey"></typeparam>
        /// <typeparam name="BlendShapeKey"></typeparam>
        /// <returns></returns>
        private Dictionary<BlendShapeKey, BlendShapeClip> _blendShapeAvatarClipMap = new Dictionary<BlendShapeKey, BlendShapeClip> ();


        private VRMLookAtHead _lookAtHead;

        private FacePose _pose;

        /// <summary>
        /// FacialAvatarキャッシュのリビルド用
        /// </summary>
        private VMCCore.Facial.FacialAvatar _buildedfaceAvatar;

        void Awake ()
        {
            face = GetComponent<Face> ();
        }


        // Use this for initialization
        void OnEnable ()
        {
            _pose = FacePose.Create ();

            _Build ();

            if (blendShapeAvatar != null) {
                var validatedBlendShapeAvatarClips = blendShapeAvatar.Clips.Where (t => t != null);  // ここで NULL を排除しなとエラーが発生する。VRM.BlendShapeMerger のバグ？
                _merger = new BlendShapeMerger (validatedBlendShapeAvatarClips, this.transform);
            }

            _lookAtHead = GetComponent<VRMLookAtHead> ();
        }

        void OnDisable ()
        {
            if (_merger != null && blendShapeAvatar != null) {
                var validatedBlendShapeAvatarClips = blendShapeAvatar.Clips.Where (t => t != null);  // ここで NULL を排除しなとエラーが発生する。VRM.BlendShapeMerger のバグ？
                _merger.RestoreMaterialInitialValues (validatedBlendShapeAvatarClips);
            }
            _merger = null;
            _pose.Dispose ();
        }


        protected  void OnValidate ()
        {
            if (face != null) {
                if (face.avatar != _buildedfaceAvatar) {
                    _Build ();
                }
            }
        }

        private void Reset ()
        {
            var blendShapeProxy = GetComponent<VRMBlendShapeProxy> ();
            if (blendShapeProxy != null) {
                blendShapeAvatar = blendShapeProxy.BlendShapeAvatar;
                blendShapeProxy.enabled = false;
            }
        }

        private void _Build()
        {
            if (face.avatar != null) face.avatar.Build();

            _blendShapeMap = new Dictionary<FaceKey, BlendShapeKey> ();

            if (face != null && face.avatar != null) {
                foreach (var f in face.avatar.faceDescriptions) {
                    var key = face.avatar.Resolve (f.stateName);
                    if (_blendShapeMap.ContainsKey (key)) {
                        continue;
                    };
                    _blendShapeMap.Add (key, MakeBlendShapeKey (f.stateName));
                }
            }

            foreach (var keyValue in _blendShapeMap) {
                var key = keyValue.Key;
                var blendShapeKey = _blendShapeMap[key];

                _blendShapeAvatarClipMap[blendShapeKey] = blendShapeAvatar.GetClip (blendShapeKey);
            }

            _buildedfaceAvatar = face.avatar;
        }


        private void Start ()
        {
        }


        void LateUpdate ()
        {
            if (face == null) return;

            face.CopyTo (ref _pose);
            FaceNormalizer.NormalizeHard (ref _pose, face.avatar);

            _UpdateBlendShapes (!Application.isPlaying);
        }

        /// <summary>
        /// VRM BlendShapeMegerによる反映を実行
        /// Meshrenderer.BlendShapeのウェイトを更新する。
        /// </summary>
        /// <param name="changedValueUpdate">BlendShapeManager.AccumulateValue() で MeshRenderer.BlendShape.value が書き換わる。値が変わっていなくてもパラメーター変更のマークが入るため、ウェイトが変化していない場合は呼ばないようするためのフラグ。エディットモード用</param>
        void _UpdateBlendShapes (bool changedValueUpdate)
        {
            if (_merger == null) return;

            // VRM BlendShape の適用
            float totalWeight = 0;
            foreach (var keyValue in _blendShapeMap) {

                var key = keyValue.Key;
                var weight = _pose.GetValue (key);
                var blendShapeKey = _blendShapeMap[key];

                var blendShapeClip = _blendShapeAvatarClipMap[blendShapeKey];
                if (blendShapeClip == null) continue;

                // ２値なら0以上のウェイトを１に設定
                if (blendShapeClip.IsBinary) {
                    weight = Mathf.Ceil (weight);
                }

                if (changedValueUpdate) {
                    if (Mathf.Approximately (_merger.GetValue (_blendShapeMap[key]), weight)) continue;
                }

                _merger.AccumulateValue (blendShapeKey, weight);
                totalWeight += weight;
            }

            // 目線の適用
            if (_lookAtHead != null) {
                float eyeYaw = _pose.GetValue (new FaceKey (FacePreset.EyesX)) * -1; // 鏡像のため左右逆転
                float eyePitch = _pose.GetValue (new FaceKey (FacePreset.EyesY));

                _lookAtHead.RaiseYawPitchChanged (eyeYaw * 120, eyePitch * 120);
            }

            if (_merger != null) {
                // ブレンドシェイプのウェイトの合計値が１以下ならニュートラルのウェイトを上げる
                _merger.AccumulateValue ( BlendShapeKey.CreateFromPreset(BlendShapePreset.Neutral), Mathf.Clamp01(1f - totalWeight));
                _merger.Apply ();
            }
        }

        public static BlendShapeKey MakeBlendShapeKey (string name)
        {
            // VRM BlendShapePresetとの名称が一致すればPresetの方を、なければ同名のBlendShapeKeyを作成する
            BlendShapePreset preset;
            if (System.Enum.TryParse (name, out preset)) {
                return BlendShapeKey.CreateFromPreset (preset);
            }
            else {
                return BlendShapeKey.CreateUnknown (name);
            }
        }

    }
}

#endif