using System.Net;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using VMCCore.Facial;
using VMCCore.Retarget;
using VMCCore.Network;

namespace VMCCore
{

    /// <summary>
    /// モーション送信
    /// </summary>
    /// すべてのプロパティが反映した後に送信しないといけないため、最後に実行するようにオーダーを設定
    [DefaultExecutionOrder (30000)]
    public class MotionSender : MonoBehaviour, INetworkReactor
    {
        public enum TrackOffsets
        {
            ApplyTransformOffset,
            ApplySceneOffset
        }

        public TrackOffsets trackOffsets = TrackOffsets.ApplyTransformOffset;

        public InputProvider inputProvider;

        public LipSyncProvider lipSyncProvider;

        // broadcast address
        public ushort port = 3333;

        private bool _connected = false;
        private HumanPoseHandler _humanPoseHandler;
        private Avatar _currentAvatar;
        private HumanPose _currentPose;

        private HumanPoseRetargetHandler _retargetHandler;

        private TransportServer server;

        public Animator animator
        {
            get { return _animator = _animator != null ? _animator : GetComponent<Animator> (); }
        }
        Animator _animator;

        public Face facer
        {
            get { return _facer = _facer != null ? _facer : GetComponent<Face> (); }
        }
        Face _facer;

        private TQ _sourceStartTQ;

        private void OnEnable ()
        {
            Debug.Assert (animator != null);
            Debug.Assert (animator.avatar != null);

            server = new TransportServer ();
            server.listerner = this;

            _currentPose = new HumanPose ();
            _retargetHandler = new HumanPoseRetargetHandler ();

            server.Listen (port);
        }

        private void OnDisable ()
        {
            if (server.isListening) {
                server.Close ();
            }
            server.Dispose ();

            _retargetHandler.Dispose ();
            _humanPoseHandler.Dispose ();

            _BuildHuman ();
        }

        void Start()
        {
            if (trackOffsets == TrackOffsets.ApplyTransformOffset) {
                _sourceStartTQ = new TQ (Vector3.zero, Quaternion.identity);
            }
            else {
                _sourceStartTQ = new TQ (animator.transform.position, animator.transform.rotation);
            }
        }

        public void OnDestroy ()
        {
        }

        void LateUpdate ()
        {
            if (_currentAvatar != animator.avatar) {
                _BuildHuman ();
            }

            _humanPoseHandler.GetHumanPose (ref _currentPose);

            _UpdateMotion ();

            server.Update ();
        }

        private void _BuildHuman ()
        {
            Debug.Assert (animator != null);

            _humanPoseHandler = new HumanPoseHandler (animator.avatar, animator.transform);
            _currentAvatar = animator.avatar;
        }

