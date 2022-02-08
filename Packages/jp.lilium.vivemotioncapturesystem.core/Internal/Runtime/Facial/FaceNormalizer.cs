using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Collections;


namespace VMCCore.Facial
{

    /// <summary>
    /// 表情ウェイトのノーマライズ機能
    /// </summary>
    public static class FaceNormalizer
    {


        private static float GetTotalLayerWeight(NativeArray<float> layerTotalWeight, int flag)
        {
            float weight = 0;
            for (int i = 0; i < layerTotalWeight.Length; i++) {
                if (((1 << i) & (int)flag) != 0) {
                    weight += layerTotalWeight[i];
                }
            }
            return weight;
        }

        private static void AddLayerWeights (NativeArray<float> layerTotalWeight, int flag, float weight)
        {
            for (int i = 0; i < layerTotalWeight.Length; i++) {
                if (((1 << i) & (int)flag) != 0) {
                    layerTotalWeight[i] += weight;
                }
            }
        }


        /// <summary>
        /// 蓄積した値をレイヤー毎にウェイトの合成処理する
        /// 排他制御。インデックスが低い表情が優先される。
        /// Ex. Joy と Angry が 1の場合、 Joy:1 Angry:0 となる。
        /// </summary>
        public static void NormalizeHard (ref FacePose pose, FacialAvatar avatar)
        {
            if (avatar == null) return;

            var layerWeights = new NativeArray<float> ((int)FaceLayer.Max, Allocator.Temp, NativeArrayOptions.ClearMemory);
            for (int i = avatar.faceDescriptions.Length-1; i >= 0; i--) {
                var desc = avatar.faceDescriptions[i];
                var key = desc.faceKey;
                var item = avatar.GetItem (key);
                int layers = (int)item.exclusiveLayer | (1 << (int)item.layer);

                var weight = pose.GetValue (key);

                if (Mathf.Approximately(weight, 0)) {
                    pose.SetValue (key, weight);
                }
                else {
                    float totalWeight = layerWeights[(int)item.layer];
                    weight = Mathf.Clamp (weight, 0, 1 - totalWeight);

                    pose.SetValue (key, weight);

                    if (item.layer != FaceLayer.Unknown) {
                        AddLayerWeights (layerWeights, layers, weight);
                    }
                }
            }
        }


        /// <summary>
        /// イージング
        /// </summary>
        public static void Ease (ref FacePose pose, ref FacePose target, FacialAvatar avatar)
        {
            if (avatar == null) return;

            for (int i = 0; i < avatar.faceDescriptions.Length; i++) {
                var desc = avatar.faceDescriptions[i];

                var weight = pose.GetValue (desc.faceKey);
                var targetWeight = target.GetValue (desc.faceKey);

                if (targetWeight > weight) {
                    weight += Mathf.Min (targetWeight - weight,  (1 / desc.easeInDuration) * Time.deltaTime); 
                }
                else {
                    weight -= Mathf.Min (weight - targetWeight, (1 / desc.easeOutDuration) * Time.deltaTime);
                }
                pose.SetValue (desc.faceKey, weight);
            }

        }

    }
}
