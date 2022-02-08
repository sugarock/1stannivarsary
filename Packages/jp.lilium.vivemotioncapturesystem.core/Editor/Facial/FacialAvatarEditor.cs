using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using VMCCore.Facial;

namespace VMCCore.Editor.Public
{
    //TODO: OdinInspectorを使うように。
    [CustomEditor (typeof (FacialAvatar), true)]
    class FacialAvatarEditor : VMCCore.Editor.FacialAvatarEditor
    {
    }

}
