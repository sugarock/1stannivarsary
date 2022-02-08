/**
 * Reference: https://github.com/duo-inc/EasyMotionRecorder/blob/master/EasyMotionRecorder/Assets/EasyMotionRecorder/Scripts/MotionDataRecorder.cs
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Lilium.FbxExporter;


namespace SequentialMotion
{


    public class SequentialMotionRecorder
    {
        public Animator animator { get; private set; }

        public int FrameIndex { get; private set; }
        public float RecordedTime { get; private set; }

        private HumanBodyBones IK_LeftFootBone = HumanBodyBones.LeftFoot;

        private HumanBodyBones IK_RightFootBone = HumanBodyBones.RightFoot;

        private MotionStreamData _stream;

        private HumanPose _currentPose;
        private HumanPoseHandler _poseHandler;

        public SequentialMotionRecorder (GameObject target)
        {
            animator = target.GetComponent<Animator> ();
            _poseHandler = new HumanPoseHandler (animator.avatar, animator.transform);

            Reset ();
        }

        /// <summary>
        /// 録画開始
        /// </summary>
        public void Reset ()
        {
            _stream = new MotionStreamData ();

            RecordedTime = 0f;
            FrameIndex = 0;

            isRecroding = false;
            currentTime = 0f;
            _bindings.Clear ();
        }

        public void TakeSnapshot (float deltaTime)
        {
            RecordedTime += deltaTime;

            // Humanoid で記録
            _poseHandler.GetHumanPose (ref _currentPose);

            var serializedPose = new MotionPose ();
            serializedPose.BodyRootPosition = animator.transform.localPosition;
            serializedPose.BodyRootRotation = animator.transform.localRotation;
            var bodyTQ = new TQ (_currentPose.bodyPosition, _currentPose.bodyRotation);
            var LeftFootTQ = new TQ (animator.GetBoneTransform (IK_LeftFootBone).position, animator.GetBoneTransform (IK_LeftFootBone).rotation);
            var RightFootTQ = new TQ (animator.GetBoneTransform (IK_RightFootBone).position, animator.GetBoneTransform (IK_RightFootBone).rotation);
            LeftFootTQ = AvatarUtility.GetIKGoalTQ (animator.avatar, animator.humanScale, AvatarIKGoal.LeftFoot, bodyTQ, LeftFootTQ);
            RightFootTQ = AvatarUtility.GetIKGoalTQ (animator.avatar, animator.humanScale, AvatarIKGoal.RightFoot, bodyTQ, RightFootTQ);

            serializedPose.BodyPosition = bodyTQ.t;
            serializedPose.BodyRotation = bodyTQ.q;
            serializedPose.LeftfootIK_Pos = LeftFootTQ.t;
            serializedPose.LeftfootIK_Rot = LeftFootTQ.q;
            serializedPose.RightfootIK_Pos = RightFootTQ.t;
            serializedPose.RightfootIK_Rot = RightFootTQ.q;
            serializedPose.FrameCount = FrameIndex;
            serializedPose.Muscles = new float[_currentPose.muscles.Length];
            serializedPose.Time = RecordedTime;
            for (int i = 0; i < serializedPose.Muscles.Length; i++) {
                serializedPose.Muscles[i] = _currentPose.muscles[i];
            }

            // HumanoidボーンをGenericで記録
            serializedPose.HumanoidBones = new List<MotionPose.HumanoidBone> ();
            SetHumanBoneTransformToHumanoidPoses (animator, ref serializedPose);

            _stream.Add (serializedPose);
            FrameIndex++;
        }

        private static void SetHumanBoneTransformToHumanoidPoses (Animator animator, ref MotionPose pose)
        {
            HumanBodyBones[] values = System.Enum.GetValues (typeof (HumanBodyBones)) as HumanBodyBones[];
            foreach (HumanBodyBones b in values) {
                if (b < 0 || b >= HumanBodyBones.LastBone) {
                    continue;
                }

                Transform t = animator.GetBoneTransform (b);
                if (t != null) {
                    pose.HumanoidBones.Add (new MotionPose.HumanoidBone { 
                        LocalPosition = t.localPosition, 
                        LocalRotation = t.localRotation 
                    });
                }
            }
        }

        protected virtual void WriteAnimationFile ()
        {
#if UNITY_EDITOR
            SafeCreateDirectory ("Assets/Recordings");

            //var path = string.Format ("Assets/Recordings/{0}_{1:yyMMdd_HHmmss}.anim", _animator.name, DateTime.Now);
            var path = string.Format ("Assets/Recordings/{0}_Humanoid.anim", animator.name, System.DateTime.Now);
            var uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath (path);
            _stream.ExportHumanoidAnimation (uniqueAssetPath);

            //AssetDatabase.CreateAsset (Poses, uniqueAssetPath);
            //AssetDatabase.Refresh ();

            RecordedTime = 0f;
            FrameIndex = 0;
#endif
        }

        /// <summary>
        /// 指定したパスにディレクトリが存在しない場合
        /// すべてのディレクトリとサブディレクトリを作成します
        /// </summary>
        public static DirectoryInfo SafeCreateDirectory (string path)
        {
            return Directory.Exists (path) ? null : Directory.CreateDirectory (path);
        }


        public float currentTime { get; private set; } = 0f;

        public bool isRecroding { get; private set; } = false;

        private struct CurveBinding
        {
            public Type type;

            public GameObject target;
            public Func<float> data;
        }

        private List<CurveBinding> _bindings = new List<CurveBinding> ();

        public MotionStreamData data;

        public void SaveToAnimationClip (AnimationClip clip)
        {
            // FBX出力用にAnimationClipのCurve情報を記録する
            //RuntimeAnimationUtility.AddCurveBindings (clip, bindings);
        }

        public void SaveToAnimatonFile (string filePath)
        {

            Debug.Log ($"Save to primitive animation file. file:{filePath}");
        }



        private string MakeRelativePath (Transform root, Transform target)
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
        private static float RawAngleValue (float value)
        {
            if (value > 180) {
                return value - 360;
            }
            return value;
        }

    }


}
