using UnityEngine;
using System.Collections;
 
public class setProbePosition: MonoBehaviour
{

    [SerializeField]
    public GameObject plane;//���ʔ��˂��s��Plane
    private ReflectionProbe probe;
    private Transform trfMainCam;
    private Transform trfPlane;

    void Start()
    {
        probe = gameObject.GetComponent<ReflectionProbe>();
	GameObject camera_object = GameObject.Find ("MainCamera");
        trfMainCam = camera_object.transform;
    }

    void Update()
    {
        //�����������o
        float _diffDistance = GetDiffDistance();
        SetPosition(_diffDistance);
    }

    private void SetPosition(float _diffDist)
    {
        probe.transform.position = new Vector3(trfMainCam.position.x, trfMainCam.position.y - (_diffDist * 2.0f), trfMainCam.position.z);
    }

    private float GetDiffDistance()
    {
        float _dist =  Mathf.Abs(trfMainCam.position.y - plane.transform.position.y);
        if (trfMainCam.position.y < plane.transform.position.y)
        {
            //�J��������ɕ��ʂ��������ꍇ��probe�Ƃ̈ʒu���t�]���邽�߂̑΍�
            _dist *= -1.0f;
        }

        return _dist;
    }
}