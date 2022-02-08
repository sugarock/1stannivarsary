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
    /// IK付きHumanPoseをPlayableに流すAnimationJob
    /// TODO: ウェイト調整に未対応
    /// TODO: 回転を加味したルートモーションは未対応
    /// </summary>
    public struct HumanPosePlayableJob : IAnimationJob
    {
        public NativeArray<MuscleHandle> muscleHandles;
        public NativeArray<float> muscleValues;

        public Vector3 bodyLocalPosition;
        public Quaternion bodyLocalRotation;
        public Vector3 leftFootPosition;
        public Vector3 rightFootPosition;
        public Quaternion leftFootRotation;
        public Quaternion rightFootRotation;

        public Vector3 rootLocalPositionPrev;

        private Vector3 _rootLocalPosition;
        private Vector3 _rootLocalPositionDelta;

        public void ProcessRootMotion(AnimationStream stream)
        {
            var humanStream = stream.AsHuman();

            _rootLocalPosition = new Vector3(bodyLocalPosition.x, 0, bodyLocalPosition.z);
            _rootLocalPositionDelta = _rootLocalPosition - rootLocalPositionPrev;

            if (!Mathf.Approximately(stream.deltaTime, 0))
            {
                stream.velocity = _rootLocalPositionDelta * (1 / stream.deltaTime);
            }

            rootLocalPositionPrev = _rootLocalPosition;
        }

        public void ProcessAnimation(AnimationStream stream)
        {
            var humanStream = stream.AsHuman();
            Debug.Assert (humanStream.isValid);

            for (int i = 0; i < this.muscleValues.Length; i++)
            {
                humanStream.SetMuscle(muscleHandles[i], muscleValues[i]);
            }

            humanStream.bodyLocalPosition = new Vector3(0, bodyLocalPosition.y, 0);
            humanStream.bodyLocalRotation = bodyLocalRotation;

            humanStream.SetGoalLocalPosition(
                AvatarIKGoal.LeftFoot,
                  leftFootPosition
                + leftFootRotation * new Vector3(-humanStream.leftFootHeight / humanStream.humanScale, 0, 0)
                - _rootLocalPosition);
            humanStream.SetGoalLocalRotation(AvatarIKGoal.LeftFoot, leftFootRotation);
            humanStream.SetGoalLocalPosition(
                AvatarIKGoal.RightFoot,
                  rightFootPosition
                + rightFootRotation * new Vector3(-humanStream.rightFootHeight / humanStream.humanScale, 0, 0)
                - _rootLocalPosition);
            humanStream.SetGoalLocalRotation(AvatarIKGoal.RightFoot, rightFootRotation);
            humanStream.SetGoalWeightPosition(AvatarIKGoal.LeftFoot, 1);
            humanStream.SetGoalWeightPosition(AvatarIKGoal.RightFoot, 1);
            humanStream.SetGoalWeightRotation(AvatarIKGoal.LeftFoot, 1);
            humanStream.SetGoalWeightRotation(AvatarIKGoal.RightFoot, 1);

            humanStream.SolveIK();
        }

    }


}