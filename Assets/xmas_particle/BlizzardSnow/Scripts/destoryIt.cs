using UnityEngine;
using System.Collections;

public class destoryIt : MonoBehaviour {

	private float life = 4;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		life -= Time.deltaTime;
		if (life < 0) {
			Destroy (gameObject );
		}
	
	}
}
