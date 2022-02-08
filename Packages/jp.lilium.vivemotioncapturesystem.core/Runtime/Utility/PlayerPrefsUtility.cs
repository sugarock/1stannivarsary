using UnityEngine;

namespace VMCCore
{

    public static class PlayerPrefsUtility
    {
        struct Container<T>
        {
            public T value;
        }


        /// <summary>
        /// 指定されたオブジェクトの情報を保存します
        /// </summary>
        public static void SetObject<T> (string key, in T value)
        {
            var container = new Container<T> { value = value };
            var json = JsonUtility.ToJson (container);
            PlayerPrefs.SetString (key, json);
        }

        /// <summary>
        /// 指定されたオブジェクトの情報を読み込みます
        /// </summary>
        public static void GetObject<T> (string key, out T value)
        {
            var json = PlayerPrefs.GetString (key);
            var container = JsonUtility.FromJson<Container<T>> (json);
            value = container.value;
        }

        /// <summary>
        /// 指定されたオブジェクトの情報を読み込みます
        /// </summary>
        public static void TryGetObject<T> (string key, ref T value)
        {
            if (!PlayerPrefs.HasKey (key)) return;

            var json = PlayerPrefs.GetString (key);
            var container = JsonUtility.FromJson<Container<T>> (json);
            value = container.value;
        }

    }

}