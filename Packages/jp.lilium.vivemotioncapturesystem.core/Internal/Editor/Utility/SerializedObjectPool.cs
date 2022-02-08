using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Lilium.VMCStudio.Editor;

namespace Lilium.VMCStudio.Editor 
{
    public static class SerializedObjectPool
    {
        private static Dictionary<Object, SerializedObject> _objects = new Dictionary<Object, SerializedObject> ();

        public static SerializedObject GetOrCreate (Object key)
        {
            Debug.Assert (key != null);
            Debug.Assert (_objects != null);

            SerializedObject serializedObj;
            if (_objects.TryGetValue( key, out serializedObj)) {
                return serializedObj;
            }
            else {
                serializedObj = new SerializedObject (key);
                _objects.Add (key, serializedObj);
                return serializedObj;
            }
        }

        public static bool Remove (Object key)
        {
            return _objects.Remove (key);
        }

        // 消去したオブジェクトのSerializedObjectを削除
        public static void Collect ()
        {
            foreach (var key in _objects.Keys.ToArray ().Where (key => key == null)) {
                _objects.Remove (key);
            }
        }


        [RuntimeInitializeOnLoadMethod]
        public static void Clear ()
        {
            _objects = new Dictionary<Object, SerializedObject> ();
        }
    }

}