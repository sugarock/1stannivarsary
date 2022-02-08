using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;

namespace VMCCore.Facial
{
    public static class FaceDefine
    {
        public const int MinRigs = (int)FacePreset.Blink;
        public const int MaxRigs = (int)FacePreset.O + 1;

        public const int MinExpressions = (int)FacePreset.Joy;
        public const int MaxExpressions = (int)FacePreset.Extend8 + 3;

        public const int LengthRigs = MaxRigs - MinRigs;
        public const int LengthExpressions = MaxExpressions - MinExpressions;
    }

    [Serializable]
    public enum FacePreset
    {
        Unknown,

        // blink
        Blink = 1,
        Blink_L,
        Blink_R,

        // eyes direction
        EyesX,                      // 目の水平方向
        EyesY,                      // 目の垂直方向

        // mouth
        A,
        I,
        U,
        E,
        O,

        // expressions
        Joy = 128,                  // 喜び
        Angry = Joy + 3,            // 怒り
        Sorrow = Angry + 3,         // 悲しみ
        Fun = Sorrow + 3,           // 楽しみ、笑い
        Surprise = Fun +3,          // 驚き
        Cold = Surprise +3,         // 冷め、ジト目
        Trouble = Cold + 3,         // 困り
        Confusion = Trouble + 3,    // 混乱
        Shy = Confusion + 3,        // 照れ
        Extend1 = Shy + 3,          // 拡張用
        Extend2 = Extend1 + 3,      // 拡張用
        Extend3 = Extend2 + 3,      // 拡張用
        Extend4 = Extend3 + 3,      // 拡張用
        Extend5 = Extend4 + 3,      // 拡張用
        Extend6 = Extend5 + 3,      // 拡張用
        Extend7 = Extend6 + 3,      // 拡張用
        Extend8 = Extend7 + 3,      // 拡張用
    }

    [StructLayout (LayoutKind.Sequential)]
    [System.Serializable]
    public struct FaceRig
    {

        [Range (0, 1)]
        public float blink;
        [Range (0, 1)]
        public float blinkR;
        [Range (0, 1)]
        public float blinkL;

        public float2 eyes;

        [Range (0, 1)]
        public float A;
        [Range (0, 1)]
        public float I;
        [Range (0, 1)]
        public float U;
        [Range (0, 1)]
        public float E;
        [Range (0, 1)]
        public float O;
    }

    [StructLayout (LayoutKind.Sequential)]
    [System.Serializable]
    public struct FaceExpression
    {
        [FloatXRange (0, 1)]
        public float3 joy;
        [FloatXRange (0, 1)]
        public float3 angry;
        [FloatXRange (0, 1)]
        public float3 sorrow;
        [FloatXRange (0, 1)]
        public float3 fun;
        [FloatXRange (0, 1)]
        public float3 surprise;
        [FloatXRange (0, 1)]
        public float3 cold;
        [FloatXRange (0, 1)]
        public float3 trouble;
        [FloatXRange (0, 1)]
        public float3 confusion;
        [FloatXRange (0, 1)]
        public float3 shy;
        [FloatXRange (0, 1)]
        public float3 extend1;
        [FloatXRange (0, 1)]
        public float3 extend2;
        [FloatXRange (0, 1)]
        public float3 extend3;
        [FloatXRange (0, 1)]
        public float3 extend4;
        [FloatXRange (0, 1)]
        public float3 extend5;
        [FloatXRange (0, 1)]
        public float3 extend6;
        [FloatXRange (0, 1)]
        public float3 extend7;
        [FloatXRange (0, 1)]
        public float3 extend8;
        [FloatXRange (0, 1)]
        public float3 extend9;
        [FloatXRange (0, 1)]
        public float3 extend10;
    }



    public enum FaceLayer
    {
        Unknown = 0,
        Blink,
        Eyes,
        Mouth,
        Eyebrow,
        
        Expressions = 8,
        Extention1,
        Extention2,
        Extention3,
        Max
    }


