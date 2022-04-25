using UnityEngine;

public class RotateTest4 : MonoBehaviour
{
    private float angle = 90f;
    Vector3 axis = Vector3.up;
    [SerializeField] float interpolant = 50f;

    Quaternion startRot;
    Quaternion targetRot;

    Quaternion der_quta = new Quaternion();
    Quaternion rangerota = new Quaternion();

    float sec = 0;

    // Start is called before the first frame update
    void Start()
    {
        startRot = transform.rotation;
        targetRot = Quaternion.AngleAxis(angle, axis) * transform.rotation;
        rangerota = startRot;
    }

    void Update()
    {
        sec += Time.deltaTime;
        //transform.rotation = Quaternion.RotateTowards(startRot, targetRot,interpolant*sec);
        //transform.rotation = Quaternion.Lerp(startRot, targetRot, Mathf.Sin(Time.deltaTime) * interpolant);

        transform.rotation = Quaternion.Lerp(startRot, DERIVATE_ROTATE(targetRot), Mathf.Sin(Time.deltaTime) * interpolant);
        ROTATION_RANGE();
        startRot = transform.rotation;
        if (sec > 0.6f)
        {
            targetRot = RANDOM_DIRECTION();
            sec = 0;
        }
    }

    //private Quaternion derivative(Quaternion rot, Quaternion target)
    //{
    //    Quaternion der_quta = new Quaternion();
    //    float h = 0.025f;
    //    der_quta.x = (Mathf.Pow(rot.x + target.x, 2) - Mathf.Pow(rot.x, 2)) * 5;
    //    der_quta.y = (Mathf.Pow(rot.y + target.y, 2) - Mathf.Pow(rot.y, 2)) * 5;
    //    der_quta.z = (Mathf.Pow(rot.z + target.z, 2) - Mathf.Pow(rot.z, 2)) * 5;
    //    return der_quta;
    //}
    
    //回転を滑らかにしてみたかったので微分？をした。
    private Quaternion DERIVATE_ROTATE(Quaternion rot)
    {
        float h = 0.025f;
        der_quta.x = (Mathf.Pow(rot.x + h,2) - Mathf.Pow(rot.x, 2))*h*5;
        der_quta.y = (Mathf.Pow(rot.y + h,2) - Mathf.Pow(rot.y, 2))*h*5;
        der_quta.z = (Mathf.Pow(rot.z + h,2) - Mathf.Pow(rot.z, 2))*h*5;
        return der_quta;
    }

    //回転の限度
    private void ROTATION_RANGE()
    {
        rangerota.x = Mathf.Clamp(transform.rotation.x, -90f, 90f);
        rangerota.y = Mathf.Clamp(transform.rotation.y, 0f, 360f);
        rangerota.z = Mathf.Clamp(transform.rotation.z, 0f, 180f);
        transform.rotation = rangerota;
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