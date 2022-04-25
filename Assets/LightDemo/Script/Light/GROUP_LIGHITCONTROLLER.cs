using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GROUP_LIGHITCONTROLLER : MonoBehaviour
{
    //direction check
    [Header("リーダーライト(必須)")]
    [SerializeField] private GameObject leaderLight = null;
    [Header("ターゲット")]
    [SerializeField] private GameObject targetObj = null;
    //follower objects
    [Header("リーダーへ従属させるライト")]
    [SerializeField] private GameObject[] followLight;
    
    [Header("ターゲットへ回転")]
    [SerializeField] private bool target_rot = false;
    //random check
    [Header("ランダムに回転")]
    [SerializeField] private bool random_rot = false;
    [Header("ライトの速度")]
    [SerializeField, Range(0f, 2f)] private float h_speed = 0.25f;
    //random attrivite
    private float interpolant = 50f;
    Quaternion[] startRot;
    Quaternion[] targetRot;
    Quaternion der_quta = new Quaternion();
    private float[] sec;

    //rotation buffor
    private Quaternion pastrot_buffor = new Quaternion();

    // Start is called before the first frame update
    void Start()
    {
        startRot = new Quaternion[followLight.Length];
        targetRot = new Quaternion[followLight.Length];
        sec = new float[followLight.Length];

        for (int i = 0; i < followLight.Length; i++)
        {
            startRot[i] = followLight[i].transform.rotation;
            targetRot[i] = Quaternion.AngleAxis(90f, Vector3.up) * followLight[i].transform.rotation;
            sec[i] = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        FOLLOWLIGHT_DIRECTION();
        if (random_rot && !target_rot) random_rot = true;
        if (random_rot && !target_rot) RANDOM_ROTATE_LIGHT();
        if (target_rot) LEADERLIGHT_DIRECTION();
    }

    private Vector3 pastpos_buffor;

    private void LEADERLIGHT_DIRECTION()
    {
       Vector3 target_pos = targetObj.GetComponent<Transform>().position;

        //checking before buffor
        if (target_pos == pastpos_buffor) return;

        Vector3 forward = target_pos - leaderLight.transform.position;
        Quaternion quat = Quaternion.LookRotation(forward, Vector3.up);
        Quaternion offset = Quaternion.FromToRotation(Vector3.right, Vector3.forward);
        
        leaderLight.transform.rotation = quat * offset;

        //position buffor over writing
        pastpos_buffor = target_pos;
    }

    //group direction
    private void FOLLOWLIGHT_DIRECTION()
    {
        if (leaderLight.transform.rotation == pastrot_buffor) return;

        for (int i = 0; i < followLight.Length; i++)
        {
            followLight[i].transform.rotation = leaderLight.transform.rotation;
        }

        pastrot_buffor = leaderLight.transform.rotation;
    }

    private void RANDOM_ROTATE_LIGHT()
    {
        for (int i = 0; i < followLight.Length; i++)
        {
            sec[i] += Time.deltaTime;
    
            followLight[i].transform.rotation = Quaternion.Lerp(startRot[i], DERIVATE_ROTATE(targetRot[i]), Mathf.Sin(Time.deltaTime) * interpolant);
            //followLight[i].transform.rotation = ROTATION_RANGE(followLight[i].transform.rotation);
            
            startRot[i] = followLight[i].transform.rotation;
            if (sec[i] > 0.5f)
            {
                targetRot[i] = RANDOM_DIRECTION();
                sec[i] = 0;
            }
        }
    }

    //random direction
    //private void RANDOMLIGHT_DIRECTION()
    //{
    //    float rand_x, rand_y, rand_z;
    //    for (int i = 0; i < followLight.Length; i++)
    //    {
    //        rand_x = Random.Range(-90, 90);
    //        rand_y = Random.Range(0, 360);
    //        rand_z = Random.Range(0, 180);
    //        followLight[i].transform.rotation = Quaternion.Euler(rand_x, rand_y, rand_z);
    //    }
    //}

    private Quaternion DERIVATE_ROTATE(Quaternion rot)
    {
        der_quta.x = (Mathf.Pow(rot.x + h_speed / 10, 2) - Mathf.Pow(rot.x, 2)) * h_speed / 10 * 5;
        der_quta.y = (Mathf.Pow(rot.y + h_speed / 10, 2) - Mathf.Pow(rot.y, 2)) * h_speed / 10 * 5;
        der_quta.z = (Mathf.Pow(rot.z + h_speed / 10, 2) - Mathf.Pow(rot.z, 2)) * h_speed / 10 * 5;
        return der_quta;
    }

    //回転の限度
    private Quaternion ROTATION_RANGE(Quaternion rangeRot)
    {
        //Quaternion rangeRot = q;
        rangeRot.x = Mathf.Clamp(transform.rotation.x, -90f, 90f);
        rangeRot.y = Mathf.Clamp(transform.rotation.y, 0f, 360f);
        rangeRot.z = Mathf.Clamp(transform.rotation.z, 0f, 180f);
        return rangeRot;
    }

    //ランダムな回転生成
    private Quaternion RANDOM_DIRECTION()
    {
        float rand_x = Random.Range(-90, 90);
        float rand_y = Random.Range(0, 360);
        float rand_z = Random.Range(0, 180);
        return Quaternion.Euler(rand_x, rand_y, rand_z);
    }
}