    [Flags]
    public enum FaceLayerFlag
    {
        Blink = 1 << FaceLayer.Blink,
        Eyes = 1 << FaceLayer.Eyes,
        Mouth = 1 << FaceLayer.Mouth,
        Eyebrow = 1 << FaceLayer.Eyebrow,

        Expressions = 1 << FaceLayer.Expressions,
        Extention1 = 1 << FaceLayer.Extention1,
        Extention2 = 1 << FaceLayer.Extention2,
        Extention3 = 1 << FaceLayer.Extention3,
    }


    [Serializable]
    public struct FaceKey : IEquatable<FaceKey>, IComparable<FaceKey>
    {
        /// <summary>
        /// Enum.ToString() のGC回避用キャッシュ
        /// </summary>
        private static readonly Dictionary<FacePreset, NativeString32> FacePresetToNameMap = new Dictionary<FacePreset, NativeString32> ();

        private static readonly Dictionary<NativeString32, FacePreset> FacePresetFromNameMap = new Dictionary<NativeString32, FacePreset> ();

        private static bool _isInitialized = false;

        private static void Initialize ()
        {
            foreach (FacePreset e in Enum.GetValues (typeof (FacePreset))) {
                FacePresetToNameMap[e] = e.ToString ();
                FacePresetFromNameMap[e.ToString ()] = e;
            }
            _isInitialized = true;
        }

        public NativeString32 name;
        public FacePreset preset;
        public ushort variation;

        public FaceKey (NativeString32 name, ushort variation = 0)
        {
            if (!_isInitialized) Initialize ();

            this.preset = FacePreset.Unknown;
            this.variation = variation;
            this.name = name;

            FacePreset key;
            if (FacePresetFromNameMap.TryGetValue (name, out key)) {
                this.preset = key;
                this.variation = variation;
            }
        }

        public FaceKey (NativeString32 name, FacePreset preset, ushort variation = 0)
        {
            if (!_isInitialized) Initialize ();

            this.preset = preset;
            this.variation = variation;
            this.name = name;
            this.variation = variation;
        }

        public FaceKey (FacePreset preset, ushort variation = 0)
        {
            if (!_isInitialized) Initialize ();

            this.preset = preset;
            this.variation = variation;

            NativeString32 name;
            if (FacePresetToNameMap.TryGetValue (this.preset, out name)) {
                this.name = name;
            }
            else {
                Debug.LogError ($"Faital error. unknown preset name. preset:{preset}");
                this.name = default (NativeString32);
            }
        }

        public FaceKey (int preset, ushort variation = 0)
        {
            if (!_isInitialized) Initialize ();

            this.preset = (FacePreset)preset;
            this.variation = variation;

            NativeString32 name;
            if (FacePresetToNameMap.TryGetValue (this.preset, out name)) {
                this.name = name;
            }
            else {
                Debug.LogError ($"Faital error. unknown preset name. preset:{preset}");
                this.name = default (NativeString32);
            }
        }


        public static implicit operator FaceKey (FacePreset value)
        {
            return new FaceKey(value, 0);
        }


        public override string ToString ()
        {
            return $"{name}[{variation}]";
        }

        public bool Equals (FaceKey other)
        {
            return preset == other.preset && variation == other.variation && name.Equals(other.name);
        }

        public override bool Equals (object obj)
        {
            if (obj is FaceKey) {
                return Equals ((FaceKey)obj);
            }
            else {
                return false;
            }
        }

        public override int GetHashCode ()
        {
            return name.GetHashCode ();
        }


        public int CompareTo (FaceKey other)
        {
            return (((int)preset) * 256 + variation) - (((int)other.preset) * 256 + other.variation);
        }

        public static bool operator == (FaceKey left, FaceKey right)
        {
            return left.Equals (right);
        }

        public static bool operator != (FaceKey left, FaceKey right)
        {
            return !(left == right);
        }

    }
}