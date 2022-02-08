using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace VMCCore.Facial
{

    [DisallowMultipleComponent]
    public class Face : MonoBehaviour
    {
        [SerializeField]
        public FacialAvatar avatar;

        public FaceRig rigs;

        public FaceExpression expressions;


        public void CopyTo (ref FacePose pose)
        {
            pose.CopyFrom (ref expressions, ref rigs);
        }

        public void CopyFrom (ref FacePose pose)
        {
            pose.CopyTo (ref expressions, ref rigs);
        }


        /// <summary>
        /// イーズ処理付きのウェイト設定
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value">目標ウェイト値</param>
        /// <param name="easeInDuration"></param>
        /// <param name="easeOutDuration"></param>
        public void SetEasingValue (FaceKey key, float value)
        {
            // 現在値
            var current = GetValue (key);

            if (Mathf.Approximately(value, current)) return;

            var desc = avatar.GetItem (key);

            // 値が変化する方向を不等号で
            var dir = value >= current ? 1 : -1;

            if (Mathf.Abs(value) > Mathf.Abs(current)) {
                var easeInDuration = desc != null ? desc.easeInDuration : 0.05f;
                current += (1 / easeInDuration) * Time.deltaTime * dir;
            }
            else {
                var easeOutDuration = desc != null ? desc.easeOutDuration : 0.1f;
                current += (1 / easeOutDuration) * Time.deltaTime * dir;
            }

            // 目標値を超えることはないように
            if (dir > 0) {
                current = Mathf.Min(value, current);
            }
            else {
                current = Mathf.Max(value, current);
            }

            SetValue (key, current);
        }

        public unsafe void SetValue (FaceKey key, float value)
        {
            int index = (int)key.preset;//avatar.GetIndex (key);

            if ((int)index >= FaceDefine.MinRigs && (int)index < FaceDefine.MaxRigs) {
                var ptr = (float*)UnsafeUtility.AddressOf (ref rigs);
                ptr[index-FaceDefine.MinRigs] = value;
            }
            else if ((int)index >= FaceDefine.MinExpressions && (int)index < FaceDefine.MaxExpressions) {
                var ptr = (float*)UnsafeUtility.AddressOf (ref expressions);
                ptr[index-FaceDefine.MinExpressions] = value;
            }
            else {
                Debug.LogError ($"[VMCCore] Unknown FaceKey. index:{index}");
            }
        }

        public unsafe float GetValue (FaceKey key)
        {
            int index = (int)key.preset;// avatar.GetIndex (key);

            if ((int)index >= FaceDefine.MinRigs && (int)index < FaceDefine.MaxRigs) {
                var ptr = (float*)UnsafeUtility.AddressOf (ref rigs);
                return ptr[index-FaceDefine.MinRigs];
            }
            else if ((int)index >= FaceDefine.MinExpressions && (int)index < FaceDefine.MaxExpressions) {
                var ptr = (float*)UnsafeUtility.AddressOf (ref expressions);
                return ptr[index-FaceDefine.MinExpressions];
            }
            else {
                Debug.LogError ($"[VMCCore] Unknown FaceKey. index:{index}");
                return 0;
            }
        }

        public NativeList<FaceKey> GetActiveFacialKeyList (Allocator allocator)
        {
            using (var data = FacePose.Create ()) {
                data.CopyFrom (ref expressions, ref rigs);
                return data.GetActiveFacialKeyList (allocator);
            }
        }

    }

}