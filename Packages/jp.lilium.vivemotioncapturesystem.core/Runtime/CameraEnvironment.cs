using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Lilium.VMCStudio
{
    [System.Serializable]
    public sealed class CameraData : System.IDisposable
    {
        public CinemachineVirtualCameraBase virtualCamera;

        public Texture2D captureTexture => _captureTexture = _captureTexture != null ? _captureTexture : CreateCaptureTexture ();

        private Texture2D _captureTexture;

        public int drawCount { get; set; }

        public CameraData (CinemachineVirtualCameraBase virtualCamera)
        {
            this.virtualCamera = virtualCamera;
        }

        Texture2D CreateCaptureTexture ()
        {
            Texture2D dest = new Texture2D (320, 180, TextureFormat.RGBA32, false);
            dest.hideFlags = HideFlags.HideAndDontSave;
            dest.Apply (false);
            return dest;
        }

        public void Dispose ()
        {
            if (_captureTexture != null) {
                GameObject.DestroyImmediate (_captureTexture);
            }
        }

        public bool isLive
        {
            get {
                var brain = GameObject.FindObjectOfType<CinemachineBrain> ();
                if (brain == null) return false;
                return brain.IsLive (virtualCamera);
            }
        }

    }

    public sealed class CameraEnvironment : MonoBehaviour
    {
        public List<CameraData> cameras = new List<CameraData> ();
 
        public static CameraEnvironment instance => _GetOrFindInstance ();

        static CameraEnvironment _instance;


        static CameraEnvironment _GetOrFindInstance ()
        {
            if (_instance == null) {
                _instance = FindObjectOfType<CameraEnvironment>();
            }
            return _instance;
        }

        public CameraData FindCameraData (CinemachineVirtualCameraBase virtualCamera)
        {
            return cameras.Where (t => t.virtualCamera == virtualCamera).FirstOrDefault ();
        }


        private void Update() 
        {
            // カメラ選択ショートカットキー Digit1 ~ 0
            // 実行中のみ有効
            for (int i = 0; i < Mathf.Min(instance.cameras.Count(), 10); i++) {
                var camera = instance.cameras[i];
                if (camera.virtualCamera == null) continue;

                if (Keyboard.current[Key.Digit1+i].wasPressedThisFrame ) {
                    camera.virtualCamera.MoveToTopOfPrioritySubqueue();
                }
            }
        }

    }


}