        unsafe void _UpdateMotion ()
        {

            HumanPose humanPose = new HumanPose ();
            _humanPoseHandler.GetHumanPose (ref humanPose);

            HumanPoseIK retargetedHumanPose = HumanPoseIK.Create (Allocator.Temp);
            _retargetHandler.Retarget (humanPose, animator, ref retargetedHumanPose);

            var motionPose = MotionPose.Craete (Allocator.Temp);

            // 姿勢
            motionPose.bodyPosition = retargetedHumanPose.humanPose.bodyPosition;
            motionPose.bodyRotation = retargetedHumanPose.humanPose.bodyRotation;
            motionPose.muscles.CopyFrom (retargetedHumanPose.humanPose.muscles);
            motionPose.goalPositions.CopyFrom (retargetedHumanPose.goalPositions);
            motionPose.goalRotations.CopyFrom (retargetedHumanPose.goalRotations);

            // リップシンク
            if (lipSyncProvider != null) {
                motionPose.visemes.CopyFrom(lipSyncProvider.visemes);
            }

            // 入力
            if (inputProvider != null) {
                int i = 0;
                foreach (var input in inputProvider.GetValues ()) {
                    if (input.valueType == InputKeybindValueType.Single) {
                        var value = input.GetValue<float> ();
                        if (i < MotionPose.HumanFacialExpressionsCount) {
                            motionPose.inputsFloat[i++] = new InputFloat { key = input.name, value = value };
                        }
                    }
                    else if (input.valueType == InputKeybindValueType.Vector2) {
                        var value = input.GetValue<Vector2> ();
                        if (i < MotionPose.HumanFacialExpressionsCount-1) {
                            //TODO: GC対策 文字連結
                            motionPose.inputsFloat[i++] = new InputFloat { key = input.name + "X", value = value.x };
                            motionPose.inputsFloat[i++] = new InputFloat { key = input.name + "Y", value = value.y };
                        }
                    }
                }

                if (i > MotionPose.HumanFacialExpressionsCount) {
                    Debug.LogWarning ("[VMCCore] Number of active facial expressions exceeded range.");
                }
            }

            // 表情
            if (facer != null) {
                int num = 0;
                var facialData = FacePose.Create ();
                facer.CopyTo (ref facialData);
                for (var i = 0; i < FaceDefine.LengthRigs; i++) {
                    var weight = facialData.rigs[i];

                    if (weight == 0) continue;

                    var key = new FaceKey ((ushort)i + FaceDefine.MinRigs);

                    motionPose.facials[num] = new MotionFacial { facial = (ushort)key.preset, variation = key.variation, weight = weight };
                    num++;
                    if (num >= MotionPose.HumanFacialExpressionsCount) {
                        Debug.LogWarning ("[VMCCore] Number of active facial expressions exceeded range.");
                        break;
                    }
                }

                for (var i = 0; i < FaceDefine.LengthExpressions; i++) {
                    var weight = facialData.expressions[i];

                    if (weight == 0) continue;

                    var key = new FaceKey ((ushort)i + FaceDefine.MinExpressions);

                    motionPose.facials[num] = new MotionFacial { facial = (ushort)key.preset, variation = key.variation, weight = weight };
                    num++;
                    if (num >= MotionPose.HumanFacialExpressionsCount) {
                        Debug.LogWarning ("[VMCCore] Number of active facial expressions exceeded range.");
                        break;
                    }
                }

                facialData.Dispose ();
            }

            // 入力内容を表情に反映
            for (int i = 0; i < MotionPose.HumanFacialExpressionsCount; i++) {
                var input = motionPose.inputsFloat[i];

                if (input.key.LengthInBytes == 0) continue;

                var faceKey = new FaceKey (input.key);
                if (faceKey.preset == FacePreset.Unknown) continue;

                facer.SetValue (faceKey, input.value);
            }

            if (!_connected) return;


            foreach (var connection in server.connections) {
                if (connection == default (NetworkConnection)) continue;

                var writer = server.driver.BeginSend (NetworkPipeline.Null, connection);
                writer.WriteStruct (ref motionPose.bodyPosition);
                writer.WriteStruct (ref motionPose.bodyRotation);
                writer.WriteNativeArray (motionPose.muscles);
                writer.WriteNativeArray (motionPose.goalPositions);
                writer.WriteNativeArray (motionPose.goalRotations);
                writer.WriteNativeArray (motionPose.facials);
                writer.WriteNativeArray (motionPose.inputsFloat);
                writer.WriteNativeArray (motionPose.visemes);
                server.driver.EndSend (writer);
            }
        }



        void INetworkReactor.OnAccepted (NetworkConnection connection)
        {
            _connected = true;
        }

        void INetworkReactor.OnReceived (NetworkConnection connection, DataStreamReader stream)
        {
        }

        void INetworkReactor.OnDisconnected (NetworkConnection connection)
        {
            _connected = false;
        }

        public void Listen ()
        {
            server.Close ();
            server.Dispose ();

            server = new TransportServer ();
            server.listerner = this;
            server.Listen (port);
        }
    }
}

