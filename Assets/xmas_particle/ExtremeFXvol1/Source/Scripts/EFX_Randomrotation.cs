﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EFX_Randomrotation : MonoBehaviour
{

	void Start ()
	{
		this.gameObject.transform.rotation = Random.rotation;
	}
}
