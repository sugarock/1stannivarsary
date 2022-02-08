//======================================
/*
@autor ktk.kumamoto
@date 2015.3.9 create
@note MouseClickPoint MoveTarget
*/
//======================================
using UnityEngine;
using System.Collections;

public class MoveTarget : MonoBehaviour {
	public Transform target;
	public GameObject Ray_obj;
	
	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Vector3 vec = Input.mousePosition;
			vec.z = 10f;
			
			target.position = GetComponent<Camera>().ScreenToWorldPoint(vec);

			Ray_obj.SendMessage("LookAt_on");
		}

	}
}