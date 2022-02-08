using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SequentialMotion
{
    public static class SequentialMotionSettings
    {
        /// <summary>
        /// Humanoid用のMuscleマッピング
        /// </summary>
        public static readonly Dictionary<string, string> TraitPropMap = new Dictionary<string, string>
        {
            {"Left Thumb 1 Stretched", "LeftHand.Thumb.1 Stretched"},
            {"Left Thumb Spread", "LeftHand.Thumb.Spread"},
            {"Left Thumb 2 Stretched", "LeftHand.Thumb.2 Stretched"},
            {"Left Thumb 3 Stretched", "LeftHand.Thumb.3 Stretched"},
            {"Left Index 1 Stretched", "LeftHand.Index.1 Stretched"},
            {"Left Index Spread", "LeftHand.Index.Spread"},
            {"Left Index 2 Stretched", "LeftHand.Index.2 Stretched"},
            {"Left Index 3 Stretched", "LeftHand.Index.3 Stretched"},
            {"Left Middle 1 Stretched", "LeftHand.Middle.1 Stretched"},
            {"Left Middle Spread", "LeftHand.Middle.Spread"},
            {"Left Middle 2 Stretched", "LeftHand.Middle.2 Stretched"},
            {"Left Middle 3 Stretched", "LeftHand.Middle.3 Stretched"},
            {"Left Ring 1 Stretched", "LeftHand.Ring.1 Stretched"},
            {"Left Ring Spread", "LeftHand.Ring.Spread"},
            {"Left Ring 2 Stretched", "LeftHand.Ring.2 Stretched"},
            {"Left Ring 3 Stretched", "LeftHand.Ring.3 Stretched"},
            {"Left Little 1 Stretched", "LeftHand.Little.1 Stretched"},
            {"Left Little Spread", "LeftHand.Little.Spread"},
            {"Left Little 2 Stretched", "LeftHand.Little.2 Stretched"},
            {"Left Little 3 Stretched", "LeftHand.Little.3 Stretched"},
            {"Right Thumb 1 Stretched", "RightHand.Thumb.1 Stretched"},
            {"Right Thumb Spread", "RightHand.Thumb.Spread"},
            {"Right Thumb 2 Stretched", "RightHand.Thumb.2 Stretched"},
            {"Right Thumb 3 Stretched", "RightHand.Thumb.3 Stretched"},
            {"Right Index 1 Stretched", "RightHand.Index.1 Stretched"},
            {"Right Index Spread", "RightHand.Index.Spread"},
            {"Right Index 2 Stretched", "RightHand.Index.2 Stretched"},
            {"Right Index 3 Stretched", "RightHand.Index.3 Stretched"},
            {"Right Middle 1 Stretched", "RightHand.Middle.1 Stretched"},
            {"Right Middle Spread", "RightHand.Middle.Spread"},
            {"Right Middle 2 Stretched", "RightHand.Middle.2 Stretched"},
            {"Right Middle 3 Stretched", "RightHand.Middle.3 Stretched"},
            {"Right Ring 1 Stretched", "RightHand.Ring.1 Stretched"},
            {"Right Ring Spread", "RightHand.Ring.Spread"},
            {"Right Ring 2 Stretched", "RightHand.Ring.2 Stretched"},
            {"Right Ring 3 Stretched", "RightHand.Ring.3 Stretched"},
            {"Right Little 1 Stretched", "RightHand.Little.1 Stretched"},
            {"Right Little Spread", "RightHand.Little.Spread"},
            {"Right Little 2 Stretched", "RightHand.Little.2 Stretched"},
            {"Right Little 3 Stretched", "RightHand.Little.3 Stretched"},
        };
    }


    public struct MotionPose
    {
        public struct HumanoidBone
        {
            public Vector3 LocalPosition;
            public Quaternion LocalRotation;
        }

        public int FrameCount;

        public float Time;

        public Vector3 BodyRootPosition;
        public Quaternion BodyRootRotation;

        public Vector3 BodyPosition;
        public Quaternion BodyRotation;
        public Vector3 LeftfootIK_Pos;
        public Quaternion LeftfootIK_Rot;
        public Vector3 RightfootIK_Pos;
        public Quaternion RightfootIK_Rot;

        public float[] Muscles;

        public List<HumanoidBone> HumanoidBones;
    }


    public class MotionStreamData
    {
        private List<MotionPose> Poses = new List<MotionPose> ();

        public void Add(in MotionPose pose)
        {
            Poses.Add (pose);
        }

#if UNITY_EDITOR
        public void ExportGenericAnimation (Animator animator)
        {
            var clip = new AnimationClip { frameRate = 30 };
            AnimationUtility.SetAnimationClipSettings (clip, new AnimationClipSettings { loopTime = false });

            var bones = Poses[0].HumanoidBones;
            for (int i = 0; i < bones.Count; i++) {
                var positionCurveX = new AnimationCurve ();
                var positionCurveY = new AnimationCurve ();
                var positionCurveZ = new AnimationCurve ();
                var rotationCurveX = new AnimationCurve ();
                var rotationCurveY = new AnimationCurve ();
                var rotationCurveZ = new AnimationCurve ();
                var rotationCurveW = new AnimationCurve ();

                foreach (var p in Poses) {
                    positionCurveX.AddKey (p.Time, p.HumanoidBones[i].LocalPosition.x);
                    positionCurveY.AddKey (p.Time, p.HumanoidBones[i].LocalPosition.y);
                    positionCurveZ.AddKey (p.Time, p.HumanoidBones[i].LocalPosition.z);
                    rotationCurveX.AddKey (p.Time, p.HumanoidBones[i].LocalRotation.x);
                    rotationCurveY.AddKey (p.Time, p.HumanoidBones[i].LocalRotation.y);
                    rotationCurveZ.AddKey (p.Time, p.HumanoidBones[i].LocalRotation.z);
                    rotationCurveW.AddKey (p.Time, p.HumanoidBones[i].LocalRotation.w);
                }

                var path = AvatarUtility.MakeRelativePath (animator.transform, animator.GetBoneTransform ((HumanBodyBones)i));

                //pathは階層
                //http://mebiustos.hatenablog.com/entry/2015/09/16/230000
                AnimationUtility.SetEditorCurve (clip,
                    new EditorCurveBinding {
                        path = path,
                        type = typeof (Transform),
                        propertyName = "m_LocalPosition.x"
                    }, positionCurveX);
                AnimationUtility.SetEditorCurve (clip,
                    new EditorCurveBinding {
                        path = path,
                        type = typeof (Transform),
                        propertyName = "m_LocalPosition.y"
                    }, positionCurveY);
                AnimationUtility.SetEditorCurve (clip,
                    new EditorCurveBinding {
                        path = path,
                        type = typeof (Transform),
                        propertyName = "m_LocalPosition.z"
                    }, positionCurveZ);

                AnimationUtility.SetEditorCurve (clip,
                    new EditorCurveBinding {
                        path = path,
                        type = typeof (Transform),
                        propertyName = "m_LocalRotation.x"
                    }, rotationCurveX);
                AnimationUtility.SetEditorCurve (clip,
                    new EditorCurveBinding {
                        path = path,
                        type = typeof (Transform),
                        propertyName = "m_LocalRotation.y"
                    }, rotationCurveY);
                AnimationUtility.SetEditorCurve (clip,
                    new EditorCurveBinding {
                        path = path,
                        type = typeof (Transform),
                        propertyName = "m_LocalRotation.z"
                    }, rotationCurveZ);
                AnimationUtility.SetEditorCurve (clip,
                    new EditorCurveBinding {
                        path = path,
                        type = typeof (Transform),
                        propertyName = "m_LocalRotation.w"
                    }, rotationCurveW);
            }

            clip.EnsureQuaternionContinuity ();

            var filepath = string.Format ("Assets/Resources/RecordMotion_{0:yyyy_MM_dd_HH_mm_ss}_Generic.anim", DateTime.Now);
            var uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath (filepath);

            AssetDatabase.CreateAsset (clip, uniqueAssetPath);
            AssetDatabase.SaveAssets ();
        }

        public void ExporterHumanoidAnimation()
        {
            var path = string.Format ("Assets/Resources/RecordMotion_{0:yyyy_MM_dd_HH_mm_ss}_Humanoid.anim", DateTime.Now);
            path = AssetDatabase.GenerateUniqueAssetPath (path);
            ExportHumanoidAnimation (path);
        }

        public void ExportHumanoidAnimation (string assetPath)
        {
            var clip = new AnimationClip { frameRate = 30 };
            AnimationUtility.SetAnimationClipSettings (clip, new AnimationClipSettings { loopTime = false });


            // body position
            {
                var curveX = new AnimationCurve ();
                var curveY = new AnimationCurve ();
                var curveZ = new AnimationCurve ();
                foreach (var item in Poses) {
                    curveX.AddKey (item.Time, item.BodyPosition.x);
                    curveY.AddKey (item.Time, item.BodyPosition.y);
                    curveZ.AddKey (item.Time, item.BodyPosition.z);
                }

                const string muscleX = "RootT.x";
                clip.SetCurve ("", typeof (Animator), muscleX, curveX);
                const string muscleY = "RootT.y";
                clip.SetCurve ("", typeof (Animator), muscleY, curveY);
                const string muscleZ = "RootT.z";
                clip.SetCurve ("", typeof (Animator), muscleZ, curveZ);
            }
            // Leftfoot position
            {
                var curveX = new AnimationCurve ();
                var curveY = new AnimationCurve ();
                var curveZ = new AnimationCurve ();
                foreach (var item in Poses) {
                    curveX.AddKey (item.Time, item.LeftfootIK_Pos.x);
                    curveY.AddKey (item.Time, item.LeftfootIK_Pos.y);
                    curveZ.AddKey (item.Time, item.LeftfootIK_Pos.z);
                }

                const string muscleX = "LeftFootT.x";
                clip.SetCurve ("", typeof (Animator), muscleX, curveX);
                const string muscleY = "LeftFootT.y";
                clip.SetCurve ("", typeof (Animator), muscleY, curveY);
                const string muscleZ = "LeftFootT.z";
                clip.SetCurve ("", typeof (Animator), muscleZ, curveZ);
            }
            // Rightfoot position
            {
                var curveX = new AnimationCurve ();
                var curveY = new AnimationCurve ();
                var curveZ = new AnimationCurve ();
                foreach (var item in Poses) {
                    curveX.AddKey (item.Time, item.RightfootIK_Pos.x);
                    curveY.AddKey (item.Time, item.RightfootIK_Pos.y);
                    curveZ.AddKey (item.Time, item.RightfootIK_Pos.z);
                }

                const string muscleX = "RightFootT.x";
                clip.SetCurve ("", typeof (Animator), muscleX, curveX);
                const string muscleY = "RightFootT.y";
                clip.SetCurve ("", typeof (Animator), muscleY, curveY);
                const string muscleZ = "RightFootT.z";
                clip.SetCurve ("", typeof (Animator), muscleZ, curveZ);
            }
            // body rotation
            {
                var curveX = new AnimationCurve ();
                var curveY = new AnimationCurve ();
                var curveZ = new AnimationCurve ();
                var curveW = new AnimationCurve ();
                foreach (var item in Poses) {
                    curveX.AddKey (item.Time, item.BodyRotation.x);
                    curveY.AddKey (item.Time, item.BodyRotation.y);
                    curveZ.AddKey (item.Time, item.BodyRotation.z);
                    curveW.AddKey (item.Time, item.BodyRotation.w);
                }

                const string muscleX = "RootQ.x";
                clip.SetCurve ("", typeof (Animator), muscleX, curveX);
                const string muscleY = "RootQ.y";
                clip.SetCurve ("", typeof (Animator), muscleY, curveY);
                const string muscleZ = "RootQ.z";
                clip.SetCurve ("", typeof (Animator), muscleZ, curveZ);
                const string muscleW = "RootQ.w";
                clip.SetCurve ("", typeof (Animator), muscleW, curveW);
            }
            // Leftfoot rotation
            {
                var curveX = new AnimationCurve ();
                var curveY = new AnimationCurve ();
                var curveZ = new AnimationCurve ();
                var curveW = new AnimationCurve ();
                foreach (var item in Poses) {
                    curveX.AddKey (item.Time, item.LeftfootIK_Rot.x);
                    curveY.AddKey (item.Time, item.LeftfootIK_Rot.y);
                    curveZ.AddKey (item.Time, item.LeftfootIK_Rot.z);
                    curveW.AddKey (item.Time, item.LeftfootIK_Rot.w);
                }

                const string muscleX = "LeftFootQ.x";
                clip.SetCurve ("", typeof (Animator), muscleX, curveX);
                const string muscleY = "LeftFootQ.y";
                clip.SetCurve ("", typeof (Animator), muscleY, curveY);
                const string muscleZ = "LeftFootQ.z";
                clip.SetCurve ("", typeof (Animator), muscleZ, curveZ);
                const string muscleW = "LeftFootQ.w";
                clip.SetCurve ("", typeof (Animator), muscleW, curveW);
            }
            // Rightfoot rotation
            {
                var curveX = new AnimationCurve ();
                var curveY = new AnimationCurve ();
                var curveZ = new AnimationCurve ();
                var curveW = new AnimationCurve ();
                foreach (var item in Poses) {
                    curveX.AddKey (item.Time, item.RightfootIK_Rot.x);
                    curveY.AddKey (item.Time, item.RightfootIK_Rot.y);
                    curveZ.AddKey (item.Time, item.RightfootIK_Rot.z);
                    curveW.AddKey (item.Time, item.RightfootIK_Rot.w);
                }

                const string muscleX = "RightFootQ.x";
                clip.SetCurve ("", typeof (Animator), muscleX, curveX);
                const string muscleY = "RightFootQ.y";
                clip.SetCurve ("", typeof (Animator), muscleY, curveY);
                const string muscleZ = "RightFootQ.z";
                clip.SetCurve ("", typeof (Animator), muscleZ, curveZ);
                const string muscleW = "RightFootQ.w";
                clip.SetCurve ("", typeof (Animator), muscleW, curveW);
            }

            // muscles
            for (int i = 0; i < HumanTrait.MuscleCount; i++) {
                var curve = new AnimationCurve ();
                foreach (var item in Poses) {
                    curve.AddKey (item.Time, item.Muscles[i]);
                }

                var muscle = HumanTrait.MuscleName[i];
                if (SequentialMotionSettings.TraitPropMap.ContainsKey (muscle)) {
                    muscle = SequentialMotionSettings.TraitPropMap[muscle];
                }

                clip.SetCurve ("", typeof (Animator), muscle, curve);
            }

            clip.EnsureQuaternionContinuity ();

            AssetDatabase.CreateAsset (clip, assetPath);
            AssetDatabase.SaveAssets ();
        }
#endif

    }

}