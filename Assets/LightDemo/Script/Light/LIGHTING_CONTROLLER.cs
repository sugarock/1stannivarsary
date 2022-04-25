using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LIGHTING_CONTROLLER : MonoBehaviour
{
    //Target Info
    [SerializeField] private GameObject targetObj = null;
    private Vector3 target_pos = new Vector3();

    [SerializeField] private GameObject[] followLight;
    [SerializeField] private bool enableGroupLighting = false;
    [SerializeField] private bool enableRandomLighting = false;
    //Mine Info
    private GameObject bmLight;

    void Start()
    {
        bmLight = this.gameObject;
        target_pos = targetObj.GetComponent<Transform>().position;
    }

    //compare data
    private Vector3 past_buffor;

    void Update()
    {
        
        target_pos = targetObj.GetComponent<Transform>().position;

        //checking before buffor
        if (target_pos == past_buffor) return;

        Vector3 forward = target_pos - bmLight.transform.position;
        Quaternion quat = Quaternion.LookRotation(forward, Vector3.up);
        Quaternion offset = Quaternion.FromToRotation(Vector3.right, Vector3.forward);
        
        bmLight.transform.rotation = quat * offset;

        //buffor over writing
        past_buffor = target_pos;
        if (enableGroupLighting || followLight != null) FOLLOWLIGHT_DIRECTION();
    }
    private float time = 0;

    private void FixedUpdate()
    {
        time += Time.deltaTime;
        if (enableRandomLighting && time > 2.0f)
        {
            time = 0;
            RANDOM_DIRECTION(); 
        }
    }

    private void FOLLOWLIGHT_DIRECTION()
    {
        for (int i = 0; i < followLight.Length; i++)
        {
            followLight[i].transform.rotation = bmLight.transform.rotation;
        }
    }

    private void RANDOM_DIRECTION()
    {
        //yield return new WaitForSeconds(1);
        float rand_x = Random.Range(-90, 90);
        float rand_y = Random.Range(0, 360);
        float rand_z = Random.Range(0, 180);
        bmLight.transform.rotation = Quaternion.Euler(rand_x, rand_y, rand_z);
    }
}