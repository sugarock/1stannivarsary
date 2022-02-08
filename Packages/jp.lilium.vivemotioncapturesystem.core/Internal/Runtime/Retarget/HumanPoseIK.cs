using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace VMCCore.Retarget
{

    public struct HumanPoseIK : System.IDisposable
    {
        public HumanPose humanPose;

        public NativeArray<Vector3> goalPositions;
        public NativeArray<Quaternion> goalRotations;
        public NativeArray<Vector3> hintPositions;

        public static HumanPoseIK Create (Allocator allocator)
        {
            var n = new HumanPoseIK ();
            n.goalPositions = new NativeArray<Vector3> (4, allocator, NativeArrayOptions.UninitializedMemory);
            n.goalRotations = new NativeArray<Quaternion> (4, allocator, NativeArrayOptions.UninitializedMemory);
            n.hintPositions = new NativeArray<Vector3> (4, allocator, NativeArrayOptions.UninitializedMemory);
            return n;
        }

        public void Dispose ()
        {
            goalPositions.Dispose ();
            goalRotations.Dispose ();
            hintPositions.Dispose ();
        }
    }

    public class HumanPoseRetargetHandler : System.IDisposable
    {
        public void Dispose ()
        {
        }

        public void Retarget (in HumanPose humanPose, Animator animator, ref HumanPoseIK output)
        {
            var tq = new TQ();
            Retarget(in humanPose, tq, animator, ref output);
        }

        public void Retarget (in HumanPose humanPose, TQ sourceStartTQ, Animator animator, ref HumanPoseIK output)
        {
            var leftFoot = animator.GetBoneTransform (HumanBodyBones.LeftFoot);
            var rightFoot = animator.GetBoneTransform (HumanBodyBones.RightFoot);

            var localBodyPosition = humanPose.bodyPosition;
            var localBodyRotation = humanPose.bodyRotation;

            TQ bodyTQ;
            if (animator.transform.parent != null) {
                bodyTQ = new TQ (
                    animator.transform.parent.rotation * (localBodyPosition * animator.humanScale) + animator.transform.parent.position,
                    animator.transform.parent.rotation * localBodyRotation);
            }
            else {
                bodyTQ = new TQ (localBodyPosition * animator.humanScale, localBodyRotation);
            }
            var leftFootTQ = new TQ (leftFoot.position, leftFoot.rotation);
            var rightFootTQ = new TQ (rightFoot.position, rightFoot.rotation);

            leftFootTQ = AvatarUtility.GetIKGoalTQ (animator.avatar, animator.humanScale, AvatarIKGoal.LeftFoot, ref bodyTQ, ref leftFootTQ);
            rightFootTQ = AvatarUtility.GetIKGoalTQ (animator.avatar, animator.humanScale, AvatarIKGoal.RightFoot, ref bodyTQ, ref rightFootTQ);


            leftFootTQ.t = localBodyRotation * leftFootTQ.t + localBodyPosition;
            leftFootTQ.q = localBodyRotation * leftFootTQ.q;
            rightFootTQ.t = localBodyRotation * rightFootTQ.t + localBodyPosition;
            rightFootTQ.q = localBodyRotation * rightFootTQ.q;


            output.humanPose.bodyPosition = localBodyPosition;
            output.humanPose.bodyRotation = localBodyRotation;
            output.humanPose.muscles = humanPose.muscles;
            output.goalPositions[0] = leftFootTQ.t;
            output.goalPositions[1] = rightFootTQ.t;
            output.goalPositions[2] = Vector3.zero;
            output.goalPositions[3] = Vector3.zero;
            output.goalRotations[0] = leftFootTQ.q;
            output.goalRotations[1] = rightFootTQ.q;
            output.goalRotations[2] = Quaternion.identity;
            output.goalRotations[3] = Quaternion.identity;
        }
    }

}
