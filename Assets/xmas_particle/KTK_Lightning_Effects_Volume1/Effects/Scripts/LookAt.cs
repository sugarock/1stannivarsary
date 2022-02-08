//======================================
/*
@autor ktk.kumamoto
@date 2015.3.9 create
@note LookAt
*/
//======================================


using UnityEngine;
using System.Collections;


public class LookAt : MonoBehaviour {
	
	public GameObject target;
	
	void LookAt_on () {
		transform.LookAt(target.transform.position);
		
		RaycastHit hit;
		
		if(Physics.Raycast(transform.position, transform.forward, out hit))
		{
			if(hit.collider)
			{
				target.transform.position = hit.point;
				target.SendMessage("EffectOn");

			}
		}
		
	}
}