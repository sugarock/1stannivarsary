using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace VMCCore
{
    [CreateAssetMenu]
    public class LegacyStreamMotionClip : ScriptableObject
    {
        public LegacyMotionStreamData data = new LegacyMotionStreamData ();
    }

}