using UnityEngine;
#if UNITY_2019_3_OR_NEWER
using UnityEngine.Animations;
#else
using UnityEngine.Experimental.Animations;
#endif
using Unity.Collections;

namespace VMCCore.Retarget
{

    /// <summary>
    /// </summary>
    public struct StancePosePlayableJob : IAnimationJob
    {


        public void ProcessRootMotion(AnimationStream stream)
        {

        }

        public void ProcessAnimation(AnimationStream stream)
        {
            var humanStream = stream.AsHuman();
            Debug.Assert (humanStream.isValid);

            humanStream.ResetToStancePose();
        }

    }


}