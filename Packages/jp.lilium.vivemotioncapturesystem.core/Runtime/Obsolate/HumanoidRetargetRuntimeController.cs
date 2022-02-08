using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VMCCore
{


    [DefaultExecutionOrder (30000)]
    public class HumanoidRetargetRuntimeController : MonoBehaviour
    {
        public Animator source;

        [SerializeField]
        private Animator _target;

        private HumanPose _currentPose;
        private HumanPoseHandler _sourcePoseHandler;
        private HumanPoseHandler _targetPoseHandler;

        private void Awake ()
        {
            Debug.Assert (source != null);

            _currentPose = new UnityEngine.HumanPose ();
        }

        private void Start ()
        {
            SetSource (source);
            SetTarget (_target);
        }

        private void Update ()
        {

            if (_target != null && _targetPoseHandler == null) {
                source.SetIKPositionWeight (AvatarIKGoal.LeftFoot, 1);
                source.SetIKPositionWeight (AvatarIKGoal.RightFoot, 1);
                source.SetIKRotationWeight (AvatarIKGoal.LeftFoot, 1);
                source.SetIKRotationWeight (AvatarIKGoal.RightFoot, 1);
            }
            if (_sourcePoseHandler != null && _targetPoseHandler != null) {
                _sourcePoseHandler.GetHumanPose (ref _currentPose);
                _targetPoseHandler.SetHumanPose (ref _currentPose);
            }
        }

        public void SetSource (Animator source)
        {
            this.source = source;
            _sourcePoseHandler = new HumanPoseHandler (source.avatar, source.transform);
        }

        public void SetTarget (Animator target)
        {
            this._target = target;
            _targetPoseHandler = new HumanPoseHandler (target.avatar, target.transform);
        }



    }

}
