using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VMCCore.Facial;

namespace VMCStudioEditor
{


    public static class VMCMenuItem
    {

        [MenuItem ("GameObject/VMCStudio/LipSync Provider")]
        public static void CreateLipSyncProvider ()
        {
            var lsp = new GameObject ("LipSync Provider").AddComponent<LipSyncProvider> ();
            lsp.useMicrophone = false;
            lsp.disableLoopback = false;
        }
    }

}