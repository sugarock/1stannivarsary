using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealtimeReflection : MonoBehaviour
{
    // "ReflectionProbe"���i�[����ϐ���錾
    ReflectionProbe probe;
 
    // �Q�[�����n�܂�O�̏����iStart�֐������O�Ɏ��s�����j
    void Awake()
    {
        // �ϐ�"probe"��ReflectionProbe�R���|�[�l���g���i�[
        probe = GetComponent<ReflectionProbe>();
    }
 
    void Update()
    {
        // �J�����̍��W���Q�Ƃ���ReflectionProbe�R���|�[�l���g�̍��W�ɑ��
        // Y���W�̂݃}�C�i�X�ɕϊ�

	GameObject camera_object = GameObject.Find ("MainCamera");

        probe.transform.position = new Vector3(
 
            camera_object.transform.position.x,
            camera_object.transform.position.y * -1,
            camera_object.transform.position.z
        );
 
        // ReflectionProbe�̃L���[�u�}�b�v���X�V
        probe.RenderProbe();
    }
}