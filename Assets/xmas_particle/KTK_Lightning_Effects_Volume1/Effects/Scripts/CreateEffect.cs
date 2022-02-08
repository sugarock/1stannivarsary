using UnityEngine;
using System.Collections;

public class CreateEffect : MonoBehaviour {

	public GameObject[] Effect;
	public GameObject Eff_Shot;
	public int num = 0;

	void Start () {
		Eff_Shot = Effect[0];
	}

	void SetEff_Change () {
		num ++;
		if(Effect.Length == num) {
			num = 0;
		}
		Eff_Shot = Effect[num];
	}

	void EffectOn () {
		Instantiate (Eff_Shot, transform.position, Quaternion.identity);
	}
	
}
