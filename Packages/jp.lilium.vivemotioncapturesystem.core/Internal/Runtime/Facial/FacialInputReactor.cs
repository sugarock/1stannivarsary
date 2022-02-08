#pragma warning disable 0414, 0649
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VMCCore.Facial
{
    public class FacialInputReactor : MonoBehaviour
    {
        public Face face;

        public InputActionAsset inputActionAsset;


        void Reset()
        {
            face = GetComponent<Face>();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        private void OnEnable() 
        {
            if (inputActionAsset != null) {
                inputActionAsset.Enable();
            }
        }

        private void OnDisable() 
        {
        }

        void OnValidate() 
        {
            
        }


        // Update is called once per frame
        void Update()
        {
            if (face == null) return;
            if (inputActionAsset == null) return;

            var inputActionMap = inputActionAsset.actionMaps[0];

            foreach (var action in inputActionMap) {
                var faceKey = new FaceKey(action.name);
                if (faceKey.preset == FacePreset.Unknown) continue;

                face.SetEasingValue(new FaceKey(action.name), action.ReadValue<float>());
            }

            var eyesAction = inputActionMap.FindAction("Eyes");

            if (eyesAction != null) {
                var eyes = eyesAction.ReadValue<Vector2>();
                face.SetEasingValue( new FaceKey("EyesX", 0), eyes.x);
                face.SetEasingValue( new FaceKey("EyesY", 0), eyes.y);
            }

        }
       
    }
    
}
