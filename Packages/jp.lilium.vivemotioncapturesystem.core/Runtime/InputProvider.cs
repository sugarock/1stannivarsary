using System.Collections.Generic;

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Collections;



namespace VMCCore
{
    public enum InputKeybindValueType
    {
        Unknown,
        Single,
        Vector2
    }

    /// <summary>
    /// 入力名とその値を定義するクラスはこれを継承する
    /// </summary>
    public interface IInputValue
    {
        string name { get; }

        InputKeybindValueType valueType { get; }

        T GetValue<T> () where T : struct;
    }


    /// <summary>
    /// 入力情報を提供するクラスはこれを実装する
    /// </summary>
    public interface IInputProvider 
    {
        IList<IInputValue> GetValues ();

        bool TryGetValue<T> (string key, out T value) where T : struct;
    }



    public class InputProvider : MonoBehaviour, IInputProvider
    {
        [System.Serializable]
        private class InputKeybind : IInputValue
        {
            [SerializeField]
            public string name { get => _name; set => _name = value; }

            [SerializeField]
            private string _name;

            public InputKeybindValueType valueType { get => _valueType; set => _valueType = value; }

            [SerializeField]
            private InputKeybindValueType _valueType;

            [SerializeField]
            public InputAction input;

            public T GetValue<T> () where T : struct
            {
                if (input == null) return default (T);
                return input.ReadValue<T> ();
            }

            public string bindingDisplayName => input?.bindings.Select (t => t.ToDisplayString ()).SingleOrDefault ();

            public void AssigneKey (string binding, InputKeybindValueType valueType)
            {
                this.valueType = valueType;
                this.input = new InputAction (_name, InputActionType.Value, binding);
                this.input.Enable ();
            }
        }


        [Serializable]
        struct SaveModel
        {
            public List<InputKeybind> keybinds;
        }

        [SerializeField, SerializeReference]
        public List<IInputValue> keybinds = new List<IInputValue> ();

        private void OnEnable()
        {
            if (PlayerPrefs.HasKey ("InputProvider")) {
                var saveModel = new SaveModel ();
                PlayerPrefsUtility.GetObject ("InputProvider", out saveModel);
                keybinds.AddRange (saveModel.keybinds.Select (t => t as IInputValue).ToList ());

                EnableInputs ();
            }
        }

        private void OnDisable ()
        {
            PlayerPrefsUtility.SetObject ("InputProvider", new SaveModel { keybinds = this.keybinds.Select (t => t as InputKeybind).ToList () }); ;
        }

        public void SetName (IInputValue keybind, string newName)
        {
            ((InputKeybind)keybind).name = newName;
        }


        public string GetBindingDisplayName (IInputValue keybind)
        {
            return ((InputKeybind)keybind).bindingDisplayName;
        }

        public InputKeybindValueType GetValueType (IInputValue keybind)
        {
            return ((InputKeybind)keybind).valueType;
        }

        [ContextMenu("Add Keybind")]
        public IInputValue AddKeybind ()
        {
            var newKeybind = new InputKeybind { input = new InputAction(" " + keybinds.Count) };
            keybinds.Add (newKeybind);
            return newKeybind;
        }

        public void AssigneKey (IInputValue input, string binding, InputKeybindValueType valueType)
        {
            ((InputKeybind)input).AssigneKey (binding, valueType);
        }

        public void RemoveKeybind (IInputValue bind)
        {
            keybinds.Remove (bind);
        }

        public void RemoveKeybindAt (int index)
        {
            keybinds.RemoveAt (index);
        }

        public static InputKeybindValueType ToValueType (System.Type type)
        {
            if (type == typeof (Vector2)) {
                return InputKeybindValueType.Vector2;
            }
            else if (type == typeof (Single)) {
                return InputKeybindValueType.Single;
            }
            return InputKeybindValueType.Unknown;
        }

        public IList<IInputValue> GetValues ()
        {
            return keybinds;
        }

        public bool TryGetValue<T> (string key, out T value) where T : struct
        {
            foreach (var keybind in keybinds) {
                if (keybind.name == key) {
                    value = keybind.GetValue<T> ();
                    return true;
                }
            }
            value = default (T);
            return false;
        }


        public void EnableInputs ()
        {
            foreach (var keybind in keybinds) {
                ((InputKeybind)keybind).input?.Enable ();
            }
        }
        public void DisableInputs ()
        {
            foreach (var keybind in keybinds) {
                ((InputKeybind)keybind).input?.Disable ();
            }
        }

    }

}
