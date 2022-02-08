using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealtimeReflection : MonoBehaviour
{
    // "ReflectionProbe"を格納する変数を宣言
    ReflectionProbe probe;
 
    // ゲームが始まる前の処理（Start関数よりも前に実行される）
    void Awake()
    {
        // 変数"probe"にReflectionProbeコンポーネントを格納
        probe = GetComponent<ReflectionProbe>();
    }
 
    void Update()
    {
        // カメラの座標を参照してReflectionProbeコンポーネントの座標に代入
        // Y座標のみマイナスに変換

	GameObject camera_object = GameObject.Find ("MainCamera");

        probe.transform.position = new Vector3(
 
            camera_object.transform.position.x,
            camera_object.transform.position.y * -1,
            camera_object.transform.position.z
        );
 
        // ReflectionProbeのキューブマップを更新
        probe.RenderProbe();
    }
}