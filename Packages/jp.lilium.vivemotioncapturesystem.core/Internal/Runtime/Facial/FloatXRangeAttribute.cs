using System.Collections;
using UnityEngine;
using UnityEditor;

using System;

namespace VMCCore.Facial
{

    [AttributeUsage (AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class FloatXRangeAttribute : PropertyAttribute
    {
        public readonly float min;
        public readonly float max;

        public FloatXRangeAttribute (float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
}

