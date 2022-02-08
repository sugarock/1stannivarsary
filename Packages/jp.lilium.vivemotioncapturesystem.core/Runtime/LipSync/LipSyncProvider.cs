using System;
using System.Linq;
using UnityEngine;

namespace VMCCore.Facial
{
    public static class MicrophoneUtility
    {
        /// <summary>
        /// デバイス名と一致するインデックスを取得する
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        public static int GetIndex(string deviceName)
        {
            var devices = Microphone.devices;
            for (int i = 0; i < devices.Length; i++) {
                if (devices[i] == deviceName) return i;
            }
            return -1;
        }

        /// <summary>
        /// 次のマイクデバイス名を取得する
        /// 現在存在しないデバイス名が入力された場合は０番目のデバイス名を返す。
        /// デバイスが一切存在しない場合は null
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        public static string GetNextDevice(string deviceName)
        {
            var devices = Microphone.devices;
            if (devices.Length == 0) return default (string);

            int index = Mathf.Max(GetIndex (deviceName), 0);

            return devices[(index + 1) % devices.Length];
        }

        /// <summary>
        /// 有効なデバイスかを検査する
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        public static bool ValidatedDevice (string deviceName)
        {
            return GetIndex (deviceName) >= 0;
        }
    }


    /// <summary>
    /// リップシンク解析後の提供をするクラスはこれを実装する
    /// </summary>
    public interface ILipSyncProvider
    {
        float[] visemes { get; }

    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public class LipSyncProvider : OVRLipSyncContextBase, ILipSyncProvider
    {
        private const int kMicFrequency = 44100;
        private const int kLengthSeconds = 1;

        [Tooltip ("ミュート")]
        public bool disableLoopback = true;

        [Tooltip ("音声入力にマイクデイバイスを使用する")]
        public bool useMicrophone = true;

        public int deviceIndex => MicrophoneUtility.GetIndex (_deviceName);

        public string deviceName { 
            get => _deviceName; 
            private set {
                _deviceName = value; 
                _isFaildDevice = false;
            }
        }

        [SerializeField]
        private string _deviceName;

        /// <summary>
        /// 口形の重みと全体ボリューム
        /// { A, I, U, E, O, Volume }
        /// </summary>
        /// <value></value>
        public float[] visemes { get; private set; } = new float[6];

        private AudioClip _microphone;

        private bool isValidSelectedDevice { get { return !string.IsNullOrEmpty (deviceName); } }

        private bool _isFaildDevice = false;

        private int _head = 0;
        private float[] _processBuffer = new float[1024];
        private float[] _microphoneBuffer = new float[kLengthSeconds * kMicFrequency];
        private int _minFreq, _maxFreq;

        private int _micFrequency = kMicFrequency;

        private float _loudness;

        void OnEnable()
        {
        }

        void OnDisable ()
        {
            StopMicrophone ();
        }

        private void Reset ()
        {
            audioSource = GetComponent<AudioSource> ();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        private void OnValidate ()
        {
            //ValidateDevice ();
        }


        /// <summary>
        /// マイクデバイスの状態を更新
        /// </summary>
        public void ValidateDevice ()
        {
            if (!MicrophoneUtility.ValidatedDevice (_deviceName)) {
                _deviceName = default (string);
                _isFaildDevice = false;
            }
        }

        /// <summary>
        /// 次のマイクデバイスを選択
        /// </summary>
        public void SetNextDevice()
        {
            _deviceName = MicrophoneUtility.GetNextDevice (_deviceName);
            _isFaildDevice = false;
            StartMicrophone();
        }


        /// <summary>
        /// Start this instance.
        /// </summary>
        void Start ()
        {
            ValidateDevice ();
            if (audioSource == null) {
                audioSource = GetComponent<AudioSource> ();
            }
        }

        /// <summary>
        /// Update this instance.
        /// </summary>
        void Update ()
        {
            if (_isFaildDevice) return;


            if (useMicrophone) {
                ProcessMicrophoneAudioReadFast ();
            }

            // trap inputs and send signals to phoneme engine for testing purposes
            // get the current viseme frame
            if (Frame != null) {
                _SetVisemeToMorphTarget (Frame, _loudness);
            }
        }


        /// <summary>
        /// Sets the viseme to morph target.
        /// </summary>
        void _SetVisemeToMorphTarget (OVRLipSync.Frame frame, float loudness)
        {
            visemes[0] = frame.Visemes[(int)OVRLipSync.Viseme.aa];
            visemes[1] = frame.Visemes[(int)OVRLipSync.Viseme.ih];
            visemes[2] = frame.Visemes[(int)OVRLipSync.Viseme.ou];
            visemes[3] = frame.Visemes[(int)OVRLipSync.Viseme.E];
            visemes[4] = frame.Visemes[(int)OVRLipSync.Viseme.oh];
            visemes[5] = loudness;
        }

        /// <summary>
        /// Raises the audio filter read event.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="channels">Channels.</param>
        void OnAudioFilterRead (float[] data, int channels)
        {
            // マイクを使わない場合は、ここで解析
            if (!useMicrophone) {
                lock (this) {
                    if (Context != 0) {
                        OVRLipSync.ProcessFrame (Context, data, Frame, channels == 2);
                    }
                }
            }

            if (disableLoopback) {

                // 音がループバックしないように消去
                for (int i = 0; i < data.Length; ++i)
                    data[i] = 0.0f;
            }
        }

        /// <summary>
        /// マイク入力専用音データ取得
        /// OnAudioFilterRead を使うより低遅延
        /// </summary>
        void ProcessMicrophoneAudioReadFast ()
        {
            if (!Microphone.IsRecording (deviceName)) {
                StartMicrophone ();
            }

            var position = Microphone.GetPosition (deviceName);
            if (position < 0 || _head == position) {
                return;
            }

            if (audioSource.clip == null) return;

            float loudness = 0;
            int loudnessSampleCount = 0;
            audioSource.clip.GetData (_microphoneBuffer, 0);
            while (GetDataLength (_microphoneBuffer.Length, _head, position) > _processBuffer.Length) {
                var remain = _microphoneBuffer.Length - _head;
                if (remain < _processBuffer.Length) {
                    Array.Copy (_microphoneBuffer, _head, _processBuffer, 0, remain);
                    Array.Copy (_microphoneBuffer, 0, _processBuffer, remain, _processBuffer.Length - remain);
                }
                else {
                    Array.Copy (_microphoneBuffer, _head, _processBuffer, 0, _processBuffer.Length);
                }


                for (int i = 0; i < _processBuffer.Length; i++) {
                    loudness += Mathf.Abs(_processBuffer[i]);
                }
                loudnessSampleCount += _processBuffer.Length;

                OVRLipSync.ProcessFrame (Context, _processBuffer, Frame, false);
                _head += _processBuffer.Length;
                if (_head > _microphoneBuffer.Length) {
                    _head -= _microphoneBuffer.Length;
                }
            }

            _loudness /= loudnessSampleCount;
        }


        static int GetDataLength (int bufferLength, int head, int tail)
        {
            if (head < tail) {
                return tail - head;
            }
            else {
                return bufferLength - head + tail;
            }
        }

        void _GetMicCaps ()
        {
            if (isValidSelectedDevice == false) return;

            Microphone.GetDeviceCaps (deviceName, out _minFreq, out _maxFreq);

            if (_minFreq == 0 && _maxFreq == 0) {
                Debug.LogWarning ("[VMCCore] min and max frequencies are 0");
                _minFreq = 44100;
                _maxFreq = 44100;
            }
        }

        public void StartMicrophone ()
        {
            if (isValidSelectedDevice == false) return;
            if (_isFaildDevice) return;

            _GetMicCaps ();

            // Starts recording
            _micFrequency = Mathf.Clamp(kMicFrequency, _minFreq, _maxFreq);
            _microphoneBuffer = new float[_micFrequency];

            _microphone = Microphone.Start (deviceName, true, 1, Mathf.Clamp(kMicFrequency, _minFreq, _maxFreq));
            if (_microphone == null) {
                _isFaildDevice = true;
                audioSource.clip = _microphone;
                return;
            }

            _isFaildDevice = false;


            audioSource.clip = _microphone;
            audioSource.loop = true;

            // Wait until the recording has started
            // 入力デバイスによっては無限ループする可能性があるのでコメントアウト
            //while (!(Microphone.GetPosition (selectedDevice) > 0)) { }

            // Play the audio source
            audioSource.Play ();
        }

        public void StopMicrophone ()
        {
            if (isValidSelectedDevice == false) return;

            // Overriden with a clip to play? Don't stop the audio source
            if ((audioSource != null) && (audioSource.clip != null) && (audioSource.clip.name == "Microphone")) {
                audioSource.Stop ();
            }

            Microphone.End (deviceName);
        }



    }

}
