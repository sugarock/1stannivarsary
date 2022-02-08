using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace VMCCore.Retarget
{
    /// <summary>
    /// referenced: https://forum.unity.com/threads/recording-humanoid-animations-with-foot-ik.545015/
    /// </summary>
    public struct TQ
    {
        public TQ (Vector3 translation, Quaternion rotation)
        {
            this.t = translation;
            this.q = rotation;
        }

        public Vector3 t;

        public Quaternion q;

        //scale should always be 1,1,1
    }

    public class AvatarUtility
    {
        private static MethodInfo methodGetAxisLength;

        private static MethodInfo methodGetPostRotation;

        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            methodGetAxisLength = typeof (Avatar).GetMethod ("GetAxisLength", BindingFlags.Instance | BindingFlags.NonPublic);
            if (methodGetAxisLength == null)
                throw new InvalidOperationException ("Cannot find GetAxisLength method.");
            methodGetPostRotation = typeof (Avatar).GetMethod ("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
            if (methodGetPostRotation == null)
                throw new InvalidOperationException ("Cannot find GetPostRotation method.");

        }

        public static TQ GetIKGoalTQ (Avatar avatar, float humanScale, AvatarIKGoal avatarIKGoal, ref TQ animatorBodyPositionRotation, ref TQ skeletonTQ)
        {
            int humanId = (int)HumanIDFromAvatarIKGoal (avatarIKGoal);
            if (humanId == (int)HumanBodyBones.LastBone)
                throw new InvalidOperationException ("Invalid human id.");
            Quaternion postRotation = (Quaternion)methodGetPostRotation.Invoke (avatar, new object[] { humanId });
            var goalTQ = new TQ (skeletonTQ.t, skeletonTQ.q * postRotation);
            if (avatarIKGoal == AvatarIKGoal.LeftFoot || avatarIKGoal == AvatarIKGoal.RightFoot) {
                // Here you could use animator.leftFeetBottomHeight or animator.rightFeetBottomHeight rather than GetAxisLenght
                // Both are equivalent but GetAxisLength is the generic way and work for all human bone
                float axislength = (float)methodGetAxisLength.Invoke (avatar, new object[] { humanId });
                Vector3 footBottom = new Vector3 (axislength, 0, 0);
                goalTQ.t += (goalTQ.q * footBottom);
            }
            // IK goal are in avatar body local space
            Quaternion invRootQ = Quaternion.Inverse (animatorBodyPositionRotation.q);
            goalTQ.t = invRootQ * (goalTQ.t - animatorBodyPositionRotation.t);
            goalTQ.q = invRootQ * goalTQ.q;
            goalTQ.t /= humanScale;

            return goalTQ;
        }

        static public HumanBodyBones HumanIDFromAvatarIKGoal (AvatarIKGoal avatarIKGoal)
        {
            HumanBodyBones humanId = HumanBodyBones.LastBone;
            switch (avatarIKGoal) {
                case AvatarIKGoal.LeftFoot: humanId = HumanBodyBones.LeftFoot; break;
                case AvatarIKGoal.RightFoot: humanId = HumanBodyBones.RightFoot; break;
                case AvatarIKGoal.LeftHand: humanId = HumanBodyBones.LeftHand; break;
                case AvatarIKGoal.RightHand: humanId = HumanBodyBones.RightHand; break;
            }
            return humanId;
        }

        // world to body local space
        static public TQ BodyLocaTQ (in TQ worldTQ, in TQ bodyTQ, float humanScale)
        {
            TQ localTQ;
            Quaternion invRootQ = Quaternion.Inverse (bodyTQ.q);
            localTQ.t = invRootQ * (worldTQ.t - bodyTQ.t);
            localTQ.q = invRootQ * worldTQ.q;
            localTQ.t /= humanScale;
            return localTQ;
        }
    }

}
