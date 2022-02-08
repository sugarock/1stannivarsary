using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;


namespace SequentialMotion
{

    // AvatarUtility and TQ class.
    // References: https://forum.unity.com/threads/recording-humanoid-animations-with-foot-ik.545015/
    public class TQ
    {
        public TQ (Vector3 translation, Quaternion rotation)
        {
            t = translation;
            q = rotation;
        }
        public Vector3 t;
        public Quaternion q;
        // Scale should always be 1,1,1
    }

    public static class AvatarUtility
    {
        static public TQ GetIKGoalTQ (Avatar avatar, float humanScale, AvatarIKGoal avatarIKGoal, TQ animatorBodyPositionRotation, TQ skeletonTQ)
        {
            int humanId = (int)HumanIDFromAvatarIKGoal (avatarIKGoal);
            if (humanId == (int)HumanBodyBones.LastBone)
                throw new System.InvalidOperationException ("Invalid human id.");
            MethodInfo methodGetAxisLength = typeof (Avatar).GetMethod ("GetAxisLength", BindingFlags.Instance | BindingFlags.NonPublic);
            if (methodGetAxisLength == null)
                throw new System.InvalidOperationException ("Cannot find GetAxisLength method.");
            MethodInfo methodGetPostRotation = typeof (Avatar).GetMethod ("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
            if (methodGetPostRotation == null)
                throw new System.InvalidOperationException ("Cannot find GetPostRotation method.");
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

        public static string MakeRelativePath (Transform root, Transform target)
        {
            List<string> pths = new List<string> ();
            Transform t = target;
            while (t != root && t != t.root) {
                pths.Add (t.name);
                t = t.parent;
            }

            //pths.Add (root.name);
            pths.Reverse ();
            return string.Join ("/", pths);
        }
    }

}
