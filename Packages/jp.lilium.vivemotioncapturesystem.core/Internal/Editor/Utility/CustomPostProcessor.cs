using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Lilium.VMCStudio.Editor
{

    /// <summary>
    /// アセットの変更をハンドリングするためのもの
    /// </summary>
    public class CustomPostprocessor : AssetPostprocessor
    {
        /// <summary>
        /// プロジェクト内のアセットに変更があると呼ばれる
        /// </summary>
        public static event System.Action<string[], string[], string[], string[]> onAssetChanged;

        static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            onAssetChanged.Invoke (importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
        }
    }

}