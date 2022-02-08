using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Networking.Transport;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using VMCCore.Facial;
using VMCCore.Retarget;
using VMCCore.Network;

namespace VMCCore
{
    public struct InputFloat
    {
        public NativeString32 key;
        public float value;
    }

    public struct InputFloat2
    {
        public NativeString32 key;
        public float2 value;
    }

    public struct MotionFacial
    {
        public ushort facial;

        public ushort variation;

        public float weight;
    }



    public struct MotionPose : System.IDisposable
    {
        public bool isCreated => muscles.IsCreated;

        public const int HumanTraitMuscleCount = 95;
        public const int HumanFacialExpressionsCount = 16;

        public Vector3 bodyPosition;

        public Quaternion bodyRotation;

        public NativeArray<float> muscles;

        public NativeArray<Vector3> goalPositions;

        public NativeArray<Quaternion> goalRotations;

        public NativeArray<MotionFacial> facials;

        public NativeArray<InputFloat> inputsFloat;

        public NativeArray<float> visemes;

        public static MotionPose Craete (Allocator allocator)
        {
            var v = new MotionPose ();
            v.muscles = new NativeArray<float> (HumanTraitMuscleCount, allocator, NativeArrayOptions.ClearMemory);
            v.goalPositions = new NativeArray<Vector3> (4, allocator, NativeArrayOptions.ClearMemory);
            v.goalRotations = new NativeArray<Quaternion> (4, allocator, NativeArrayOptions.ClearMemory);
            v.facials = new NativeArray<MotionFacial> (HumanFacialExpressionsCount, allocator, NativeArrayOptions.ClearMemory);
            v.inputsFloat = new NativeArray<InputFloat> (HumanFacialExpressionsCount, allocator, NativeArrayOptions.ClearMemory);
            v.visemes = new NativeArray<float>(6, allocator, NativeArrayOptions.ClearMemory);
            return v;
        }

        public void Dispose()
        {
            this.muscles.Dispose ();
            this.facials.Dispose ();
            this.goalPositions.Dispose ();
            this.goalRotations.Dispose ();
            this.inputsFloat.Dispose ();
            this.visemes.Dispose();
        }
    }

    public class MotionProvider : MonoBehaviour, INetworkReactor
    {
        [SerializeField]
        private string _address = "127.0.0.1";

        [SerializeField]
        private ushort _port = 3333;

        public MotionPose motionPose => _motionPose;

        private MotionPose _motionPose;

        private TransportClient _client;


        private float retryTime = 1;

        private void OnEnable ()
        {
            _client = new TransportClient ();
            _client.listerner = this;


            _client.Connect (_address, _port);
        }

        private void OnDisable ()
        {
            _client.Disconnect ();
            _client.Dispose ();

            if (_motionPose.isCreated) {
                _motionPose.Dispose ();
            }
        }

        void Start()
        {
        }


        public void OnDestroy ()
        {
        }

        bool _isRecieved = false;

        void Update ()
        {
            if (!_client.isConnected) {
                retryTime -= Time.deltaTime;
                if (retryTime < 0) {
                    _client.Connect (_address, _port);
                    retryTime = 1;
                }
            }

            _client.Update ();
            _isRecieved = false;
        }


        void INetworkReactor.OnAccepted (NetworkConnection connection)
        {
        }

        void INetworkReactor.OnReceived (NetworkConnection connection, DataStreamReader stream)
        {
            if (!_motionPose.isCreated) {
                _motionPose = MotionPose.Craete (Allocator.Persistent);
            }

            stream.ReadStruct (ref _motionPose.bodyPosition);
            stream.ReadStruct (ref _motionPose.bodyRotation);
            stream.ReadNativeArray (_motionPose.muscles);
            stream.ReadNativeArray (_motionPose.goalPositions);
            stream.ReadNativeArray (_motionPose.goalRotations);
            stream.ReadNativeArray (_motionPose.facials);
            stream.ReadNativeArray (_motionPose.inputsFloat);
            stream.ReadNativeArray (_motionPose.visemes);
            _isRecieved = true;

            // 自動切断対策
            var writer = _client.driver.BeginSend (connection);
            writer.WriteInt (0);
            _client.driver.EndSend (writer);

        }

        void INetworkReactor.OnDisconnected (NetworkConnection connection)
        {
        }
    }


}

