using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Collections;
using System.Reflection;

namespace VMCCore.Retarget
{

    public static class VMCAvatarUtility
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


        // TODO: rename
        static public Vector3 GetRootPosition (Vector3 value)
        {
            return new Vector3 (value.x, 0, value.z);
        }

        static public Vector3 GetAntiRootPosition (Vector3 value)
        {
            return new Vector3 (0, value.y, 0);
        }


        public static void ReconstructionHumanoidSkeleton (Animator source, float shoulderWidth)
        {
            var leftShoulder = source.GetBoneTransform (HumanBodyBones.LeftUpperArm);
            var rightShoulder = source.GetBoneTransform (HumanBodyBones.RightUpperArm);

            leftShoulder.position = new Vector3 (shoulderWidth * -0.5f, leftShoulder.position.y, leftShoulder.position.z);
            rightShoulder.position = new Vector3 (shoulderWidth * +0.5f, rightShoulder.position.y, rightShoulder.position.z);
        }


        /// <summary>
        /// HumaniodAvatarの構築
        /// </summary>
        /// <param name="source">ルート</param>
        /// <param name="pivotPosition">足元の位置。ワールド座標系で。</param>
        /// <param name="frontDirection">正面の向き。ワールド座標系で。</param>
        /// <returns></returns>
        public static Avatar BuildHumanoidAvatar (Animator source, Vector3 pivotPosition, Vector3 frontDirection, IList<Transform> boneTransforms)
        {
            var frontRotation = Quaternion.LookRotation (frontDirection, Vector3.up);

            // 一時的に位置角度を変更。アバター作成時にはワールド座標系でZ方向を正面に向き、かつ足元の高さを０の位置に配置しておく必要がある。
            var originalRotation = source.transform.rotation;
            var origibalPosition = source.transform.position;
            source.transform.rotation = Quaternion.Inverse (frontRotation) * source.transform.rotation;
            source.transform.position = Vector3.zero;

            // HumanBone リストを形成する
            List<HumanBone> humanBones = new List<HumanBone> (HumanTrait.BoneName.Length);
            for (int i = 0; i < HumanTrait.BoneName.Length; i++) {
                string humanBoneName = HumanTrait.BoneName[i];

                var bone = boneTransforms[i];
                if (bone != null) {
                    HumanBone humanBone = new HumanBone ();
                    humanBone.humanName = humanBoneName;
                    humanBone.boneName = bone.name;
                    humanBone.limit.useDefaultValues = true;

                    humanBones.Add (humanBone);
                }
            }

            var skeltons = new List<Transform> ();
            MakeSkeleton (source.transform, ref skeltons);

            List<SkeletonBone> skeletonBones = new List<SkeletonBone> (skeltons.Count);
            for (int i = 0; i < skeltons.Count; i++) {
                Transform bone = skeltons[i];
                SkeletonBone skeltonBone = new SkeletonBone ();
                skeltonBone.name = bone.name;
                skeltonBone.position = bone.localPosition;
                skeltonBone.rotation = bone.localRotation;
                skeltonBone.scale = Vector3.one;

                skeletonBones.Add (skeltonBone);
            }

            HumanDescription humanDesc = new HumanDescription ();
            humanDesc.human = humanBones.ToArray ();
            humanDesc.skeleton = skeletonBones.ToArray ();
            humanDesc.upperArmTwist = 0.5f;
            humanDesc.lowerArmTwist = 0.5f;
            humanDesc.upperLegTwist = 0.5f;
            humanDesc.lowerLegTwist = 0.5f;
            humanDesc.armStretch = 0.05f;
            humanDesc.legStretch = 0.05f;
            humanDesc.feetSpacing = 0f;
            humanDesc.hasTranslationDoF = false;

            var avatar = AvatarBuilder.BuildHumanAvatar (source.gameObject, humanDesc);
            if (!avatar.isValid || !avatar.isHuman) {
                Debug.LogError ("setup error");
                return null;
            }

            source.transform.rotation = originalRotation;
            source.transform.position = origibalPosition;
            return avatar;
        }

        /// <summary>
        /// 再帰的にTransformを走査して、ボーン構造を生成する
        /// </summary>
        /// <param name="current">現在のTransform</param>
        private static void MakeSkeleton (Transform current, ref List<Transform> skeletons)
        {
            skeletons.Add (current);

            for (int i = 0; i < current.childCount; i++) {
                Transform child = current.GetChild (i);
                MakeSkeleton (child, ref skeletons);
            }
        }

    }

}
