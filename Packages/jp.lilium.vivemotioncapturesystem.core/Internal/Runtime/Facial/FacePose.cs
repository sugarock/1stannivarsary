using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace VMCCore.Facial
{

    /// <summary>
    /// 表情の姿勢情報
    /// いくつかの表情制御システムで加工される
    /// </summary>
    public struct FacePose : System.IDisposable
    {
        public NativeArray<float> expressions;

        public NativeArray<float> rigs;

        public bool isCreated => expressions.IsCreated;

        public static unsafe FacePose Create ()
        {
            FacePose f = new FacePose ();
            f.rigs = new NativeArray<float> (sizeof(FaceRig), Allocator.Persistent, NativeArrayOptions.ClearMemory);
            f.expressions = new NativeArray<float> (sizeof(FaceExpression), Allocator.Persistent, NativeArrayOptions.ClearMemory);
            return f;
        }

        public void Dispose ()
        {
            rigs.Dispose ();
            expressions.Dispose ();
        }

        public void SetValue (FaceKey key, float value)
        {
            if ((int)key.preset >= FaceDefine.MinExpressions && (int)key.preset < FaceDefine.MaxExpressions) {
                int index = (int)(key.preset - FaceDefine.MinExpressions) + (int)key.variation;
                Debug.Assert (index >= 0 && index < FaceDefine.LengthExpressions);
                expressions[index] = value;
            }
            else if ((int) key.preset >= FaceDefine.MinRigs && (int)key.preset < FaceDefine.MaxRigs) {
                var index = (int)(key.preset - FaceDefine.MinRigs);
                Debug.Assert (index >= 0 && index < FaceDefine.LengthRigs);
                rigs[index] = value;
            }
            else {
                Debug.LogError ($"[{GetType().Assembly.GetName().Name}] Unknown index FacialExpressionKey index:{key.preset}");
            }
        }

        public float GetValue (FaceKey key)
        {
            if ((int)key.preset >= FaceDefine.MinExpressions && (int)key.preset < FaceDefine.MaxExpressions) {
                var index = (int)(key.preset - FaceDefine.MinExpressions);
                Debug.Assert (index >= 0 && index < FaceDefine.LengthExpressions);
                return expressions[index];
            }
            else if ((int)key.preset >= FaceDefine.MinRigs && (int)key.preset < FaceDefine.MaxRigs) {
                var index = (int)(key.preset - FaceDefine.MinRigs);
                Debug.Assert (index >= 0 && index < FaceDefine.LengthRigs);
                return rigs[index];
            }
            else {
                Debug.LogError ($"[{GetType().Assembly.GetName().Name}] Unknown index FacialExpressionKey preset:{key.preset} name:{key.name}");
                return 0;
            }
        }

        /// <summary>
        /// ウェイト値が0ではないFacialKey一覧の取得
        /// </summary>
        /// <param name="allocator"></param>
        /// <returns></returns>
        public NativeList<FaceKey> GetActiveFacialKeyList (Allocator allocator)
        {
            var activeFacialKeys = new NativeList<FaceKey> (FaceDefine.LengthRigs + FaceDefine.LengthExpressions, allocator);
            for (int i = 0; i < expressions.Length; i++) {
                if (expressions[i] == 0) continue;
                activeFacialKeys.Add (new FaceKey(i+ FaceDefine.MinExpressions));
            }
            for (int i = 0; i < rigs.Length; i++) {
                if (rigs[i] == 0) continue;
                activeFacialKeys.Add (new FaceKey (i + FaceDefine.MinRigs));
            }
            return activeFacialKeys;
        }


        public unsafe void CopyFrom (ref FaceExpression expression, ref FaceRig rig)
        {
            Debug.Assert(expressions != null);
            Debug.Assert(expressions.IsCreated);
            Debug.Assert(expressions.GetUnsafePtr() != null);

            UnsafeUtility.CopyStructureToPtr (ref expression, expressions.GetUnsafePtr ());
            UnsafeUtility.CopyStructureToPtr (ref rig, rigs.GetUnsafePtr ());
        }


        //TODO: 実装
        public unsafe void CopyTo (ref FaceExpression expression, ref FaceRig rig)
        {
            throw new System.NotImplementedException();
        }

    }
}