using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FreeCamera : MonoBehaviour
{
	public float m_MoveSpeed = 0f;
	public float m_RotateSpeed = 0f;
	public KeyCode m_ForwardButton = KeyCode.W;
	public KeyCode m_BackwardButton = KeyCode.S;
	public KeyCode m_RightButton = KeyCode.D;
	public KeyCode m_LeftButton = KeyCode.A;
	public KeyCode m_UpButton = KeyCode.Q;
	public KeyCode m_DownButton = KeyCode.E;
	public bool m_ShowInternalMaps = false;

    void Update ()
    {
        // translation
        {
            Vector3 dir = Vector3.zero;
			Move (m_ForwardButton, ref dir, transform.forward);
			Move (m_BackwardButton, ref dir, -transform.forward);
			Move (m_RightButton, ref dir, transform.right);
			Move (m_LeftButton, ref dir, -transform.right);
			Move (m_UpButton, ref dir, transform.up);
			Move (m_DownButton, ref dir, -transform.up);
			transform.position += dir * m_MoveSpeed * Time.deltaTime;
        }
        // rotation
        {
            if (Input.GetMouseButton (0))
            {
                Vector3 eulerAngles = transform.eulerAngles;
				eulerAngles.x += -Input.GetAxis("Mouse Y") * 359f * m_RotateSpeed;
				eulerAngles.y += Input.GetAxis("Mouse X") * 359f * m_RotateSpeed;
                transform.eulerAngles = eulerAngles;
            }
        }
	}
	void Move (KeyCode key, ref Vector3 moveTo, Vector3 dir)
    {
        if (Input.GetKey (key))
			moveTo = dir;
    }
#if UNITY_EDITOR
	void OnGUI ()
	{
		GUI.Box (new Rect (10, 10, 260, 25), "Realtime Planar Reflections Demo");
		if (m_ShowInternalMaps)
		{
			PlanarReflection[] pr = GameObject.FindObjectsOfType<PlanarReflection> ();
			GUI.DrawTextureWithTexCoords (new Rect (10, 10, 128, 128), pr[0].m_RTReflectionColor, new Rect (0, 0, 1, 1));
			if (pr[0].m_EnableHeightFading)
				GUI.DrawTextureWithTexCoords (new Rect (148, 10, 128, 128), pr[0].m_RTHeightAtten, new Rect (0, 0, 1, 1));
		}
	}
#endif
}
